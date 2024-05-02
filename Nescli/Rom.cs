namespace Nescli;

/// <summary>
/// Implementation of supporting ROM component on bus
/// </summary>
public class Rom : IMemory
{
    private readonly byte[] _bytes;

    /// <summary>
    /// Allocates a new, empty ROM
    /// </summary>
    /// <param name="size">Size of the ROM in bytes</param>
    public Rom(ushort size)
    {
        _bytes = new byte[size];
    }

    /// <summary>
    /// Generates a ROM for a preexisting byte array, provided it is not larger than 0xffff bytes
    /// </summary>
    /// <param name="bytes">Bytes to turn into ROM</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if Array is too long</exception>
    public Rom(byte[] bytes)
    {
        if (bytes.Length > ushort.MaxValue)
        {
            throw new ArgumentOutOfRangeException();
        }
        _bytes = bytes;
    }

    /// <summary>
    /// External reading from ROM
    /// </summary>
    /// <param name="position">Index within ROM, NOT within CPU memory map!</param>
    /// <returns>Byte at specified position</returns>
    public byte Read(ushort position)
    {
        return _bytes[position];
    }

    /// <summary>
    /// Attempt of external writing to ROM
    /// </summary>
    /// <param name="position">Position attempted to write at</param>
    /// <param name="value">Value attempted to write</param>
    /// <exception cref="MemoryAccessViolationException">Will always be thrown, as ROM is read-only. If this happens, it's the assembly programmer's fault.</exception>
    public void Write(ushort position, byte value)
    {
        throw new MemoryAccessViolationException($"Attempted to write to ROM address: {position}");
    }
}

