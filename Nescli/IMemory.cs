namespace Nescli;

/// <summary>
/// An abstraction for any kind of memory mapped device. Read() and Write()
/// are more to be understood as handlers for the scenario in which the CPU
/// attempts to write to / read from an address in this component's memory space,
/// should throw a MemoryAccessViolationException if illegal operation is performed
/// </summary>
public interface IMemory
{
    public byte Read(ushort position);
    public void Write(ushort position, byte value);
}