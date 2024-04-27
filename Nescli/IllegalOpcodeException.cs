namespace Nescli;

[Serializable]
public class IllegalOpcodeException : Exception
{
    public IllegalOpcodeException()
    {
    }

    public IllegalOpcodeException(string message) : base(message)
    {

    }

    public IllegalOpcodeException(string message, Exception inner) : base(message, inner)
    {

    }

    public IllegalOpcodeException(byte value) : base($"Failed to resolve 0x{value:x2} to an opcode")
    {

    }
}