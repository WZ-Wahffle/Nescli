namespace Nescli;

/// <summary>
/// Implementation of RAM component
/// </summary>
public class Ram : IMemory
{
    private readonly byte[] _contents;

    /// <summary>
    /// Constructs a RAM to a specified size
    /// </summary>
    /// <param name="size">The target size</param>
    public Ram(ushort size)
    {
        _contents = new byte[size];
    }

    /// <summary>
    /// Reads a value from ROM, always accessible due to RAM nature
    /// </summary>
    /// <param name="position">Position to read at</param>
    /// <returns>Byte read at position</returns>
    public byte Read(ushort position)
    {
        return _contents[position];
    }

    /// <summary>
    /// Writes a value to ROM, always accessible due to RAM nature
    /// </summary>
    /// <param name="position">Position to write to</param>
    /// <param name="value">Byte to write</param>
    public void Write(ushort position, byte value)
    {
        _contents[position] = value;
    }
}