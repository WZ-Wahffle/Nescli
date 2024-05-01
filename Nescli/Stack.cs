namespace Nescli;

/// <summary>
/// Implementation of stack for subroutines and general storage
/// </summary>
public class Stack : IMemory
{
    private ushort _sp;
    private readonly byte[] _contents;

    public Stack(ushort size)
    {
        _sp = (ushort)(size - 1);
        _contents = new byte[size];
    }

    /// <summary>
    /// Attempt to manually read from the stack
    /// </summary>
    /// <param name="position">Arbitrary position</param>
    /// <returns>Nothing, ideally</returns>
    /// <exception cref="MemoryAccessViolationException">Will always be thrown, since the stack is not intended for manual reads/writes</exception>
    public byte Read(ushort position)
    {
        throw new MemoryAccessViolationException($"Attempted to read from stack at {position}");
    }

    /// <summary>
    /// Intended way of reading from stack, but does require casting to use
    /// </summary>
    /// <returns>The value at the top of the stack</returns>
    public byte Pop()
    {
        byte ret = _contents[_sp++];
        if (_sp == _contents.Length)
        {
            _sp = 0;
        }

        return ret;
    }

    /// <summary>
    /// Intended way of writing to stack, but does require casting to use
    /// </summary>
    /// <param name="value">Value to put on stack</param>
    public void Push(byte value)
    {
        _contents[_sp--] = value;
        if (_sp == ushort.MaxValue)
        {
            _sp = (ushort)(_contents.Length - 1);
        }
    }

    /// <summary>
    /// Attempt to manually write to the stack
    /// </summary>
    /// <param name="position">Arbitrary position</param>
    /// <param name="value">Arbitrary value</param>
    /// <exception cref="MemoryAccessViolationException">Will always be thrown, since the stack is not intended for manual reads/writes</exception>
    public void Write(ushort position, byte value)
    {
        throw new MemoryAccessViolationException($"Attempted to write to stack at {position}");
    }
}