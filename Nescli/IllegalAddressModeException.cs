namespace Nescli;

/// <summary>
/// Exception for signaling that an opcode was somehow executed
/// with an illegal addressing mode, should not happen unless someone manually
/// calls the Execute() method of a CPU with bad parameters
/// </summary>
[Serializable]
public class IllegalAddressModeException : Exception
{
    public IllegalAddressModeException()
    {
    }

    public IllegalAddressModeException(string message) : base(message)
    {

    }

    public IllegalAddressModeException(string message, Exception inner) : base(message, inner)
    {

    }

    public IllegalAddressModeException(Instruction ins) : base($"Tried to execute {ins.Op} with {ins.AddressMode}")
    {

    }

    public IllegalAddressModeException(byte value) : base($"Failed to resolve {value:x2}")
    {

    }
}