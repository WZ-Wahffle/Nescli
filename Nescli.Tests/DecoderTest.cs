using System;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nescli;

namespace Nescli.Tests;

[TestClass]
[TestSubject(typeof(Decoder))]
public class DecoderTest
{

    /// <summary>
    /// Test a few random opcodes to ensure the fabric of reality is intact
    /// </summary>
    [TestMethod]
    public void TestOpcodes()
    {
        Assert.AreEqual(Decoder.Decode(0x76), new Tuple<Opcode, AddressMode>(Opcode.Ror, AddressMode.IndexedZeroPageX));
        Assert.AreEqual(Decoder.Decode(0x89), new Tuple<Opcode, AddressMode>(Opcode.Bit, AddressMode.Immediate));
        Assert.AreEqual(Decoder.Decode(0x7c), new Tuple<Opcode, AddressMode>(Opcode.Jmp, AddressMode.AbsoluteIndexedIndirect));
        Assert.AreEqual(Decoder.Decode(0xda), new Tuple<Opcode, AddressMode>(Opcode.Phx, AddressMode.Implied));
    }
}