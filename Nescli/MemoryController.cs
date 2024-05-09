namespace Nescli;

/// <summary>
/// Abstraction for a CPU's address and data lines
/// </summary>
public class MemoryController
{
    private IMemory[] _memory = [];
    private Range[] _mapping = [];
    private byte _intermediate;

    /// <summary>
    /// Add a memory component to the ram bus of the processor
    /// </summary>
    /// <param name="memory">The component to add</param>
    /// <param name="start">What RAM address component's first address should be at; inclusive</param>
    /// <param name="end">What RAM address component's last address should be at; exclusive</param>
    public void AddMemory(IMemory memory, int start, int end)
    {
        _memory = _memory.Append(memory).ToArray();
        _mapping = _mapping.Append(new Range(new Index(start), new Index(end))).ToArray();
    }

    /// <summary>
    /// Abstraction for putting a value onto the address bus and providing a read signal
    /// </summary>
    /// <param name="position">Value on the address bus</param>
    /// <returns>Value received on data bus</returns>
    /// <exception cref="MemoryAccessViolationException">Thrown if target address is not mapped</exception>
    public byte Read(ushort position)
    {
        for (var i = 0; i < _memory.Length; i++)
        {
            if (_mapping[i].Start.Value <= position && _mapping[i].End.Value > position)
            {
                return _memory[i].Read((ushort)(position - _mapping[i].Start.Value));
            }
        }

        throw new MemoryAccessViolationException($"Attempted to read from nonexistent address: {position}");
    }

    /// <summary>
    /// The same as Read, except for values 8 bytes wide (may or may not break on Big Endian platforms, who knows at this point)
    /// </summary>
    /// <param name="position">Index of the first byte to be read</param>
    /// <returns>The 8 read bytes</returns>
    public ulong Read64(ushort position)
    {
        var buffer = new byte[8];
        for (ushort i = 0; i < 8; i++)
        {
            buffer[i] = Read((ushort)(position + i));
        }

        return BitConverter.ToUInt64(buffer, 0);
    }

    /// <summary>
    /// Abstraction for putting values onto the address and data buses and providing a write signal
    /// </summary>
    /// <param name="position">Value on address bus</param>
    /// <param name="value">Value on data bus</param>
    /// <exception cref="MemoryAccessViolationException">Thrown if target address does not exist or is read-only</exception>
    public void Write(ushort position, byte value)
    {
        for (var i = 0; i < _memory.Length; i++)
        {
            if (_mapping[i].Start.Value <= position && _mapping[i].End.Value > position)
            {
                _memory[i].Write((ushort)(position - _mapping[i].Start.Value), value);
                return;
            }
        }

        throw new MemoryAccessViolationException($"Attempted to write to nonexistent address: 0x{position:x}");
    }
}