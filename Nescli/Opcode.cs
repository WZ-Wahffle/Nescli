namespace Nescli;

/// <summary>
/// Discerns different opcodes
/// </summary>
public enum Opcode
{
    Adc,
    And,
    Asl,
    Bcc,
    Bcs,
    Beq,
    Bit,
    Bmi,
    Bne,
    Bpl,
    Bra,
    Brk,
    Bvc,
    Bvs,
    Clc,
    Cld,
    Cli,
    Clv,
    Cmp,
    Cpx,
    Cpy,
    Dec,
    Dex,
    Dey,
    Eor,
    Inc,
    Inx,
    Iny,
    Jmp,
    Jsr,
    Lda,
    Ldx,
    Ldy,
    Lsr,
    Nop,
    Ora,
    Pha,
    Php,
    Phx,
    Phy,
    Pla,
    Plp,
    Plx,
    Ply,
    Rol,
    Ror,
    Rti,
    Rts,
    Sbc,
    Sec,
    Sed,
    Sei,
    Sta,
    Stx,
    Sty,
    Stz,
    Tax,
    Tay,
    Trb,
    Tsb,
    Tsx,
    Txa,
    Txs,
    Tya,
}

/// <summary>
/// Discerns different addressing modes
/// </summary>
public enum AddressMode
{
    Immediate,
    Absolute,
    ZeroPage,
    Accumulator,
    Implied,
    IndexedIndirect, // ind. x
    IndirectIndexed, // ind. y
    IndexedZeroPageX, // zpg. x
    IndexedZeroPageY, // zpg. y
    IndexedAbsoluteX, // abs. x
    IndexedAbsoluteY, // abs. y
    Relative,
    AbsoluteIndirect, // (abs)
    AbsoluteIndexedIndirect, // abs (ind, x)
    ZeroPageIndirect, // (zpg)
}

/// <summary>
/// Responsible for decoding opcodes into enum members.
/// Can be static, since no internal state is saved here
/// </summary>
public static class Decoder
{
    /// <summary>
    /// Decodes an 8-bit value into a combination of opcode and addressing mode
    /// </summary>
    /// <param name="code">Value to decode</param>
    /// <returns>Resolved opcode and addressing mode</returns>
    /// <exception cref="IllegalOpcodeException">Thrown if code does not resolve to a valid opcode</exception>
    /// <exception cref="IllegalAddressModeException">Thrown if code does not resolve to a valid addressing mode</exception>
    public static Tuple<Opcode, AddressMode> Decode(byte code)
    {
        return new Tuple<Opcode, AddressMode>(code switch
        {
            0x00 => Opcode.Brk,
            0x10 => Opcode.Bpl,
            0x20 => Opcode.Jsr,
            0x30 => Opcode.Bmi,
            0x40 => Opcode.Rti,
            0x50 => Opcode.Bvc,
            0x60 => Opcode.Rts,
            0x70 => Opcode.Bvs,
            0x80 => Opcode.Bra,
            0x90 => Opcode.Bcc,
            0xa0 or 0xa4 or 0xb4 or 0xac or 0xbc => Opcode.Ldy,
            0xb0 => Opcode.Bcs,
            0xc0 or 0xc4 or 0xcc => Opcode.Cpy,
            0xd0 => Opcode.Bne,
            0xe0 or 0xe4 or 0xec => Opcode.Cpx,
            0xf0 => Opcode.Beq,
            0x01 or 0x11 or 0x12 or 0x05 or 0x15 or 0x09 or 0x19 or 0x0d or 0x1d => Opcode.Ora,
            0x21 or 0x31 or 0x32 or 0x25 or 0x35 or 0x29 or 0x39 or 0x2d or 0x3d => Opcode.And,
            0x41 or 0x51 or 0x52 or 0x45 or 0x55 or 0x49 or 0x59 or 0x4d or 0x5d => Opcode.Eor,
            0x61 or 0x71 or 0x72 or 0x65 or 0x75 or 0x69 or 0x79 or 0x6d or 0x7d => Opcode.Adc,
            0x81 or 0x91 or 0x92 or 0x85 or 0x95 or 0x99 or 0x8d or 0x9d => Opcode.Sta,
            0xa1 or 0xb1 or 0xb2 or 0xa5 or 0xb5 or 0xa9 or 0xb9 or 0xad or 0xbd => Opcode.Lda,
            0xc1 or 0xd1 or 0xd2 or 0xc5 or 0xd5 or 0xc9 or 0xd9 or 0xcd or 0xdd => Opcode.Cmp,
            0xe1 or 0xf1 or 0xf2 or 0xe5 or 0xf5 or 0xe9 or 0xf9 or 0xed or 0xfd => Opcode.Sbc,
            0xa2 or 0xa6 or 0xb6 or 0xae or 0xbe => Opcode.Ldx,
            0x04 or 0x0c => Opcode.Tsb,
            0x14 or 0x1c => Opcode.Trb,
            0x24 or 0x34 or 0x89 or 0x2c or 0x3c => Opcode.Bit,
            0x64 or 0x74 or 0x9c or 0x9e => Opcode.Stz,
            0x84 or 0x94 or 0x8c => Opcode.Sty,
            0x06 or 0x16 or 0x0a or 0x0e or 0x1e => Opcode.Asl,
            0x26 or 0x36 or 0x2a or 0x2e or 0x3e => Opcode.Rol,
            0x46 or 0x56 or 0x4a or 0x4e or 0x5e => Opcode.Lsr,
            0x66 or 0x76 or 0x6a or 0x6e or 0x7e => Opcode.Ror,
            0x86 or 0x96 or 0x8e => Opcode.Stx,
            0xc6 or 0xd6 or 0x3a or 0xce or 0xde => Opcode.Dec,
            0xe6 or 0xf6 or 0x1a or 0xee or 0xfe => Opcode.Inc,
            0x08 => Opcode.Php,
            0x18 => Opcode.Clc,
            0x28 => Opcode.Plp,
            0x38 => Opcode.Sec,
            0x48 => Opcode.Pha,
            0x58 => Opcode.Cli,
            0x68 => Opcode.Pla,
            0x78 => Opcode.Sei,
            0x88 => Opcode.Dey,
            0x98 => Opcode.Tya,
            0xa8 => Opcode.Tay,
            0xb8 => Opcode.Clv,
            0xc8 => Opcode.Iny,
            0xd8 => Opcode.Cld,
            0xe8 => Opcode.Inx,
            0xf8 => Opcode.Sed,
            0x5a => Opcode.Phy,
            0x7a => Opcode.Ply,
            0x8a => Opcode.Txa,
            0x9a => Opcode.Txs,
            0xaa => Opcode.Tax,
            0xba => Opcode.Tsx,
            0xca => Opcode.Dex,
            0xda => Opcode.Phx,
            0xea => Opcode.Nop,
            0xfa => Opcode.Plx,
            0x4c or 0x6c or 0x7c => Opcode.Jmp,
            _ => throw new IllegalOpcodeException(code)
        }, code switch
        {
            0x69 or 0x29 or 0x89 or 0xc9 or 0xe0 or 0xc0 or 0x49 or 0xa9 or 0xa2 or 0xa0 or 0x09 or 0xe9 => AddressMode
                .Immediate,
            0x6d or 0x2d or 0x0e or 0x2c or 0xcd or 0xec or 0xcc or 0xce or 0x4d or 0xee or 0x4c or 0x20 or 0xad or 0xae
                or 0xac or 0x4e or 0x0d or 0x2e or 0x6e or 0xed or 0x8d or 0x8d or 0x8e or 0x8c or 0x9c or 0x1c
                or 0x0c => AddressMode.Absolute,
            0x65 or 0x25 or 0x06 or 0x24 or 0xc5 or 0xe4 or 0xc4 or 0xc6 or 0x45 or 0xe6 or 0xa5 or 0xa6 or 0xa4 or 0x46
                or 0x05 or 0x26 or 0x66 or 0xe5 or 0x85 or 0x86 or 0x84 or 0x64 or 0x14 or 0x04 => AddressMode.ZeroPage,
            0x0a or 0x1a or 0x2a or 0x3a or 0x4a or 0x6a => AddressMode.Accumulator,
            0x00 or 0x18 or 0xd8 or 0x58 or 0xb8 or 0xca or 0x88 or 0xe8 or 0xc8 or 0xea or 0x48 or 0x08 or 0xda or 0x5a
                or 0x68 or 0x28 or 0xfa or 0x7a or 0x40 or 0x60 or 0x38 or 0xf8 or 0x78 or 0xaa or 0xa8 or 0xba or 0x8a
                or 0x9a or 0x98 => AddressMode.Implied,
            0x61 or 0x21 or 0xc1 or 0x41 or 0xa1 or 0x01 or 0xe1 or 0x81 => AddressMode.IndexedIndirect,
            0x71 or 0x31 or 0xd1 or 0x51 or 0xb1 or 0x11 or 0xf1 or 0x91 => AddressMode.IndirectIndexed,
            0x75 or 0x35 or 0x16 or 0x34 or 0xd5 or 0xd6 or 0x55 or 0xf6 or 0xb5 or 0xb4 or 0x56 or 0x15 or 0x36 or 0x76
                or 0xf5 or 0x95 or 0x94 or 0x74 => AddressMode.IndexedZeroPageX,
            0xb6 or 0x96 => AddressMode.IndexedZeroPageY,
            0x7d or 0x3d or 0x1e or 0x3c or 0xdd or 0xde or 0x5d or 0xfe or 0xbd or 0xbc or 0x5e or 0x1d or 0x3e or 0x7e
                or 0xfd or 0x9d or 0x9e => AddressMode.IndexedAbsoluteX,
            0x79 or 0x39 or 0xd9 or 0x59 or 0xb9 or 0xbe or 0x19 or 0xf9 or 0x99 => AddressMode.IndexedAbsoluteY,
            0x90 or 0xb0 or 0xf0 or 0x30 or 0xd0 or 0x10 or 0x80 or 0x50 or 0x70 => AddressMode.Relative,
            0x6c => AddressMode.AbsoluteIndirect,
            0x7c => AddressMode.AbsoluteIndexedIndirect,
            0x72 or 0x32 or 0xd2 or 0x52 or 0xb2 or 0x12 or 0xf2 or 0x92 => AddressMode.ZeroPageIndirect,
            _ => throw new IllegalAddressModeException(code)
        });
    }

    /// <summary>
    /// Resolves the number of extra bytes to be read for a given addressing mode
    /// </summary>
    /// <param name="addr">The addressing mode to resolve</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if addr is not within the range of the backing Enum</exception>
    /// <returns>The number of extra bytes to account for</returns>
    public static int ResolveRemainingBytes(AddressMode addr)
    {
        return addr switch
        {
            AddressMode.Immediate => 1,
            AddressMode.Absolute => 2,
            AddressMode.ZeroPage => 1,
            AddressMode.Accumulator => 0,
            AddressMode.Implied => 0,
            AddressMode.IndexedIndirect => 1,
            AddressMode.IndirectIndexed => 1,
            AddressMode.IndexedZeroPageX => 1,
            AddressMode.IndexedZeroPageY => 1,
            AddressMode.IndexedAbsoluteX => 2,
            AddressMode.IndexedAbsoluteY => 2,
            AddressMode.Relative => 1,
            AddressMode.AbsoluteIndirect => 2,
            AddressMode.AbsoluteIndexedIndirect => 2,
            AddressMode.ZeroPageIndirect => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(addr), addr, null)
        };
    }
}