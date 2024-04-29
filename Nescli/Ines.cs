namespace Nescli;

/// <summary>
/// Describes the contents of a typical .nes file.
/// Mostly just serves as an intermediate container,
/// since most of the file is just a ROM dump anyway, but I'll
/// probably add more once I find some uses for the extra header values.
/// </summary>
public class Ines
{
    public byte[] HeaderUnused;
    public long FileSize { get; private set; }
    public int AsmSize { get; set; }
    public int GraphicsSize { get; set; }

    public Ines(long fileSize)
    {
        FileSize = fileSize;
    }
}