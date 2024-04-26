namespace Nescli;

/// <summary>
/// Represents the 6502 CPU inside an NES as well, whose only way of
/// interfacing with the outside world is over the address and data lines,
/// emulated by the MemoryController member
/// </summary>
public class Cpu
{
    private MemoryController _mc;
    public ushort _pc { get; }

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
        _pc = (ushort)((mc.Read(0xfffd) << 8) | mc.Read(0xfffc));
    }

    /// <summary>
    /// Begins execution, so far only parses the first opcode
    /// </summary>
    public void Run()
    {
        var opcode = _mc.Read(_pc);
        var op = Decoder.Decode(opcode);
        Console.WriteLine(op);
    }
}