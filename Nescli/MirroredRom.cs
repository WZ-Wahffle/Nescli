namespace Nescli;

/// <summary>
/// Extension of the Rom memory component made for dynamic size memory spaces that need to be mirrored to fit
/// </summary>
public class MirroredRom : Rom
{
    /// <summary>
    /// Unaltered base constructor, see Rom implementation
    /// </summary>
    /// <param name="size">Size of the target memory</param>
    public MirroredRom(ushort size) : base(size)
    {
    }

    /// <summary>
    /// Constructs a Rom segment based off of existing contents, mirroring them across the suggested space
    /// </summary>
    /// <param name="bytes">The contents to build around, can be longer, shorter or equal to size</param>
    /// <param name="suggestedSize">The target size of the memory segment</param>
    public MirroredRom(byte[] bytes, ushort suggestedSize) : base(MirrorContents(bytes, suggestedSize))
    {

    }

    /// <summary>
    /// Helper method for the mirroring constructor
    /// </summary>
    /// <param name="contents">The contents to build around, can be longer, shorter or equal to size</param>
    /// <param name="size">The target size of the memory segment</param>
    /// <returns>The mirrored byte array to construct a Rom around</returns>
    private static byte[] MirrorContents(byte[] contents, ushort size)
    {
        var mirrored = new byte[size];
        for (var i = 0; i < size; i++)
        {
            mirrored[i] = contents[i % contents.Length];
        }

        return mirrored;
    }
}