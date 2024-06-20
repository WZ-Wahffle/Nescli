using System.Threading.Channels;
using Raylib_cs;

namespace Nescli;

/// <summary>
/// Represents the Picture Processing Unit inside an NES. It has its own memory map
/// and an internal 256x240px framebuffer for the screen.
/// </summary>
public class Ppu
{
    private Channel<Cpu.InterruptSource> _channel;
    public ushort V;
    public ushort T;
    public byte X;
    public bool W;
    private byte _vramAddressIncrementPerDataReadWrite;
    private bool _useHigherPatternTableSprites;
    private bool _useHigherPatternTableBackgrounds;
    private bool _useWideSprites;
    private bool _enableNmi;
    private readonly MemoryController _mc;
    private readonly int[,] _frameBuffer;

    private class OamObject
    {
        public byte YCoordinate;
        public byte TileNo;
        public byte Attribute;
        public byte XCoordinate;
    }

    private OamObject[] _oam = new OamObject[64];
    private byte _oamAddress;

    private int _xScroll;
    private int _yScroll;
    private ushort _baseNametableAddress;
    private bool _nmiFlag = true;
    private bool _spriteZeroHitFlag = true;
    private bool _spriteOverflowFlag = true;
    private bool _greyscaleMode = false;
    private bool _showBackgroundLeftmost = false;
    private bool _showSpriteLeftmost = false;
    private bool _showBackground = false;
    private bool _showSprites = false;
    private bool _emphasizeRed = false;
    private bool _emphasizeGreen = false;
    private bool _emphasizeBlue = false;


    /// <summary>
    /// NTSC color palette
    /// </summary>
    private static readonly Color[] Palette =
    [
        new(0x62, 0x62, 0x62, 0xff),
        new(0x00, 0x1f, 0xb2, 0xff),
        new(0x24, 0x04, 0xc8, 0xff),
        new(0x52, 0x00, 0xb2, 0xff),
        new(0x73, 0x00, 0x76, 0xff),
        new(0x80, 0x00, 0x24, 0xff),
        new(0x73, 0x0b, 0x00, 0xff),
        new(0x52, 0x28, 0x00, 0xff),
        new(0x24, 0x44, 0x00, 0xff),
        new(0x00, 0x57, 0x00, 0xff),
        new(0x00, 0x5c, 0x00, 0xff),
        new(0x00, 0x53, 0x24, 0xff),
        new(0x00, 0x3c, 0x76, 0xff),
        new(0x00, 0x00, 0x00, 0xff),
        new(0x00, 0x00, 0x00, 0xff),
        new(0x00, 0x00, 0x00, 0xff),
        new(0xab, 0xab, 0xab, 0xff),
        new(0x0d, 0x57, 0xff, 0xff),
        new(0x4b, 0x30, 0xff, 0xff),
        new(0x8a, 0x13, 0xff, 0xff),
        new(0xbc, 0x08, 0xd6, 0xff),
        new(0xd2, 0x12, 0x69, 0xff),
        new(0xc7, 0x2e, 0x00, 0xff),
        new(0x9d, 0x54, 0x00, 0xff),
        new(0x60, 0x7b, 0x00, 0xff),
        new(0x20, 0x98, 0x00, 0xff),
        new(0x00, 0xa3, 0x00, 0xff),
        new(0x00, 0x99, 0x42, 0xff),
        new(0x00, 0x7d, 0xb4, 0xff),
        new(0x00, 0x00, 0x00, 0xff),
        new(0x00, 0x00, 0x00, 0xff),
        new(0x00, 0x00, 0x00, 0xff),
        new(0xff, 0xff, 0xff, 0xff),
        new(0x53, 0xae, 0xff, 0xff),
        new(0x90, 0x85, 0xff, 0xff),
        new(0xd3, 0x65, 0xff, 0xff),
        new(0xff, 0x57, 0xff, 0xff),
        new(0xff, 0x5d, 0xcf, 0xff),
        new(0xff, 0x77, 0x57, 0xff),
        new(0xfa, 0x9e, 0x00, 0xff),
        new(0xbd, 0xc7, 0x00, 0xff),
        new(0x7a, 0xe7, 0x00, 0xff),
        new(0x43, 0xf6, 0x11, 0xff),
        new(0x26, 0xef, 0x7e, 0xff),
        new(0x2c, 0xd5, 0xf6, 0xff),
        new(0x4e, 0x4e, 0x4e, 0xff),
        new(0x00, 0x00, 0x00, 0xff),
        new(0x00, 0x00, 0x00, 0xff),
        new(0xff, 0xff, 0xff, 0xff),
        new(0xb6, 0xe1, 0xff, 0xff),
        new(0xce, 0xd1, 0xff, 0xff),
        new(0xe9, 0xc3, 0xff, 0xff),
        new(0xff, 0xbc, 0xff, 0xff),
        new(0xff, 0xbd, 0xf4, 0xff),
        new(0xff, 0xc6, 0xc3, 0xff),
        new(0xff, 0xd5, 0x9a, 0xff),
        new(0xe9, 0xe6, 0x81, 0xff),
        new(0xce, 0xf4, 0x81, 0xff),
        new(0xb6, 0xfb, 0x9a, 0xff),
        new(0xa9, 0xfa, 0xc3, 0xff),
        new(0xa9, 0xf0, 0xf4, 0xff),
        new(0xb8, 0xb8, 0xb8, 0xff),
        new(0x00, 0x00, 0x00, 0xff),
        new(0x00, 0x00, 0x00, 0xff),
    ];

    /// <summary>
    /// Constructs a new PPU and sets its framebuffer to a black screen
    /// </summary>
    /// <param name="mc">The memory controller to attach</param>
    /// <param name="channel">A reference to a channel to send NMIs to, receiving end should be given to a CPU</param>
    public Ppu(MemoryController mc, Channel<Cpu.InterruptSource> channel)
    {
        _channel = channel;
        _mc = mc;
        _frameBuffer = new int[256, 240];
        for (var i = 0; i < _frameBuffer.GetLength(0); i++)
        {
            for (var j = 0; j < _frameBuffer.GetLength(1); j++)
            {
                _frameBuffer[i, j] = 0x3f;
            }
        }

        for (var i = 0; i < _oam.Length; i++)
        {
            _oam[i] = new OamObject();
        }
    }

    /// <summary>
    /// Control register accessed by the CPU over the data bus
    /// </summary>
    /// <param name="value">Value on the bus</param>
    public void WritePpuCtrl(byte value)
    {
        _baseNametableAddress = (value & 0b11) switch
        {
            0 => 0x2000,
            1 => 0x2400,
            2 => 0x2800,
            3 => 0x2c00,
            _ => 0 // so the linter stops complaining about non-exhaustive switches
        };

        _vramAddressIncrementPerDataReadWrite = (byte)((value & 0b100) != 0 ? 32 : 1);
        _useHigherPatternTableSprites = (value & 0b1000) != 0;
        _useHigherPatternTableBackgrounds = (value & 0b10000) != 0;
        _useWideSprites = (value & 0b100000) != 0;
        _enableNmi = (value & 0b10000000) != 0;
    }

    public void WritePpuMask(byte value)
    {
        _greyscaleMode = (value & 0b1) != 0;
        _showBackgroundLeftmost = (value & 0b10) != 0;
        _showSpriteLeftmost = (value & 0b100) != 0;
        _showBackground = (value & 0b1000) != 0;
        _showSprites = (value & 0b10000) != 0;
        _emphasizeRed = (value & 0b100000) != 0;
        _emphasizeGreen = (value & 0b1000000) != 0;
        _emphasizeBlue = (value & 0b10000000) != 0;
    }

    /// <summary>
    /// Edits the 16-bit VRAM address, one byte at a time
    /// </summary>
    /// <param name="value">High byte on first call, low byte on second</param>
    public void WritePpuAddr(byte value)
    {
        if (W)
        {
            T &= 0b1111111100000000;
            T |= value;
            V = T;
            W = false;
        }
        else
        {
            value &= 0b111111;
            T &= 0b11111111;
            T |= (ushort)(value << 8);
            W = true;
        }
    }

    /// <summary>
    /// Writes to VRAM, based on address loaded into V register
    /// </summary>
    /// <param name="value">Value to write into implicit address</param>
    public void WritePpuData(byte value)
    {
        Console.WriteLine(V.ToString("x4") + ": " + value.ToString("x2"));
        _mc.Write(V, value);
        V += _vramAddressIncrementPerDataReadWrite;
    }

    /// <summary>
    /// Modifies the scrolling state across the VRAM
    /// </summary>
    /// <param name="value">X position on first call, Y position on second</param>
    public void WritePpuScroll(byte value)
    {
        if (W)
        {
            _yScroll = value;
            W = false;
        }
        else
        {
            _xScroll = value;
            W = true;
        }
    }

    /// <summary>
    /// Write a value into the address offset within the OAM
    /// </summary>
    /// <param name="value">Value to write</param>
    public void WriteOamAddr(byte value)
    {
        _oamAddress = value;
    }

    /// <summary>
    /// Write a value at the current OAM address
    /// </summary>
    /// <param name="value">Value to write</param>
    public void WriteOamData(byte value)
    {
        switch (_oamAddress % 4)
        {
            case 0:
                _oam[_oamAddress / 4].YCoordinate = value;
                break;
            case 1:
                _oam[_oamAddress / 4].TileNo = value;
                break;
            case 2:
                _oam[_oamAddress / 4].Attribute = value;
                break;
            case 3:
                _oam[_oamAddress / 4].XCoordinate = value;
                break;
        }

        _oamAddress++;
    }

    /// <summary>
    /// Gets the status of the PPU, to inform the CPU where the rendering process is
    /// </summary>
    /// <returns>The status bits, see NesDev wiki</returns>
    public byte ReadPpuStatus()
    {
        W = false;
        var ret = (_nmiFlag ? 0x80 : 0)
                  | (_spriteZeroHitFlag ? 0x40 : 0)
                  | (_spriteOverflowFlag ? 0x20 : 0);
        _nmiFlag = false;
        return (byte)ret;
    }

    public void DrawBackground()
    {
        if (!_showBackground) return;
        for (var x = 0; x < 256; x += 8)
        {
            for (var y = 0; y < 240; y += 8)
            {
            }
        }

    }

    /// <summary>
    /// Fetches a tile for the background, dependent on the background pattern table offset
    /// </summary>
    /// <param name="index">The index of the pattern, between 0 and 512, exclusively</param>
    /// <returns>A 8x8 array containing color indices between 0 and 3, inclusively</returns>
    public int[,] FetchTileBackground(int index)
    {
        ulong l1 = _mc.Read64((ushort)(index * 16 + (_useHigherPatternTableBackgrounds ? 0x1000 : 0)));
        ulong l2 = _mc.Read64((ushort)(index * 16 + 8 + (_useHigherPatternTableBackgrounds ? 0x1000 : 0)));
        var ret = new int[8, 8];

        var x = 0;
        var y = 0;
        for (var j = 63; j >= 0; j--)
        {
            if ((l1 & (1ul << j)) != 0)
            {
                if ((l2 & (1ul << j)) != 0)
                {
                    ret[x, y] = 3;
                }
                else
                {
                    ret[x, y] = 2;
                }
            }
            else
            {
                if ((l2 & (1ul << j)) != 0)
                {
                    ret[x, y] = 1;
                }
                else
                {
                    ret[x, y] = 0;
                }
            }

            x++;
            if (x != 8) continue;
            x %= 8;
            y++;
        }

        return ret;
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

            // The x offset for a sprite needs to increase by 8 for every 8 pixels across
            // 'i' increments by 16 for every 8 pixels, hence dividing by 2 and truncating
            // to screen width
            var xOffset = i / 2 % 256;

            // The y offset for a sprite needs to increase by 8 for every 256 pixels across
            // Again dividing by 2 to get an increase of 8 per sprite, dividing by 256 to get
            // an increase of 1 per 32 sprites (=256 pixels) and multiply by 8
            // to get 8 pixels per row of sprites
            var yOffset = i / 2 / 256 * 8;

            var x = 0;
            var y = 0;
            for (var j = 63; j >= 0; j--)
            {
                if ((l1 & (1ul << j)) != 0)
                {
                    if ((l2 & (1ul << j)) != 0)
                    {
                        // y is always stepped through in reverse, since y screen coordinates
                        // are flipped compared to the internal model of the CHR data
                        _frameBuffer[xOffset + x, yOffset + (7 - y)] = 0x3f;
                    }
                    else
                    {
                        _frameBuffer[xOffset + x, yOffset + (7 - y)] = 0x10;
                    }
                }
                else
                {
                    if ((l2 & (1ul << j)) != 0)
                    {
                        _frameBuffer[xOffset + x, yOffset + (7 - y)] = 0x00;
                    }
                    else
                    {
                        _frameBuffer[xOffset + x, yOffset + (7 - y)] = 0x20;
                    }
                }

                x++;
                if (x != 8) continue;
                x %= 8;
                y++;
            }
        }
    }

    /// <summary>
    /// Initializes the drawing routine, emulating the 256x240 video output generated by the PPU
    /// </summary>
    public void StartRendering()
    {
        // The PPU renders a total of 262 scan lines per frame, and calls the NMI on the CPU
        // once it enters the 241st. 241-262 are not visible, since they do not fit in the NES' resolution.
        // This makes the window between 241 and 262 theoretically the only safe place to write to
        // the PPU from the CPU, hence the NMI.

        Task.Run(() =>
        {
            Raylib.InitWindow(256, 240, "nes");
            Raylib.SetTargetFPS(60);
            while (!Raylib.WindowShouldClose())
            {
                _nmiFlag = false;
                _spriteZeroHitFlag = false;
                _spriteOverflowFlag = false;
                Raylib.PollInputEvents();
                Raylib.BeginDrawing();
                for (var i = 0; i < _frameBuffer.GetLength(0); i++)
                {
                    for (var j = 0; j < _frameBuffer.GetLength(1); j++)
                    {
                        Raylib.DrawPixel(i, j, Palette[_frameBuffer[i, j]]);
                    }
                }

                Raylib.EndDrawing();
                _nmiFlag = true;
                if (_enableNmi) _channel.Writer.TryWrite(Cpu.InterruptSource.Nmi);
            }

            Raylib.CloseWindow();
        });
    }
}