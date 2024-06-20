using System;
using System.IO;
using System.Linq;
using System.Threading.Channels;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nescli.Tests;

[TestClass]
[TestSubject(typeof(Ppu))]
public class PpuTest
{
    //[TestMethod]
    public void TestDrawing()
    {
        using var f = File.OpenRead("../../../color_test.nes");
        using BinaryReader reader = new(f);
        Assert.IsTrue(reader.ReadBytes(4).SequenceEqual(new byte[] { 0x4e, 0x45, 0x53, 0x1a }));
        var file = new Ines(reader.ReadBytes(12));

        var asmRom = reader.ReadBytes(file.AsmSize);
        var graphicsRom = reader.ReadBytes(file.GraphicsSize);

        var channel = Channel.CreateBounded<Cpu.InterruptSource>(10);

        var memoryControllerPpu = new MemoryController();
        memoryControllerPpu.AddMemory(new Rom(graphicsRom), 0x0000, 0x2000);
        memoryControllerPpu.AddMemory(new Ram(0x1000), 0x2000, 0x3000);
        var ppu = new Ppu(memoryControllerPpu, channel);

        var apu = new Apu();

        var memoryControllerCpu = new MemoryController();
        memoryControllerCpu.AddMemory(new PpuBusAdapter(ppu), 0x2000, 0x4000);
        memoryControllerCpu.AddMemory(new ApuBusAdapter(apu), 0x4000, 0x4018);
        memoryControllerCpu.AddMemory(new MirroredRom(asmRom, 0x8000), 0x8000, 0x10000);
        memoryControllerCpu.AddMemory(new Ram(0x800), 0x0, 0x800);
        var cpu = new Cpu(memoryControllerCpu, channel);

        channel.Writer.TryWrite(Cpu.InterruptSource.Reset);

        ppu.GenerateSpritesheet();

        ppu.StartRendering();

        while (true)
        {
            cpu.Run();
        }
    }
}