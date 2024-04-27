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

        var file = new Ines();
        file.fileSize = f.Length;

        file.asmSize = reader.ReadByte();
        file.graphicsSize = reader.ReadByte();
        file._headerUnused = reader.ReadBytes(10);

        Console.WriteLine("File size: " + file.fileSize + "b");
        Console.WriteLine("Program ROM size: " + file.asmSize * 16384 + "b");
        Console.WriteLine("Graphics ROM size: " + file.graphicsSize * 8192 + "b");

        var asmRom = reader.ReadBytes(16384 * file.asmSize);
        var graphicsRom = reader.ReadBytes(8192 * file.graphicsSize);

        var memoryController = new MemoryController();
        memoryController.AddMemory(new Rom(asmRom), 0x8000, 0x10000);
        var cpu = new Cpu(memoryController);

        // temporary implementation to help find unimplemented opcodes
        for (int i = 0; i < 100; i++) cpu.Run();
    }
}