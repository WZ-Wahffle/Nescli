namespace Nescli;

/// <summary>
/// Reports attempts to perform a memory access on a memory type
/// which does not permit such an option, such as trying to write to ROM
/// or trying to read an unmapped address
/// </summary>
[Serializable]
public class MemoryAccessViolationException : Exception
{
    public MemoryAccessViolationException()
    {
    }

    public MemoryAccessViolationException(string message) : base(message)
    {

    }

    public MemoryAccessViolationException(string message, Exception inner) : base(message, inner)
    {

    }
}