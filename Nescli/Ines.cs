namespace Nescli;

/// <summary>
/// Describes the contents of a typical .nes file.
/// Mostly just serves as an intermediate container,
/// since most of the file is just a ROM dump anyway, but I'll
/// probably add more once I find some uses for the extra header values.
/// </summary>
public class Ines
{
    public byte[] _headerUnused;
    public long fileSize { get; set; }
    public int asmSize { get; set; }
    public int graphicsSize { get; set; }
}