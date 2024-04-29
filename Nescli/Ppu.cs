using Raylib_cs;

namespace Nescli;

public class Ppu
{
    private readonly MemoryController _mc;
    public readonly Color[,] FrameBuffer;

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
}