namespace Nescli;

public class Stack : IMemory
{
    private ushort _sp;
    private readonly byte[] _contents;

    public Stack(ushort size)
    {
        _sp = (ushort)(size - 1);
        _contents = new byte[size];
    }
    public byte Read(ushort position)
    {
        throw new MemoryAccessViolationException($"Attempted to read from stack at {position}");
    }

    public byte Pop()
    {
        byte ret = _contents[_sp++];
        if (_sp == _contents.Length)
        {
            _sp = 0;
        }

        return ret;
    }

    public void Push(byte value)
    {
        _contents[_sp--] = value;
        if (_sp == ushort.MaxValue)
        {
            _sp = (ushort)(_contents.Length - 1);
        }
    }

    public void Write(ushort position, byte value)
    {
        throw new MemoryAccessViolationException($"Attempted to write to stack at {position}");
    }
}