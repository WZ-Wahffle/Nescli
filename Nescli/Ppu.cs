using Raylib_cs;

namespace Nescli;

public class Ppu
{
    private readonly MemoryController _mc;
    public readonly Color[,] FrameBuffer;
    private ushort _baseNametableAddress;

    public Ppu(MemoryController mc)
    {
        _mc = mc;
        FrameBuffer = new Color[256,240];
        for (var i = 0; i < FrameBuffer.GetLength(0); i++)
        {
            for (var j = 0; j < FrameBuffer.GetLength(1); j++)
            {
                FrameBuffer[i, j] = new Color(0, 0, 0, 255);
            }
        }
    }

    public void PpuCtrl(byte value)
    {
        _baseNametableAddress = (value & 0b11) switch
        {
            0 => 0x2000,
            1 => 0x2400,
            2 => 0x2800,
            3 => 0x2c00,
            _ => throw new ArgumentOutOfRangeException() // primarily so the linter shuts up
        };
    }
}