namespace Nescli;

/// <summary>
/// Represents the 6502 CPU inside an NES as well, whose only way of
/// interfacing with the outside world is over the address and data lines,
/// emulated by the MemoryController member
/// </summary>
public class Cpu
{
    private readonly MemoryController _mc;
    private ushort Pc { get; set; }
    private byte _a = 0;
    private byte _x = 0;
    private byte _y = 0;
    private byte _s = 0;
    private byte _p = 0;

    /// <summary>
    /// Memory handling is managed through constructor injection,
    /// since the CPU is entirely unable to communicate with
    /// the outside world otherwise
    /// </summary>
    /// <param name="mc">The memory map to assign to the processor</param>
    public Cpu(MemoryController mc)
    {
        _mc = mc;
        // Program counter is set to the 16-bit ROM address stored at 0xfffd and 0xfffc,
        // as it would in a real 6502
        Pc = (ushort)((mc.Read(0xfffd) << 8) | mc.Read(0xfffc));
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
        Execute(instruction);
    }

    /// <summary>
    /// Performs the actions associated with an instruction
    /// </summary>
    /// <param name="ins">Instruction to execute</param>
    /// <exception cref="NotImplementedException">Thrown if instruction has not been implemented yet</exception>
    /// <exception cref="IllegalAddressModeException">Thrown if address mode in instruction is undefined for opcode</exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private void Execute(Instruction ins)
    {
        switch (ins.Op)
        {
            case Opcode.Adc:
                throw new NotImplementedException(ins.ToString());
                break;
            case Opcode.And:
                throw new NotImplementedException(ins.ToString());

                break;
            case Opcode.Asl:
                throw new NotImplementedException(ins.ToString());
                break;
            case Opcode.Bcc:
                throw new NotImplementedException(ins.ToString());
                break;
            case Opcode.Bcs:
                throw new NotImplementedException(ins.ToString());
                break;
            case Opcode.Beq:
                throw new NotImplementedException(ins.ToString());
                break;
            case Opcode.Bit:
                throw new NotImplementedException(ins.ToString());
                break;
            case Opcode.Bmi:
                throw new NotImplementedException(ins.ToString());
                break;
            case Opcode.Bne:
                throw new NotImplementedException(ins.ToString());
                break;
            case Opcode.Bpl:
                throw new NotImplementedException(ins.ToString());
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

                _p &= 0b11110111;
                break;
            case Opcode.Cli:
                throw new NotImplementedException(ins.ToString());
                break;
            case Opcode.Clv:
                throw new NotImplementedException(ins.ToString());
                break;
            case Opcode.Cmp:
                throw new NotImplementedException(ins.ToString());
                break;
            case Opcode.Cpx:
                throw new NotImplementedException(ins.ToString());
                break;
            case Opcode.Cpy:
                throw new NotImplementedException(ins.ToString());
                break;
            case Opcode.Dec:
                throw new NotImplementedException(ins.ToString());
                break;
            case Opcode.Dex:
                throw new NotImplementedException(ins.ToString());
                break;
            case Opcode.Dey:
                throw new NotImplementedException(ins.ToString());
                break;
            case Opcode.Eor:
                throw new NotImplementedException(ins.ToString());
                break;
            case Opcode.Inc:
                throw new NotImplementedException(ins.ToString());
                break;
            case Opcode.Inx:
                throw new NotImplementedException(ins.ToString());
                break;
            case Opcode.Iny:
                throw new NotImplementedException(ins.ToString());
                break;
            case Opcode.Jmp:
                throw new NotImplementedException(ins.ToString());
                break;
            case Opcode.Jsr:
                throw new NotImplementedException(ins.ToString());
                break;
            case Opcode.Lda:
                throw new NotImplementedException(ins.ToString());
                break;
            case Opcode.Ldx:
                throw new NotImplementedException(ins.ToString());
                break;
            case Opcode.Ldy:
                throw new NotImplementedException(ins.ToString());
                break;
            case Opcode.Lsr:
                throw new NotImplementedException(ins.ToString());
                break;
            case Opcode.Nop:
                throw new NotImplementedException(ins.ToString());
                break;
            case Opcode.Ora:
                throw new NotImplementedException(ins.ToString());
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
                throw new NotImplementedException(ins.ToString());
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

                _p |= 0b100;
                break;
            case Opcode.Sta:
                throw new NotImplementedException(ins.ToString());
                break;
            case Opcode.Stx:
                throw new NotImplementedException(ins.ToString());
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
                throw new NotImplementedException(ins.ToString());
                break;
            case Opcode.Txs:
                throw new NotImplementedException(ins.ToString());
                break;
            case Opcode.Tya:
                throw new NotImplementedException(ins.ToString());
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}