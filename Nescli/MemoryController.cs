namespace Nescli;

/// <summary>
/// Abstraction for a CPU's address and data lines
/// </summary>
public class MemoryController
{
    private IMemory[] _memory = [];
    private Range[] _mapping = [];

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

        throw new MemoryAccessViolationException($"Attempted to write to nonexistent address: {position}");
    }

    /// <summary>
    /// Finds the stack in the memory map, not very nice but does work
    /// </summary>
    /// <returns>The stack, if found</returns>
    /// <exception cref="KeyNotFoundException">Thrown if there is no stack</exception>
    public Stack FindStack()
    {
        foreach (var i in _memory)
        {
            if(i.GetType() == typeof(Stack))
            {
                return (Stack)i;
            }
        }

        throw new KeyNotFoundException("No stack found in memory controller");
    }
}