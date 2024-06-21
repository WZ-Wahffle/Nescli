using System.Diagnostics.CodeAnalysis;
using System.Threading.Channels;

namespace Nescli;

/// <summary>
/// Represents the 6502 CPU inside an NES, whose only way of
/// interfacing with the outside world is over the address and data lines,
/// emulated by the MemoryController member
/// </summary>
public class Cpu
{
    private readonly MemoryController _mc;
    private Channel<InterruptSource> _channel;
    public ushort Pc { get; set; }
    private ushort OldPc { get; set; }
    public byte A { get; private set; }
    public byte X { get; private set; }
    public byte Y { get; private set; }
    public byte S { get; private set; }
    public byte P { get; private set; }

    /// <summary>
    /// Memory handling is managed through constructor injection,
    /// since the CPU is entirely unable to communicate with
    /// the outside world otherwise
    /// </summary>
    /// <param name="mc">The memory map to assign to the processor</param>
    /// <param name="channel">A reference to a channel to receive interrupts from</param>
    public Cpu(MemoryController mc, Channel<InterruptSource> channel)
    {
        _channel = channel;
        _mc = mc;
    }

    /// <summary>
    /// Wrapper around reading from the bus, since some addresses have exceptional behaviour
    /// </summary>
    /// <param name="position">Value on the address bus</param>
    /// <returns>Value received on data bus</returns>
    public byte Read(ushort position)
    {
        return _mc.Read(position);
    }

    /// <summary>
    /// Wrapper around writing to the bus, since some addresses have exceptional behaviour
    /// </summary>
    /// <param name="position">Value on address bus</param>
    /// <param name="value">Value on data bus</param>
    public void Write(ushort position, byte value)
    {
        // OAMDMA workaround, must be caught here since the PPU has no way of requesting it itself
        if (position == 0x4014)
        {
            for (var i = 0; i < 0x100; i++)
            {
                _mc.Write(0x2004, _mc.Read((ushort)((value << 8) + i)));
            }

            return;
        }

        _mc.Write(position, value);
    }

    /// <summary>
    /// Describes the different interrupts the NES can handle
    /// </summary>
    public enum InterruptSource
    {
        Irq,
        Reset,
        Nmi,
        Abort,
        Brk
    }

    /// <summary>
    /// Handles all interrupts, both software and hardware
    /// </summary>
    /// <param name="src">The source of the interrupt</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if invalid src value is provided</exception>
    private void Interrupt(InterruptSource src)
    {
        PushToStack((byte)(OldPc >> 8));
        PushToStack((byte)(OldPc & 0xff));
        PushToStack(P);
        SetStatusBit(StatusBits.NotIrqDisable, true);

        var resetVector = src switch
        {
            InterruptSource.Irq => new Tuple<ushort, ushort>(0xffff, 0xfffe),
            InterruptSource.Brk => new Tuple<ushort, ushort>(0xffff, 0xfffe),
            InterruptSource.Reset => new Tuple<ushort, ushort>(0xfffd, 0xfffc),
            InterruptSource.Nmi => new Tuple<ushort, ushort>(0xfffb, 0xfffa),
            InterruptSource.Abort => new Tuple<ushort, ushort>(0xfff9, 0xfff8),
            _ => throw new ArgumentOutOfRangeException(nameof(src), src, null)
        };

        Pc = (ushort)((Read(resetVector.Item1) << 8) | Read(resetVector.Item2));
    }

    /// <summary>
    /// Fetches, decodes and executes a single instruction
    /// </summary>
    [SuppressMessage("ReSharper.DPA", "DPA0000: DPA issues")]
    public void Run()
    {
        OldPc = Pc;
        var unresolvedOp = Read(Pc++);
        var opcode = Decoder.Decode(unresolvedOp);
        var extraBytes = new byte[Decoder.ResolveRemainingBytes(opcode.Item2)];
        for (var i = 0; i < extraBytes.Length; i++)
        {
            extraBytes[i] = Read(Pc++);
        }

        var instruction = new Instruction(opcode.Item1, opcode.Item2, extraBytes);
        try
        {
            if (_channel.Reader.TryRead(out InterruptSource source))
            {
                Interrupt(source);
            }
            else
            {
                Execute(instruction);
            }
        }
        catch (Exception e)
        {
            throw new Exception($"ProgramCounter: 0x{Pc:x}", e);
        }
    }

    /// <summary>
    /// Performs the actions associated with an instruction
    /// </summary>
    /// <param name="ins">Instruction to execute</param>
    /// <exception cref="IllegalAddressModeException">Thrown if address mode in instruction is undefined for opcode</exception>
    public void Execute(Instruction ins)
    {
        switch (ins.Op)
        {
            case Opcode.Adc:
                switch (ins.AddressMode)
                {
                    case AddressMode.Immediate:
                    case AddressMode.Absolute:
                    case AddressMode.ZeroPage:
                    case AddressMode.IndexedIndirect:
                    case AddressMode.IndirectIndexed:
                    case AddressMode.IndexedZeroPageX:
                    case AddressMode.IndexedAbsoluteX:
                    case AddressMode.IndexedAbsoluteY:
                    case AddressMode.ZeroPageIndirect:
                        var result = ResolveAddressRead(ins) + A + (GetStatusBit(StatusBits.Carry) ? 1 : 0);
                        SetStatusBit(StatusBits.Overflow, result is > 255 or < 0);
                        SetStatusBit(StatusBits.Negative, (result & 0b10000000) != 0);
                        SetStatusBit(StatusBits.Carry, result > 255);
                        SetStatusBit(StatusBits.Zero, result % 256 == 0);
                        A = (byte)result;
                        break;
                    default:
                        throw new IllegalAddressModeException(ins);
                }

                break;
            case Opcode.And:
                switch (ins.AddressMode)
                {
                    case AddressMode.Immediate:
                    case AddressMode.Absolute:
                    case AddressMode.ZeroPage:
                    case AddressMode.IndirectIndexed:
                    case AddressMode.IndexedIndirect:
                    case AddressMode.IndexedZeroPageX:
                    case AddressMode.IndexedAbsoluteX:
                    case AddressMode.IndexedAbsoluteY:
                    case AddressMode.ZeroPageIndirect:
                        A &= ResolveAddressRead(ins);
                        SetStatusBit(StatusBits.Zero, A == 0);
                        SetStatusBit(StatusBits.Negative, (A & 0b10000000) != 0);
                        break;
                    default:
                        throw new IllegalAddressModeException(ins);
                }

                break;
            case Opcode.Asl:
                switch (ins.AddressMode)
                {
                    case AddressMode.Accumulator:
                        SetStatusBit(StatusBits.Carry, (A & 0b10000000) != 0);
                        A <<= 1;
                        SetStatusBit(StatusBits.Negative, (A & 0b10000000) != 0);
                        SetStatusBit(StatusBits.Zero, A == 0);
                        break;
                    case AddressMode.Absolute:
                    case AddressMode.ZeroPage:
                    case AddressMode.IndexedZeroPageX:
                    case AddressMode.IndexedAbsoluteX:
                        SetStatusBit(StatusBits.Carry, (ResolveAddressRead(ins) & 0b10000000) != 0);
                        Write(ResolveAddressWrite(ins), (byte)(ResolveAddressRead(ins) << 1));
                        SetStatusBit(StatusBits.Negative, (ResolveAddressRead(ins) & 0b10000000) != 0);
                        SetStatusBit(StatusBits.Zero, ResolveAddressRead(ins) == 0);
                        break;
                    default:
                        throw new IllegalAddressModeException(ins);
                }

                break;
            case Opcode.Bcc:
                if (ins.AddressMode != AddressMode.Relative)
                {
                    throw new IllegalAddressModeException(ins);
                }

                if (GetStatusBit(StatusBits.Carry) == false)
                {
                    Pc = ResolveAddressWrite(ins);
                }

                break;
            case Opcode.Bcs:
                if (ins.AddressMode != AddressMode.Relative)
                {
                    throw new IllegalAddressModeException(ins);
                }

                if (GetStatusBit(StatusBits.Carry))
                {
                    Pc = ResolveAddressWrite(ins);
                }

                break;
            case Opcode.Beq:
                if (ins.AddressMode != AddressMode.Relative)
                {
                    throw new IllegalAddressModeException(ins);
                }

                if (GetStatusBit(StatusBits.Zero))
                {
                    Pc = ResolveAddressWrite(ins);
                }

                break;
            case Opcode.Bit:
                switch (ins.AddressMode)
                {
                    case AddressMode.Immediate:
                    case AddressMode.Absolute:
                    case AddressMode.ZeroPage:
                    case AddressMode.IndexedZeroPageX:
                    case AddressMode.IndexedAbsoluteX:
                        var result = ResolveAddressRead(ins) & A;
                        SetStatusBit(StatusBits.Zero, result == 0);
                        SetStatusBit(StatusBits.Negative, (result & 0b10000000) != 0);
                        SetStatusBit(StatusBits.Overflow, (result & 0b1000000) != 0);
                        break;
                    default:
                        throw new IllegalAddressModeException(ins);
                }

                break;
            case Opcode.Bmi:
                if (ins.AddressMode != AddressMode.Relative)
                {
                    throw new IllegalAddressModeException(ins);
                }

                if (GetStatusBit(StatusBits.Negative))
                {
                    Pc = ResolveAddressWrite(ins);
                }

                break;
            case Opcode.Bne:
                if (ins.AddressMode != AddressMode.Relative)
                {
                    throw new IllegalAddressModeException(ins);
                }

                if (GetStatusBit(StatusBits.Zero) == false)
                {
                    Pc = ResolveAddressWrite(ins);
                }

                break;
            case Opcode.Bpl:
                if (ins.AddressMode != AddressMode.Relative)
                {
                    throw new IllegalAddressModeException(ins);
                }

                if (GetStatusBit(StatusBits.Negative) == false)
                {
                    Pc = ResolveAddressWrite(ins);
                }

                break;
            case Opcode.Bra:
                if (ins.AddressMode != AddressMode.Relative)
                {
                    throw new IllegalAddressModeException(ins);
                }

                Pc = ResolveAddressWrite(ins);

                break;
            case Opcode.Brk:
                throw new NotImplementedException(ins.ToString());
                break;
            case Opcode.Bvc:
                throw new NotImplementedException(ins.ToString());
                break;
            case Opcode.Bvs:
                throw new NotImplementedException(ins.ToString());
                break;
            case Opcode.Clc:
                if (ins.AddressMode != AddressMode.Implied)
                {
                    throw new IllegalAddressModeException(ins);
                }

                SetStatusBit(StatusBits.Carry, false);
                break;
            case Opcode.Cld:
                if (ins.AddressMode != AddressMode.Implied)
                {
                    throw new IllegalAddressModeException(ins);
                }

                SetStatusBit(StatusBits.DecimalMode, false);
                break;
            case Opcode.Cli:
                throw new NotImplementedException(ins.ToString());
                break;
            case Opcode.Clv:
                throw new NotImplementedException(ins.ToString());
                break;
            case Opcode.Cmp:
                switch (ins.AddressMode)
                {
                    case AddressMode.Immediate:
                    case AddressMode.Absolute:
                    case AddressMode.ZeroPage:
                    case AddressMode.IndexedIndirect:
                    case AddressMode.IndirectIndexed:
                    case AddressMode.IndexedZeroPageX:
                    case AddressMode.IndexedAbsoluteX:
                    case AddressMode.IndexedAbsoluteY:
                    case AddressMode.ZeroPageIndirect:
                        var cmp = ResolveAddressRead(ins);
                        if (A < cmp)
                        {
                            SetStatusBit(StatusBits.Negative, ((A - cmp) & 0x80) != 0);
                            SetStatusBit(StatusBits.Zero, false);
                            SetStatusBit(StatusBits.Carry, false);
                        }

                        if (A == cmp)
                        {
                            SetStatusBit(StatusBits.Negative, false);
                            SetStatusBit(StatusBits.Zero, true);
                            SetStatusBit(StatusBits.Carry, true);
                        }

                        if (A < cmp)
                        {
                            SetStatusBit(StatusBits.Negative, ((A - cmp) & 0x80) != 0);
                            SetStatusBit(StatusBits.Zero, false);
                            SetStatusBit(StatusBits.Carry, true);
                        }

                        break;
                    default:
                        throw new IllegalAddressModeException(ins);
                }

                break;
            case Opcode.Cpx:
                switch (ins.AddressMode)
                {
                    case AddressMode.Immediate:
                    case AddressMode.Absolute:
                    case AddressMode.ZeroPage:
                        var cmp = ResolveAddressRead(ins);
                        if (X < cmp)
                        {
                            SetStatusBit(StatusBits.Negative, ((X - cmp) & 0x80) != 0);
                            SetStatusBit(StatusBits.Zero, false);
                            SetStatusBit(StatusBits.Carry, false);
                        }

                        if (X == cmp)
                        {
                            SetStatusBit(StatusBits.Negative, false);
                            SetStatusBit(StatusBits.Zero, true);
                            SetStatusBit(StatusBits.Carry, true);
                        }

                        if (X < cmp)
                        {
                            SetStatusBit(StatusBits.Negative, ((X - cmp) & 0x80) != 0);
                            SetStatusBit(StatusBits.Zero, false);
                            SetStatusBit(StatusBits.Carry, true);
                        }

                        break;
                    default:
                        throw new IllegalAddressModeException(ins);
                }

                break;
            case Opcode.Cpy:
                switch (ins.AddressMode)
                {
                    case AddressMode.Immediate:
                    case AddressMode.Absolute:
                    case AddressMode.ZeroPage:
                        var cmp = ResolveAddressRead(ins);
                        if (Y < cmp)
                        {
                            SetStatusBit(StatusBits.Negative, ((Y - cmp) & 0x80) != 0);
                            SetStatusBit(StatusBits.Zero, false);
                            SetStatusBit(StatusBits.Carry, false);
                        }

                        if (Y == cmp)
                        {
                            SetStatusBit(StatusBits.Negative, false);
                            SetStatusBit(StatusBits.Zero, true);
                            SetStatusBit(StatusBits.Carry, true);
                        }

                        if (Y < cmp)
                        {
                            SetStatusBit(StatusBits.Negative, ((Y - cmp) & 0x80) != 0);
                            SetStatusBit(StatusBits.Zero, false);
                            SetStatusBit(StatusBits.Carry, true);
                        }

                        break;
                    default:
                        throw new IllegalAddressModeException(ins);
                }

                break;
            case Opcode.Dec:
                switch (ins.AddressMode)
                {
                    case AddressMode.Absolute:
                    case AddressMode.ZeroPage:
                    case AddressMode.IndexedZeroPageX:
                    case AddressMode.IndexedAbsoluteX:
                        var result = ResolveAddressRead(ins) - 1;
                        SetStatusBit(StatusBits.Negative, result < 0);
                        SetStatusBit(StatusBits.Zero, result == 0);
                        Write(ResolveAddressWrite(ins), (byte)result);
                        break;
                    default:
                        throw new IllegalAddressModeException(ins);
                }

                break;
            case Opcode.Dex:
                if (ins.AddressMode != AddressMode.Implied)
                {
                    throw new IllegalAddressModeException(ins);
                }

                X--;
                SetStatusBit(StatusBits.Zero, X == 0);
                SetStatusBit(StatusBits.Negative, X >> 7 != 0);
                break;
            case Opcode.Dey:
                if (ins.AddressMode != AddressMode.Implied)
                {
                    throw new IllegalAddressModeException(ins);
                }

                Y--;
                SetStatusBit(StatusBits.Zero, Y == 0);
                SetStatusBit(StatusBits.Negative, Y >> 7 != 0);
                break;
            case Opcode.Eor:
                switch (ins.AddressMode)
                {
                    case AddressMode.Immediate:
                    case AddressMode.Absolute:
                    case AddressMode.ZeroPage:
                    case AddressMode.IndexedIndirect:
                    case AddressMode.IndirectIndexed:
                    case AddressMode.IndexedZeroPageX:
                    case AddressMode.IndexedAbsoluteX:
                    case AddressMode.IndexedAbsoluteY:
                    case AddressMode.ZeroPageIndirect:
                        A ^= ResolveAddressRead(ins);
                        SetStatusBit(StatusBits.Zero, A == 0);
                        SetStatusBit(StatusBits.Negative, (A & 0b10000000) != 0);
                        break;
                    default:
                        throw new IllegalAddressModeException(ins);
                }

                break;
            case Opcode.Inc:
                switch (ins.AddressMode)
                {
                    case AddressMode.Absolute:
                    case AddressMode.ZeroPage:
                    case AddressMode.IndexedZeroPageX:
                    case AddressMode.IndexedAbsoluteX:
                        var addr = ResolveAddressWrite(ins);
                        var value = (byte)(Read(addr) + 1);
                        Write(addr, value);
                        SetStatusBit(StatusBits.Zero, value == 0);
                        SetStatusBit(StatusBits.Negative, (value & 0b10000000) != 0);
                        break;
                    default:
                        throw new IllegalAddressModeException(ins);
                }

                break;
            case Opcode.Inx:
                if (ins.AddressMode != AddressMode.Implied)
                {
                    throw new IllegalAddressModeException(ins);
                }

                X++;
                SetStatusBit(StatusBits.Negative, (X & 0b10000000) != 0);
                SetStatusBit(StatusBits.Zero, X == 0);

                break;
            case Opcode.Iny:
                if (ins.AddressMode != AddressMode.Implied)
                {
                    throw new IllegalAddressModeException(ins);
                }

                Y++;
                SetStatusBit(StatusBits.Negative, (Y & 0b10000000) != 0);
                SetStatusBit(StatusBits.Zero, Y == 0);
                break;
            case Opcode.Jmp:
                switch (ins.AddressMode)
                {
                    case AddressMode.Absolute:
                    case AddressMode.AbsoluteIndirect:
                    case AddressMode.AbsoluteIndexedIndirect:
                        Pc = ResolveAddressWrite(ins);
                        break;
                    default:
                        throw new IllegalAddressModeException(ins);
                }

                break;
            case Opcode.Jsr:
                if (ins.AddressMode != AddressMode.Absolute)
                {
                    throw new IllegalAddressModeException(ins);
                }

                PushToStack((byte)(Pc >> 8));
                PushToStack((byte)(Pc & 0xff));
                Pc = ResolveAddressWrite(ins);
                break;
            case Opcode.Lda:
                switch (ins.AddressMode)
                {
                    case AddressMode.Immediate:
                    case AddressMode.Absolute:
                    case AddressMode.ZeroPage:
                    case AddressMode.IndexedIndirect:
                    case AddressMode.IndirectIndexed:
                    case AddressMode.IndexedZeroPageX:
                    case AddressMode.IndexedAbsoluteX:
                    case AddressMode.IndexedAbsoluteY:
                    case AddressMode.ZeroPageIndirect:
                        A = ResolveAddressRead(ins);
                        SetStatusBit(StatusBits.Zero, A == 0);
                        SetStatusBit(StatusBits.Negative, A > 127);
                        break;
                    default:
                        throw new IllegalAddressModeException(ins);
                }

                break;
            case Opcode.Ldx:
                switch (ins.AddressMode)
                {
                    case AddressMode.Immediate:
                    case AddressMode.Absolute:
                    case AddressMode.ZeroPage:
                    case AddressMode.IndexedZeroPageY:
                    case AddressMode.IndexedAbsoluteY:
                        X = ResolveAddressRead(ins);
                        SetStatusBit(StatusBits.Zero, X == 0);
                        SetStatusBit(StatusBits.Negative, X > 127);
                        break;
                    default:
                        throw new IllegalAddressModeException(ins);
                }

                break;
            case Opcode.Ldy:
                switch (ins.AddressMode)
                {
                    case AddressMode.Immediate:
                    case AddressMode.Absolute:
                    case AddressMode.ZeroPage:
                    case AddressMode.IndexedZeroPageX:
                    case AddressMode.IndexedAbsoluteX:
                        Y = ResolveAddressRead(ins);
                        SetStatusBit(StatusBits.Zero, X == 0);
                        SetStatusBit(StatusBits.Negative, X > 127);
                        break;
                    default:
                        throw new IllegalAddressModeException(ins);
                }

                break;
            case Opcode.Lsr:
                switch (ins.AddressMode)
                {
                    case AddressMode.Accumulator:
                        SetStatusBit(StatusBits.Carry, (A & 0b1) != 0);
                        A >>= 1;
                        SetStatusBit(StatusBits.Negative, false);
                        SetStatusBit(StatusBits.Zero, A == 0);
                        break;
                    case AddressMode.Absolute:
                    case AddressMode.ZeroPage:
                    case AddressMode.IndexedZeroPageX:
                    case AddressMode.IndexedAbsoluteX:
                        SetStatusBit(StatusBits.Carry, (ResolveAddressRead(ins) & 0b1) != 0);
                        Write(ResolveAddressWrite(ins), (byte)(ResolveAddressRead(ins) >> 1));
                        SetStatusBit(StatusBits.Negative, false);
                        SetStatusBit(StatusBits.Zero, ResolveAddressRead(ins) == 0);
                        break;
                    default:
                        throw new IllegalAddressModeException(ins);
                }

                break;
            case Opcode.Nop:
                throw new NotImplementedException(ins.ToString());
                break;
            case Opcode.Ora:
                switch (ins.AddressMode)
                {
                    case AddressMode.Immediate:
                    case AddressMode.Absolute:
                    case AddressMode.ZeroPage:
                    case AddressMode.IndexedIndirect:
                    case AddressMode.IndirectIndexed:
                    case AddressMode.IndexedZeroPageX:
                    case AddressMode.IndexedAbsoluteX:
                    case AddressMode.IndexedAbsoluteY:
                    case AddressMode.ZeroPageIndirect:
                        A |= ResolveAddressRead(ins);
                        SetStatusBit(StatusBits.Zero, A == 0);
                        SetStatusBit(StatusBits.Negative, (A & 0b10000000) != 0);
                        break;
                    default:
                        throw new IllegalAddressModeException(ins);
                }

                break;
            case Opcode.Pha:
                if (ins.AddressMode != AddressMode.Implied)
                {
                    throw new IllegalAddressModeException(ins);
                }

                PushToStack(A);
                break;
            case Opcode.Php:
                throw new NotImplementedException(ins.ToString());
                break;
            case Opcode.Phx:
                throw new NotImplementedException(ins.ToString());
                break;
            case Opcode.Phy:
                throw new NotImplementedException(ins.ToString());
                break;
            case Opcode.Pla:
                if (ins.AddressMode != AddressMode.Implied)
                {
                    throw new IllegalAddressModeException(ins);
                }

                A = PopFromStack();
                SetStatusBit(StatusBits.Negative, (A & 0b10000000) != 0);
                SetStatusBit(StatusBits.Zero, A == 0);
                break;
            case Opcode.Plp:
                throw new NotImplementedException(ins.ToString());
                break;
            case Opcode.Plx:
                throw new NotImplementedException(ins.ToString());
                break;
            case Opcode.Ply:
                throw new NotImplementedException(ins.ToString());
                break;
            case Opcode.Rol:
                switch (ins.AddressMode)
                {
                    case AddressMode.Accumulator:
                        byte newA = (byte)(GetStatusBit(StatusBits.Carry) ? 1 : 0);
                        newA |= (byte)(A << 1);
                        SetStatusBit(StatusBits.Carry, (A & 0b10000000) != 0);
                        A = newA;
                        SetStatusBit(StatusBits.Negative, (A & 0b10000000) != 0);
                        SetStatusBit(StatusBits.Zero, A == 0);
                        break;
                    case AddressMode.Absolute:
                    case AddressMode.ZeroPage:
                    case AddressMode.IndexedZeroPageX:
                    case AddressMode.IndexedAbsoluteX:
                        byte newVal = (byte)(GetStatusBit(StatusBits.Carry) ? 1 : 0);
                        newVal |= (byte)(ResolveAddressRead(ins) << 1);
                        SetStatusBit(StatusBits.Carry, (ResolveAddressRead(ins) & 0b10000000) != 0);
                        Write(ResolveAddressWrite(ins), newVal);
                        SetStatusBit(StatusBits.Negative, (ResolveAddressRead(ins) & 0b10000000) != 0);
                        SetStatusBit(StatusBits.Zero, ResolveAddressRead(ins) == 0);
                        break;
                    default:
                        throw new IllegalAddressModeException(ins);
                }

                break;
            case Opcode.Ror:
                switch (ins.AddressMode)
                {
                    case AddressMode.Accumulator:
                        byte newA = (byte)(GetStatusBit(StatusBits.Carry) ? 0b10000000 : 0);
                        newA |= (byte)(A >> 1);
                        SetStatusBit(StatusBits.Carry, (A & 1) != 0);
                        A = newA;
                        SetStatusBit(StatusBits.Negative, (A & 0b10000000) != 0);
                        SetStatusBit(StatusBits.Zero, A == 0);
                        break;
                    case AddressMode.Absolute:
                    case AddressMode.ZeroPage:
                    case AddressMode.IndexedZeroPageX:
                    case AddressMode.IndexedAbsoluteX:
                        byte newVal = (byte)(GetStatusBit(StatusBits.Carry) ? 0b10000000 : 0);
                        newVal |= (byte)(ResolveAddressRead(ins) >> 1);
                        SetStatusBit(StatusBits.Carry, (ResolveAddressRead(ins) & 1) != 0);
                        Write(ResolveAddressWrite(ins), newVal);
                        SetStatusBit(StatusBits.Negative, (ResolveAddressRead(ins) & 0b10000000) != 0);
                        SetStatusBit(StatusBits.Zero, ResolveAddressRead(ins) == 0);
                        break;
                    default:
                        throw new IllegalAddressModeException(ins);
                }

                break;
            case Opcode.Rti:
                if (ins.AddressMode != AddressMode.Implied)
                {
                    throw new IllegalAddressModeException(ins);
                }

                P = PopFromStack();
                var lb2 = PopFromStack();
                var hb2 = PopFromStack();
                Pc = (ushort)((hb2 << 8) | lb2);
                break;
            case Opcode.Rts:
                if (ins.AddressMode != AddressMode.Implied)
                {
                    throw new IllegalAddressModeException(ins);
                }

                var lb = PopFromStack();
                var hb = PopFromStack();
                Pc = (ushort)((hb << 8) | lb);
                break;
            case Opcode.Sbc:
                switch (ins.AddressMode)
                {
                    case AddressMode.Immediate:
                    case AddressMode.Absolute:
                    case AddressMode.ZeroPage:
                    case AddressMode.IndexedIndirect:
                    case AddressMode.IndirectIndexed:
                    case AddressMode.IndexedZeroPageX:
                    case AddressMode.IndexedAbsoluteX:
                    case AddressMode.IndexedAbsoluteY:
                    case AddressMode.ZeroPageIndirect:
                        int result = A - ResolveAddressRead(ins) - (GetStatusBit(StatusBits.Carry) ? 0 : 1);
                        SetStatusBit(StatusBits.Zero, result == 0);
                        SetStatusBit(StatusBits.Carry, result >= 0);
                        SetStatusBit(StatusBits.Overflow, result < -128);
                        SetStatusBit(StatusBits.Negative, result < 0);
                        Write(ResolveAddressWrite(ins), (byte)result);
                        break;
                    default:
                        throw new IllegalAddressModeException(ins);
                }

                break;
            case Opcode.Sec:
                if (ins.AddressMode != AddressMode.Implied)
                {
                    throw new IllegalAddressModeException(ins);
                }

                SetStatusBit(StatusBits.Carry, true);
                break;
            case Opcode.Sed:
                throw new NotImplementedException(ins.ToString());
                break;
            case Opcode.Sei:
                if (ins.AddressMode != AddressMode.Implied)
                {
                    throw new IllegalAddressModeException(ins);
                }

                SetStatusBit(StatusBits.NotIrqDisable, true);
                break;
            case Opcode.Sta:
                switch (ins.AddressMode)
                {
                    case AddressMode.Absolute:
                    case AddressMode.ZeroPage:
                    case AddressMode.IndexedIndirect:
                    case AddressMode.IndirectIndexed:
                    case AddressMode.IndexedZeroPageX:
                    case AddressMode.IndexedAbsoluteX:
                    case AddressMode.IndexedAbsoluteY:
                    case AddressMode.ZeroPageIndirect:
                        Write(ResolveAddressWrite(ins), A);
                        break;
                    default:
                        throw new IllegalAddressModeException(ins);
                }

                break;
            case Opcode.Stx:
                switch (ins.AddressMode)
                {
                    case AddressMode.Absolute:
                    case AddressMode.ZeroPage:
                    case AddressMode.IndexedZeroPageY:
                        Write(ResolveAddressWrite(ins), X);
                        break;
                    default:
                        throw new IllegalAddressModeException(ins);
                }

                break;
            case Opcode.Sty:
                switch (ins.AddressMode)
                {
                    case AddressMode.Absolute:
                    case AddressMode.ZeroPage:
                    case AddressMode.IndexedZeroPageX:
                        Write(ResolveAddressWrite(ins), Y);
                        break;
                    default:
                        throw new IllegalAddressModeException(ins);
                }

                break;
            case Opcode.Stz:
                throw new NotImplementedException(ins.ToString());
                break;
            case Opcode.Tax:
                if (ins.AddressMode != AddressMode.Implied)
                {
                    throw new IllegalAddressModeException(ins);
                }

                X = A;
                SetStatusBit(StatusBits.Zero, X == 0);
                SetStatusBit(StatusBits.Negative, (X & 0b10000000) != 0);

                break;
            case Opcode.Tay:
                if (ins.AddressMode != AddressMode.Implied)
                {
                    throw new IllegalAddressModeException(ins);
                }

                Y = A;
                SetStatusBit(StatusBits.Zero, Y == 0);
                SetStatusBit(StatusBits.Negative, (Y & 0b10000000) != 0);
                break;
            case Opcode.Trb:
                throw new NotImplementedException(ins.ToString());
                break;
            case Opcode.Tsb:
                throw new NotImplementedException(ins.ToString());
                break;
            case Opcode.Tsx:
                throw new NotImplementedException(ins.ToString());
                break;
            case Opcode.Txa:
                if (ins.AddressMode != AddressMode.Implied)
                {
                    throw new IllegalAddressModeException(ins);
                }

                A = X;
                SetStatusBit(StatusBits.Zero, A == 0);
                SetStatusBit(StatusBits.Negative, (A & 0b10000000) != 0);

                break;
            case Opcode.Txs:
                if (ins.AddressMode != AddressMode.Implied)
                {
                    throw new IllegalAddressModeException(ins);
                }

                S = X;
                break;
            case Opcode.Tya:
                if (ins.AddressMode != AddressMode.Implied)
                {
                    throw new IllegalAddressModeException(ins);
                }

                A = Y;
                SetStatusBit(StatusBits.Zero, A == 0);
                SetStatusBit(StatusBits.Negative, (A & 0b10000000) != 0);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    /// Converts a combination of address mode and values into a read value
    /// </summary>
    /// <param name="ins">The instruction to operate on</param>
    /// <returns>The value to operate further with. Normally 8-bit unsigned, but the jump instructions return an address</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if an addressing mode is provided that doesn't fit with reading</exception>
    private byte ResolveAddressRead(Instruction ins)
    {
        return ins.AddressMode switch
        {
            AddressMode.Immediate => ins.ExtraBytes[0],
            AddressMode.Absolute => Read((ushort)(ins.ExtraBytes[0] | (ins.ExtraBytes[1] << 8))),
            AddressMode.ZeroPage => Read(ins.ExtraBytes[0]),
            AddressMode.Accumulator => A,
            AddressMode.IndexedIndirect => Read((ushort)(Read((byte)(ins.ExtraBytes[0] + X)) |
                                                         (Read((byte)(ins.ExtraBytes[0] + X + 1)) << 8))),
            AddressMode.IndirectIndexed => Read((ushort)(Read(ins.ExtraBytes[0]) +
                                                         (Read((ushort)(ins.ExtraBytes[0] + 1)) << 8) + Y)),
            AddressMode.IndexedZeroPageX => Read((ushort)(ins.ExtraBytes[0] + X)),
            AddressMode.IndexedZeroPageY => Read((ushort)(ins.ExtraBytes[0] + Y)),
            AddressMode.IndexedAbsoluteX => Read((ushort)((ins.ExtraBytes[0] | (ins.ExtraBytes[1] << 8)) + X)),
            AddressMode.IndexedAbsoluteY => Read((ushort)((ins.ExtraBytes[0] | (ins.ExtraBytes[1] << 8)) + Y)),
            AddressMode.ZeroPageIndirect => Read((ushort)(Read(ins.ExtraBytes[0]) |
                                                          (Read((ushort)(ins.ExtraBytes[0] + 1)) << 8))),
            _ => throw new IllegalAddressModeException(ins)
        };
    }

    /// <summary>
    /// Converts a combination of address mode and values into a target address for an instruction
    /// </summary>
    /// <param name="ins">The instruction to operate on</param>
    /// <returns>The address, assuming the 6502's 16-bit memory space</returns>
    /// <exception cref="IllegalAddressModeException">Thrown if an addressing mode is provided that doesn't fit with writing</exception>
    private ushort ResolveAddressWrite(Instruction ins)
    {
        return ins.AddressMode switch
        {
            AddressMode.Absolute => (ushort)(ins.ExtraBytes[0] | (ins.ExtraBytes[1] << 8)),
            AddressMode.ZeroPage => ins.ExtraBytes[0],
            AddressMode.IndexedIndirect => (ushort)(Read((byte)(ins.ExtraBytes[0] + X)) |
                                                    (Read((byte)(ins.ExtraBytes[0] + X + 1)) << 8)),
            AddressMode.IndirectIndexed => (ushort)(Read(ins.ExtraBytes[0]) +
                                                    (Read((ushort)(ins.ExtraBytes[0] + 1)) << 8) + Y),
            AddressMode.IndexedZeroPageX => (byte)(ins.ExtraBytes[0] + X),
            AddressMode.IndexedZeroPageY => (byte)(ins.ExtraBytes[0] + Y),
            AddressMode.IndexedAbsoluteX => (ushort)((ins.ExtraBytes[0] | (ins.ExtraBytes[1] << 8)) + X),
            AddressMode.IndexedAbsoluteY => (ushort)((ins.ExtraBytes[0] | (ins.ExtraBytes[1] << 8)) + Y),
            AddressMode.Relative => (ushort)(Pc + (ins.ExtraBytes[0] - ((ins.ExtraBytes[0] & 0x80) != 0 ? 256 : 0))),
            AddressMode.AbsoluteIndirect => (ushort)
                (Read((ushort)(ins.ExtraBytes[0] | (ins.ExtraBytes[1] << 8))) |
                 (Read((ushort)((ins.ExtraBytes[0] | (ins.ExtraBytes[1] << 8)) + 1)) << 8)),
            AddressMode.AbsoluteIndexedIndirect => (ushort)
                (Read((ushort)((ins.ExtraBytes[0] | (ins.ExtraBytes[1] << 8)) + X)) |
                 (Read((ushort)((ins.ExtraBytes[0] | (ins.ExtraBytes[1] << 8)) + 1 + X)) << 8)),
            _ => throw new IllegalAddressModeException(ins)
        };
    }

    /// <summary>
    /// Bits in the status register, primarily used to make setting and clearing more transparent
    /// </summary>
    enum StatusBits
    {
        Carry = 0,
        Zero = 1,
        NotIrqDisable = 2,
        DecimalMode = 3, // Will not actually be implemented, as the NES' 6502 derivative omits it too
        BrkCommand = 4,
        Overflow = 6,
        Negative = 7
    }

    /// <summary>
    /// Sets the state of a bit in the status register
    /// </summary>
    /// <param name="index">Bit to modify</param>
    /// <param name="value">true if it should be set to 1, false otherwise</param>
    private void SetStatusBit(StatusBits index, bool value)
    {
        if (value)
        {
            P |= (byte)(1 << (int)index);
        }
        else
        {
            P &= (byte)(0xff ^ (1 << (int)index));
        }
    }

    /// <summary>
    /// Gets the state of a bit in the status register
    /// </summary>
    /// <param name="index">Bit to get</param>
    /// <returns>True if the bit is set, false otherwise</returns>
    private bool GetStatusBit(StatusBits index)
    {
        return (P & (1 << (int)index)) != 0;
    }

    /// <summary>
    /// Pushes to the stack located between 0x100 and 0x1ff, addresses based on S register
    /// </summary>
    /// <param name="value">Byte to push to stack</param>
    private void PushToStack(byte value)
    {
        Write((ushort)(0x100 + S--), value);
    }

    /// <summary>
    /// Pops from the stack located between 0x100 and 0x1ff, addresses based on S register
    /// </summary>
    /// <returns>Byte popped from stack</returns>
    private byte PopFromStack()
    {
        return Read((ushort)(0x100 + ++S));
    }
}