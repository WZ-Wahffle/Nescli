﻿using System.Threading.Channels;

namespace Nescli;

internal static class Program
{
    public static void Main(string[] args)
    {
        using var f = File.OpenRead(args[0]);
        using BinaryReader reader = new(f);

        // ensure file header is intact
        if (!reader.ReadBytes(4).SequenceEqual(new byte[] { 0x4e, 0x45, 0x53, 0x1a }))
        {
            throw new ArgumentException("Invalid iNES file header");
        }

        var file = new Ines(reader.ReadBytes(12));

        Console.WriteLine("Program ROM size: " + file.AsmSize + "b");
        Console.WriteLine("Graphics ROM size: " + file.GraphicsSize + "b");

        var asmRom = reader.ReadBytes(file.AsmSize);
        var graphicsRom = reader.ReadBytes(file.GraphicsSize);

        var channel = Channel.CreateBounded<Cpu.InterruptSource>(10);

        var memoryControllerPpu = new MemoryController();
        memoryControllerPpu.AddMemory(new Rom(graphicsRom), 0x0000, 0x2000);
        memoryControllerPpu.AddMemory(new Ram(0x1000), 0x2000, 0x3000);
        memoryControllerPpu.AddMemory(new MirroredRam(0x20), 0x3f00, 0x3fff);
        var ppu = new Ppu(memoryControllerPpu, channel);

        var apu = new Apu();

        var memoryControllerCpu = new MemoryController();
        memoryControllerCpu.AddMemory(new MirroredRam(0x800), 0x0, 0x2000);
        memoryControllerCpu.AddMemory(new PpuBusAdapter(ppu), 0x2000, 0x4000);
        memoryControllerCpu.AddMemory(new ApuBusAdapter(apu), 0x4000, 0x4018);
        memoryControllerCpu.AddMemory(new MirroredRom(asmRom, 0x8000), 0x8000, 0x10000);
        var cpu = new Cpu(memoryControllerCpu, channel);

        channel.Writer.TryWrite(Cpu.InterruptSource.Reset);

        ppu.GenerateSpritesheet();

        ppu.StartRendering();

        // temporary implementation to help find unimplemented opcodes
        while(true) cpu.Run();
    }
}