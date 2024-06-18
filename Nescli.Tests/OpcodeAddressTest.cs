using System.Threading.Channels;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nescli.Tests;

[TestClass]
[TestSubject(typeof(Cpu))]
public class OpcodeAddressTest
{
    [TestMethod]
    public void TestAddressing()
    {
        MemoryController memCtl = new MemoryController();
        memCtl.AddMemory(new Ram(0xffff), 0x0000, 0x10000);
        Channel<Cpu.InterruptSource> c = Channel.CreateBounded<Cpu.InterruptSource>(10);
        var cpu = new Cpu(memCtl, c);

        // Immediate
        cpu.Execute(new Instruction(Opcode.Lda, AddressMode.Immediate, [0x10]));
        Assert.AreEqual(cpu.A, 0x10);

        // Absolute
        cpu.Execute(new Instruction(Opcode.Lda, AddressMode.Immediate, [0x10]));
        cpu.Execute(new Instruction(Opcode.Sta, AddressMode.Absolute, [0x03, 0x00]));
        Assert.AreEqual(cpu.Read(0x0003), cpu.A);

        // Zero-Page
        cpu.Execute(new Instruction(Opcode.Lda, AddressMode.Immediate, [0x11]));
        cpu.Execute(new Instruction(Opcode.Sta, AddressMode.ZeroPage, [0x04]));
        Assert.AreEqual(cpu.Read(0x0004), cpu.A);

        // Accumulated
        cpu.Execute(new Instruction(Opcode.Lda, AddressMode.Immediate, [0x12]));
        cpu.Execute(new Instruction(Opcode.Asl, AddressMode.Accumulator, []));
        Assert.AreEqual(cpu.A, 0x24);

        // Implied
        cpu.Execute(new Instruction(Opcode.Ldx, AddressMode.Immediate, [0x15]));
        cpu.Execute(new Instruction(Opcode.Inx, AddressMode.Implied, []));
        Assert.AreEqual(cpu.X, 0x16);

        // Indexed Indirect (X)
        cpu.Execute(new Instruction(Opcode.Lda, AddressMode.Immediate, [0x20]));
        cpu.Execute(new Instruction(Opcode.Sta, AddressMode.ZeroPage, [0x50]));
        cpu.Execute(new Instruction(Opcode.Lda, AddressMode.Immediate, [0x00]));
        cpu.Execute(new Instruction(Opcode.Sta, AddressMode.ZeroPage, [0x51]));

        cpu.Execute(new Instruction(Opcode.Ldx, AddressMode.Immediate, [0x28]));
        cpu.Execute(new Instruction(Opcode.Lda, AddressMode.Immediate, [0x17]));
        cpu.Execute(new Instruction(Opcode.Sta, AddressMode.IndexedIndirect, [0x28]));
        Assert.AreEqual(cpu.Read(0x20), 0x17);

        // Indirect Indexed (Y)
        cpu.Execute(new Instruction(Opcode.Lda, AddressMode.Immediate, [0x28]));
        cpu.Execute(new Instruction(Opcode.Sta, AddressMode.ZeroPage, [0x86]));
        cpu.Execute(new Instruction(Opcode.Lda, AddressMode.Immediate, [0x40]));
        cpu.Execute(new Instruction(Opcode.Sta, AddressMode.ZeroPage, [0x87]));

        cpu.Execute(new Instruction(Opcode.Ldy, AddressMode.Immediate, [0x10]));
        cpu.Execute(new Instruction(Opcode.Lda, AddressMode.Immediate, [0x41]));
        cpu.Execute(new Instruction(Opcode.Sta, AddressMode.IndirectIndexed, [0x86]));
        Assert.AreEqual(cpu.Read(0x4038), 0x41);

        // Zero-Paged Indexed X

        cpu.Execute(new Instruction(Opcode.Lda, AddressMode.Immediate, [0x65]));
        cpu.Execute(new Instruction(Opcode.Ldx, AddressMode.Immediate, [0x15]));
        cpu.Execute(new Instruction(Opcode.Sta, AddressMode.IndexedZeroPageX, [0x60]));
        Assert.AreEqual(cpu.Read(0x75), 0x65);

        // Zero-Paged Indexed Y

        cpu.Execute(new Instruction(Opcode.Ldx, AddressMode.Immediate, [0x66]));
        cpu.Execute(new Instruction(Opcode.Ldy, AddressMode.Immediate, [0x16]));
        cpu.Execute(new Instruction(Opcode.Stx, AddressMode.IndexedZeroPageY, [0x60]));
        Assert.AreEqual(cpu.Read(0x76), 0x66);


    }
}