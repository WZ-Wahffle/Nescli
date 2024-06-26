namespace Nescli;

/// <summary>
/// Abstraction for a full, self-contained instruction
/// </summary>
public class Instruction
{
    public readonly Opcode Op;
    public readonly AddressMode AddressMode;
    public readonly byte[] ExtraBytes;

    /// <summary>
    /// Constructs a new instruction
    /// </summary>
    /// <param name="op">The opcode of the instruction</param>
    /// <param name="addressMode">The addressing mode for the instruction</param>
    /// <param name="extraBytes">The bytes required as extra parameters</param>
    public Instruction(Opcode op, AddressMode addressMode, byte[] extraBytes)
    {
        Op = op;
        AddressMode = addressMode;
        ExtraBytes = extraBytes;
    }

    /// <summary>
    /// Simple toString for debugging purposes, may be edited or removed later
    /// </summary>
    /// <returns>A rough string representation of the object</returns>
    public override string ToString()
    {
        var s = $"{Op}, {AddressMode}, [";
        foreach (var b in ExtraBytes)
        {
            s += b + ", ";
        }

        return s + "]";
    }
}