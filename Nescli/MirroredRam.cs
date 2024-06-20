namespace Nescli;

/// <summary>
/// Extension of the Rom memory component made for mirrored memory segments
/// </summary>
public class MirroredRam : Ram
{
    private readonly ushort size;
    /// <summary>
    /// unaltered base constructor, see Ram implementation
    /// </summary>
    /// <param name="size">Size of non-mirrored elements</param>
    public MirroredRam(ushort size) : base(size)
    {

    }

    /// <summary>
    /// Modified Read, truncates index to actual index range
    /// </summary>
    /// <param name="position">position to read from; truncated to size</param>
    /// <returns>byte at position</returns>
    public new byte Read(ushort position)
    {
        return base.Read((ushort)(position % size));
    }
}