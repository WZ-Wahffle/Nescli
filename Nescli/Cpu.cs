namespace Nescli;

/// <summary>
/// Represents the 6502 CPU inside an NES as well, whose only way of
/// interfacing with the outside world is over the address and data lines,
/// emulated by the MemoryController member
/// </summary>
public class Cpu
{
    private MemoryController _mc;
    public ushort Pc { get; set; }

    /// <summary>
    /// Memory handling is handled through constructor injection,
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
    /// Begins execution, so far only parses the first opcode
    /// </summary>
    public void Run()
    {

        var opcode = _mc.Read(Pc++);
        var op = Decoder.Decode(opcode);
        var extraBytes = new byte[Decoder.ResolveRemainingBytes(op.Item2)];
        for (var i = 0; i < extraBytes.Length; i++)
        {
            extraBytes[i] = _mc.Read(Pc++);
        }

        var instruction = new Instruction(op.Item1, op.Item2, extraBytes);

        Console.WriteLine(instruction);
    }
}