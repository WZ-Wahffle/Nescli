namespace Nescli;

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