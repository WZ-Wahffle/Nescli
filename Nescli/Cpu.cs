namespace Nescli;

/// <summary>
/// Represents the 6502 CPU inside an NES, whose only way of
/// interfacing with the outside world is over the address and data lines,
/// emulated by the MemoryController member
/// </summary>
public class Cpu
{
    private readonly MemoryController _mc;
    private ushort Pc { get; set; }
    public byte A { get; private set; } = 0;
    public byte X { get; private set; } = 0;
    public byte Y { get; private set; } = 0;
    public byte S { get; private set; } = 0;
    public byte P { get; private set; } = 0;

    /// <summary>
    /// Memory handling is managed through constructor injection,
    /// since the CPU is entirely unable to communicate with
    /// the outside world otherwise
    /// </summary>
    /// <param name="mc">The memory map to assign to the processor</param>
    public Cpu(MemoryController mc)
    {
        _mc = mc;
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
    public void Interrupt(InterruptSource src)
    {
        PushToStack((byte)(Pc >> 8));
        PushToStack((byte)(Pc & 0xff));
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

        Pc = (ushort)((_mc.Read(resetVector.Item1) << 8) | _mc.Read(resetVector.Item2));
    }

    /// <summary>
    /// Fetches, decodes and executes a single instruction
    /// </summary>
    public void Run()
    {
        var unresolvedOp = _mc.Read(Pc++);
        var opcode = Decoder.Decode(unresolvedOp);
        var extraBytes = new byte[Decoder.ResolveRemainingBytes(opcode.Item2)];
        for (var i = 0; i < extraBytes.Length; i++)
        {
            extraBytes[i] = _mc.Read(Pc++);
        }

        var instruction = new Instruction(opcode.Item1, opcode.Item2, extraBytes);
        Console.WriteLine(instruction);
        try
        {
            Execute(instruction);
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
    /// <exception cref="NotImplementedException">Thrown if instruction has not been implemented yet</exception>
    /// <exception cref="IllegalAddressModeException">Thrown if address mode in instruction is undefined for opcode</exception>
    public void Execute(Instruction ins)
    {
        switch (ins.Op)
        {
            case Opcode.Adc:
                throw new NotImplementedException(ins.ToString());
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
                        A &= (byte)ResolveAddressRead(ins);
                        SetStatusBit(StatusBits.Zero, A == 0);
                        SetStatusBit(StatusBits.Negative, (A & 0b10000000) != 0);
                        break;
                    default:
                        throw new IllegalAddressModeException(ins);
                }

                break;
            case Opcode.Asl:
                throw new NotImplementedException(ins.ToString());
                break;
            case Opcode.Bcc:
                throw new NotImplementedException(ins.ToString());
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
                throw new NotImplementedException(ins.ToString());
                break;
            case Opcode.Bit:
                switch (ins.AddressMode)
                {
                    case AddressMode.Immediate:
                    case AddressMode.Absolute:
                    case AddressMode.ZeroPage:
                    case AddressMode.IndexedZeroPageX:
                    case AddressMode.IndexedAbsoluteX:
                        var result = (byte)ResolveAddressRead(ins) & A;
                        SetStatusBit(StatusBits.Zero, result == 0);
                        SetStatusBit(StatusBits.Negative, (result & 0b10000000) != 0);
                        SetStatusBit(StatusBits.Overflow, (result & 0b1000000) != 0);
                        break;
                    default:
                        throw new IllegalAddressModeException(ins);
                }

                break;
            case Opcode.Bmi:
                throw new NotImplementedException(ins.ToString());
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
                throw new NotImplementedException(ins.ToString());
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
                throw new NotImplementedException(ins.ToString());
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
                throw new NotImplementedException(ins.ToString());
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
                        A ^= (byte)ResolveAddressRead(ins);
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
                        var value = (byte)(_mc.Read(addr) + 1);
                        _mc.Write(addr, value);
                        SetStatusBit(StatusBits.Zero, value == 0);
                        SetStatusBit(StatusBits.Negative, (value & 0b10000000) != 0);
                        break;
                    default:
                        throw new IllegalAddressModeException(ins);
                }

                break;
            case Opcode.Inx:
                throw new NotImplementedException(ins.ToString());
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
                        A = (byte)ResolveAddressRead(ins);
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
                        X = (byte)ResolveAddressRead(ins);
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
                        Y = (byte)ResolveAddressRead(ins);
                        SetStatusBit(StatusBits.Zero, X == 0);
                        SetStatusBit(StatusBits.Negative, X > 127);
                        break;
                    default:
                        throw new IllegalAddressModeException(ins);
                }

                break;
            case Opcode.Lsr:
                throw new NotImplementedException(ins.ToString());
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
                        A |= (byte)ResolveAddressRead(ins);
                        SetStatusBit(StatusBits.Zero, A == 0);
                        SetStatusBit(StatusBits.Negative, (A & 0b10000000) != 0);
                        break;
                    default:
                        throw new IllegalAddressModeException(ins);
                }

                break;
            case Opcode.Pha:
                throw new NotImplementedException(ins.ToString());
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
                throw new NotImplementedException(ins.ToString());
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
                throw new NotImplementedException(ins.ToString());
                break;
            case Opcode.Ror:
                throw new NotImplementedException(ins.ToString());
                break;
            case Opcode.Rti:
                throw new NotImplementedException(ins.ToString());
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
                throw new NotImplementedException(ins.ToString());
                break;
            case Opcode.Sec:
                throw new NotImplementedException(ins.ToString());
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
                        _mc.Write(ResolveAddressWrite(ins), A);
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
                        _mc.Write(ResolveAddressWrite(ins), X);
                        break;
                    default:
                        throw new IllegalAddressModeException(ins);
                }

                break;
            case Opcode.Sty:
                throw new NotImplementedException(ins.ToString());
                break;
            case Opcode.Stz:
                throw new NotImplementedException(ins.ToString());
                break;
            case Opcode.Tax:
                throw new NotImplementedException(ins.ToString());
                break;
            case Opcode.Tay:
                throw new NotImplementedException(ins.ToString());
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
                throw new NotImplementedException(ins.ToString());
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
    private int ResolveAddressRead(Instruction ins)
    {
        return ins.AddressMode switch
        {
            AddressMode.Immediate => ins.ExtraBytes[0],
            AddressMode.Absolute => _mc.Read((ushort)(ins.ExtraBytes[0] | ins.ExtraBytes[1] << 8)),
            AddressMode.ZeroPage => _mc.Read(ins.ExtraBytes[0]),
            AddressMode.IndexedIndirect => _mc.Read((ushort)(_mc.Read((byte)(ins.ExtraBytes[0] + X)) |
                                                             _mc.Read((byte)(ins.ExtraBytes[0] + X + 1)) << 8)),
            AddressMode.IndirectIndexed => _mc.Read((ushort)(_mc.Read(ins.ExtraBytes[0]) +
                                                             (_mc.Read((ushort)(ins.ExtraBytes[0] + 1)) << 8) + Y)),
            AddressMode.IndexedZeroPageX => _mc.Read((byte)(_mc.Read(ins.ExtraBytes[0]) + X)),
            AddressMode.IndexedZeroPageY => _mc.Read((byte)(_mc.Read(ins.ExtraBytes[0]) + Y)),
            AddressMode.IndexedAbsoluteX => _mc.Read((ushort)((ins.ExtraBytes[0] | ins.ExtraBytes[1] << 8) + X)),
            AddressMode.IndexedAbsoluteY => _mc.Read((ushort)((ins.ExtraBytes[0] | ins.ExtraBytes[1] << 8) + Y)),
            AddressMode.AbsoluteIndirect => _mc.Read(_mc.Read((ushort)(ins.ExtraBytes[0] | ins.ExtraBytes[1] << 8))) |
                                            _mc.Read(_mc.Read(
                                                (ushort)((ins.ExtraBytes[0] | ins.ExtraBytes[1] << 8) + 1))) << 8,
            AddressMode.AbsoluteIndexedIndirect => _mc.Read(_mc.Read((ushort)((ins.ExtraBytes[0] |
                                                                               ins.ExtraBytes[1] << 8) + X))) |
                                                   _mc.Read(_mc.Read(
                                                       (ushort)((ins.ExtraBytes[0] | ins.ExtraBytes[1] << 8) + 1 +
                                                                X))) << 8,
            AddressMode.ZeroPageIndirect => _mc.Read((ushort)(_mc.Read(ins.ExtraBytes[0]) |
                                                              _mc.Read((ushort)(ins.ExtraBytes[0] + 1)) << 8)),
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
            AddressMode.Absolute => (ushort)(ins.ExtraBytes[0] | ins.ExtraBytes[1] << 8),
            AddressMode.ZeroPage => ins.ExtraBytes[0],
            AddressMode.IndexedIndirect => (ushort)(_mc.Read((byte)(ins.ExtraBytes[0] + X)) |
                                                    _mc.Read((byte)(ins.ExtraBytes[0] + X + 1)) << 8),
            AddressMode.IndirectIndexed => (ushort)(_mc.Read(ins.ExtraBytes[0]) +
                                                    (_mc.Read((ushort)(ins.ExtraBytes[0] + 1)) << 8) + Y),
            AddressMode.IndexedZeroPageX => (byte)(_mc.Read(ins.ExtraBytes[0]) + X),
            AddressMode.IndexedZeroPageY => (byte)(_mc.Read(ins.ExtraBytes[0]) + Y),
            AddressMode.IndexedAbsoluteX => (ushort)((ins.ExtraBytes[0] | ins.ExtraBytes[1] << 8) + X),
            AddressMode.IndexedAbsoluteY => (ushort)((ins.ExtraBytes[0] | ins.ExtraBytes[1] << 8) + Y),
            AddressMode.Relative => (ushort)(Pc + (ins.ExtraBytes[0] - ((ins.ExtraBytes[0] & 0x80) != 0 ? 256 : 0))),
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
        DecimalMode = 3, // WIll not actually be implemented, as the NES' 6502 derivative omits it too
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
        _mc.Write((ushort)(0x100 + S--), value);
    }

    /// <summary>
    /// Pops from the stack located between 0x100 and 0x1ff, addresses based on S register
    /// </summary>
    /// <returns>Byte popped from stack</returns>
    private byte PopFromStack()
    {
        return _mc.Read((ushort)(0x100 + ++S));
    }
}