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
        memCtl.AddMemory(new Rom(0xffff), 0x0000, 0x10000);
        Channel<Cpu.InterruptSource> c = Channel.CreateBounded<Cpu.InterruptSource>(10);
        var cpu = new Cpu(memCtl, c);
        Assert.IsNotNull(cpu);
        cpu.Execute(new Instruction(Opcode.Lda, AddressMode.Immediate, [17]));
        Assert.AreEqual(cpu.A, 17);
    }
}