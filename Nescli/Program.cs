using System.Diagnostics;
using Raylib_cs;

namespace Nescli;

internal static class Program
{
    public static void Main()
    {
        using var f = File.OpenRead("/home/mvauderw/RiderProjects/Nescli/Nescli/smb1.nes");
        using BinaryReader reader = new(f);

        // ensure file header is intact
        if (!reader.ReadBytes(4).SequenceEqual(new byte[] { 0x4e, 0x45, 0x53, 0x1a }))
        {
            Console.WriteLine("File header invalid");
            return;
        }

        var file = new Ines(f.Length);

        file.AsmSize = reader.ReadByte();
        file.GraphicsSize = reader.ReadByte();
        file.HeaderUnused = reader.ReadBytes(10);

        Console.WriteLine("File size: " + file.FileSize + "b");
        Console.WriteLine("Program ROM size: " + file.AsmSize * 16384 + "b");
        Console.WriteLine("Graphics ROM size: " + file.GraphicsSize * 8192 + "b");

        var asmRom = reader.ReadBytes(16384 * file.AsmSize);
        var graphicsRom = reader.ReadBytes(8192 * file.GraphicsSize);

        var memoryControllerPpu = new MemoryController();
        memoryControllerPpu.AddMemory(new Rom(graphicsRom), 0x0000, 0x2000);
        var ppu = new Ppu(memoryControllerPpu);

        var memoryControllerCpu = new MemoryController();
        memoryControllerCpu.AddMemory(new PpuBusAdapter(ppu), 0x2000, 0x4000);
        memoryControllerCpu.AddMemory(new Rom(asmRom), 0x8000, 0x10000);
        var cpu = new Cpu(memoryControllerCpu);


        Task.Run(() =>
        {
            Raylib.InitWindow(256, 240, "Hello, World");
            Raylib.SetTargetFPS(60);
            while (!Raylib.WindowShouldClose())
            {
                Raylib.BeginDrawing();
                for (var i = 0; i < ppu.FrameBuffer.GetLength(0); i++)
                {
                    for (var j = 0; j < ppu.FrameBuffer.GetLength(1); j++)
                    {
                        Raylib.DrawPixel(i, j, ppu.FrameBuffer[i, j]);
                    }
                }
                Raylib.EndDrawing();
            }

            Raylib.CloseWindow();
        });

        Thread.Sleep(5000);

        // temporary implementation to help find unimplemented opcodes
        for (int i = 0; i < 100; i++) cpu.Run();
    }
}