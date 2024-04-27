using Nescli;

var f = File.OpenRead("/home/mvauderw/RiderProjects/Nescli/Nescli/smb1.nes");
if (f == Stream.Null)
{
    Console.WriteLine("Invalid file");
    return 1;
}

{
    var b = new byte[4];
    f.ReadExactly(b, 0, 4);
    if (!b.SequenceEqual(new byte[] { 0x4e, 0x45, 0x53, 0x1a }))
    {
        Console.WriteLine("File header invalid");
        return 1;
    }
}

var file = new Ines();
file.fileSize = f.Length;

{
    var b = new byte[1];
    f.ReadExactly(b, 0, 1);
    file.asmSize = b[0];
    f.ReadExactly(b, 0, 1);
    file.graphicsSize = b[0];
}
{
    var b = new byte[10];
    f.ReadExactly(b, 0, 10);
    file._headerUnused = (byte[])b.Clone();
}
Console.WriteLine("File size: " + file.fileSize + "b");
Console.WriteLine("Program ROM size: " + file.asmSize * 16384 + "b");
Console.WriteLine("Graphics ROM size: " + file.graphicsSize * 8192 + "b");

var asmRom = new byte[16384 * file.asmSize];
f.ReadExactly(asmRom, 0, 16384 * file.asmSize);
var graphicsRom = new byte[8192 * file.graphicsSize];
f.ReadExactly(graphicsRom, 0, 8192 * file.graphicsSize);
f.Close();

var memoryController = new MemoryController();
memoryController.AddMemory(new Rom(asmRom), 0x8000, 0x10000);
var cpu = new Cpu(memoryController);

// temporary implementation to help find unimplemented opcodes
for(int i = 0; i < 100; i++) cpu.Run();

return 0;