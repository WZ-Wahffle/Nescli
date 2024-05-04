namespace Nescli;

/// <summary>
/// Describes the contents of a typical .nes file.
/// Mostly just serves as an intermediate container,
/// since most of the file is just a ROM dump anyway, but I'll
/// probably add more once I find some uses for the extra header values.
/// </summary>
public class Ines
{
    public enum GamePlatform
    {
        Pal,
        Ntsc,
        Dual
    }
    public enum NametableArrangement
    {
        Vertical,
        Horizontal
    }
    public byte[] HeaderUnused;
    public int AsmSize { get; set; }
    public int GraphicsSize { get; set; }
    public NametableArrangement Arrangement;
    public GamePlatform Platform;
    public bool HasPrgRam;
    public bool HasTrainer;
    public bool HasAlternativeNametableLayout;
    public int MapperIndex;
    public int PrgRamSize;

    public Ines(byte[] header)
    {
        if ((header[4] & 0xc) != 0) throw new ArgumentException("Only NES 1.0 supported!");
        AsmSize = 16384 * header[0];
        GraphicsSize = 8192 * header[1];
        Arrangement = (header[2] & 0x01) != 0 ? NametableArrangement.Horizontal : NametableArrangement.Vertical;
        HasPrgRam = (header[2] & 0x02) != 0;
        HasTrainer = (header[2] & 0x04) != 0;
        HasAlternativeNametableLayout = (header[2] & 0x08) != 0;
        MapperIndex = (header[2] >> 4) | (header[3] & 0xf0);
        PrgRamSize = 8192 * header[4];
        Platform = (header[5] & 0b11) switch
        {
            0 => GamePlatform.Ntsc,
            1 => GamePlatform.Dual,
            2 => GamePlatform.Pal,
            3 => GamePlatform.Dual,
            _ => 0
        };
    }
}