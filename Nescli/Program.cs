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
            throw new ArgumentException("Invalid iNES file header");
        }

        var file = new Ines(reader.ReadBytes(12));

        Console.WriteLine("Program ROM size: " + file.AsmSize + "b");
        Console.WriteLine("Graphics ROM size: " + file.GraphicsSize + "b");

        var asmRom = reader.ReadBytes(file.AsmSize);
        var graphicsRom = reader.ReadBytes(file.GraphicsSize);

        var memoryControllerPpu = new MemoryController();
        memoryControllerPpu.AddMemory(new Rom(graphicsRom), 0x0000, 0x2000);
        var ppu = new Ppu(memoryControllerPpu);

        var apu = new Apu();

        var memoryControllerCpu = new MemoryController();
        memoryControllerCpu.AddMemory(new PpuBusAdapter(ppu), 0x2000, 0x4000);
        memoryControllerCpu.AddMemory(new ApuBusAdapter(apu), 0x4000, 0x4018);
        memoryControllerCpu.AddMemory(new Rom(asmRom), 0x8000, 0x10000);
        memoryControllerCpu.AddMemory(new Ram(0x800), 0x0, 0x800);
        var cpu = new Cpu(memoryControllerCpu);
        cpu.Interrupt(Cpu.InterruptSource.Reset);


        ppu.GenerateSpritesheet();

        ppu.StartRendering();

        Thread.Sleep(1000);

        // temporary implementation to help find unimplemented opcodes
        while(true) cpu.Run();
    }
}