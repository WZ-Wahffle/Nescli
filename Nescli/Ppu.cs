using Raylib_cs;

namespace Nescli;

/// <summary>
/// Represents the Picture Processing Unit inside an NES. It has its own memory map
/// and an internal 256x240px framebuffer for the screen.
/// </summary>
public class Ppu
{
    private readonly MemoryController _mc;
    public readonly Color[,] FrameBuffer;
    private ushort _baseNametableAddress;

    /// <summary>
    /// Constructs a new PPU and sets its framebuffer to a black screen
    /// </summary>
    /// <param name="mc">The memory controller to attach</param>
    public Ppu(MemoryController mc)
    {
        _mc = mc;
        FrameBuffer = new Color[256, 240];
        for (var i = 0; i < FrameBuffer.GetLength(0); i++)
        {
            for (var j = 0; j < FrameBuffer.GetLength(1); j++)
            {
                FrameBuffer[i, j] = new Color(0, 0, 0, 255);
            }
        }
    }

    /// <summary>
    /// Control register accessed by the CPU over the data bus
    /// </summary>
    /// <param name="value">Value on the bus</param>
    public void PpuCtrl(byte value)
    {
        _baseNametableAddress = (value & 0b11) switch
        {
            0 => 0x2000,
            1 => 0x2400,
            2 => 0x2800,
            3 => 0x2c00,
            _ => 0 // so the linter stops complaining about non-exhaustive switches
        };
    }

    /// <summary>
    /// Fills the framebuffer with the CHR Rom as a big spritesheet, for debug purposes
    /// </summary>
    public void GenerateSpritesheet()
    {
        for (ushort i = 0; i < 0x2000; i += 16)
        {
            ulong l1 = _mc.Read64(i);
            ulong l2 = _mc.Read64((ushort)(i + 8));

            var x = 0;
            var y = 0;
            for (var j = 63; j > 0; j--)
            {
                if ((l1 & (1ul << j)) != 0)
                {
                    if ((l2 & (1ul << j)) != 0)
                    {
                        FrameBuffer[i / 2 % (32 * 8) + x, i / (32 * 8) + y] = new Color(0xc0, 0xc0, 0xc0, 0xff);
                    }
                    else
                    {
                        FrameBuffer[i / 2 % (32 * 8) + x, i / (32 * 8) + y] = new Color(0x40, 0x40, 0x40, 0xff);
                    }
                }
                else
                {
                    if ((l2 & (1ul << j)) != 0)
                    {
                        FrameBuffer[i / 2 % (32 * 8) + x, i / (32 * 8) + y] = new Color(0x80, 0x80, 0x80, 0xff);
                    }
                    else
                    {
                        FrameBuffer[i / 2 % (32 * 8) + x, i / (32 * 8) + y] = new Color(0x0, 0x0, 0x0, 0xff);
                    }
                }
                x++;
                if (x != 8) continue;
                x %= 8;
                y++;
            }
        }
    }
}