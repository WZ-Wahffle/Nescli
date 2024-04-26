namespace Nescli;

/// <summary>
/// Discerns different opcodes, as well as an extra entry for invalid opcodes
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
    InvalidOpcode
}

/// <summary>
/// Discerns different addressing modes,
/// may be turned into implementations of an interface in the future
/// </summary>
public enum AddressMode
{
    Accumulator,
    Immediate,
    Absolute,
    ZeroPage,
    IndexedZeroPage,
    IndexedAbsolute,
    Implied,
    Relative,
    IndexedIndirect,
    IndirectIndexed,
    AbsoluteIndirect
}

/// <summary>
/// Responsible for decoding opcodes into enum members.
/// Can be static, since no internal state is saved here
/// </summary>
public static class Decoder
{
    /// <summary>
    /// Decodes an 8-bit value into its corresponding opcode
    /// </summary>
    /// <param name="code">The value to decode</param>
    /// <returns>The corresponding opcode</returns>
    public static Opcode Decode(byte code)
    {
        switch (code)
        {
            case 0x00: return Opcode.Brk;
            case 0x10: return Opcode.Bpl;
            case 0x20: return Opcode.Jsr;
            case 0x30: return Opcode.Bmi;
            case 0x40: return Opcode.Rti;
            case 0x50: return Opcode.Bvc;
            case 0x60: return Opcode.Rts;
            case 0x70: return Opcode.Bvs;
            case 0x80: return Opcode.Bra;
            case 0x90: return Opcode.Bcc;
            case 0xa0:
            case 0xa4:
            case 0xb4:
            case 0xac:
            case 0xbc: return Opcode.Ldy;
            case 0xb0: return Opcode.Bcs;
            case 0xc0:
            case 0xc4:
            case 0xcc: return Opcode.Cpy;
            case 0xd0: return Opcode.Bne;
            case 0xe0:
            case 0xe4:
            case 0xec: return Opcode.Cpx;
            case 0xf0: return Opcode.Beq;
            case 0x01:
            case 0x11:
            case 0x12:
            case 0x05:
            case 0x15:
            case 0x09:
            case 0x19:
            case 0x0d:
            case 0x1d: return Opcode.Ora;
            case 0x21:
            case 0x31:
            case 0x32:
            case 0x25:
            case 0x35:
            case 0x29:
            case 0x39:
            case 0x2d:
            case 0x3d: return Opcode.And;
            case 0x41:
            case 0x51:
            case 0x52:
            case 0x45:
            case 0x55:
            case 0x49:
            case 0x59:
            case 0x4d:
            case 0x5d: return Opcode.Eor;
            case 0x61:
            case 0x71:
            case 0x72:
            case 0x65:
            case 0x75:
            case 0x69:
            case 0x79:
            case 0x6d:
            case 0x7d: return Opcode.Adc;
            case 0x81:
            case 0x91:
            case 0x92:
            case 0x85:
            case 0x95:
            case 0x99:
            case 0x8d:
            case 0x9d: return Opcode.Sta;
            case 0xa1:
            case 0xb1:
            case 0xb2:
            case 0xa5:
            case 0xb5:
            case 0xa9:
            case 0xb9:
            case 0xad:
            case 0xbd: return Opcode.Lda;
            case 0xc1:
            case 0xd1:
            case 0xd2:
            case 0xc5:
            case 0xd5:
            case 0xc9:
            case 0xd9:
            case 0xcd:
            case 0xdd: return Opcode.Cmp;
            case 0xe1:
            case 0xf1:
            case 0xf2:
            case 0xe5:
            case 0xf5:
            case 0xe9:
            case 0xf9:
            case 0xed:
            case 0xfd: return Opcode.Sbc;
            case 0xa2:
            case 0xa6:
            case 0xb6:
            case 0xae:
            case 0xbe: return Opcode.Ldx;
            case 0x04:
            case 0x0c: return Opcode.Tsb;
            case 0x14:
            case 0x1c: return Opcode.Trb;
            case 0x24:
            case 0x34:
            case 0x89:
            case 0x2c:
            case 0x3c: return Opcode.Bit;
            case 0x64:
            case 0x74:
            case 0x9c:
            case 0x9e: return Opcode.Stz;
            case 0x84:
            case 0x94:
            case 0x8c: return Opcode.Sty;
            case 0x06:
            case 0x16:
            case 0x0a:
            case 0x0e:
            case 0x1e: return Opcode.Asl;
            case 0x26:
            case 0x36:
            case 0x2a:
            case 0x2e:
            case 0x3e: return Opcode.Rol;
            case 0x46:
            case 0x56:
            case 0x4a:
            case 0x4e:
            case 0x5e: return Opcode.Lsr;
            case 0x66:
            case 0x76:
            case 0x6a:
            case 0x6e:
            case 0x7e: return Opcode.Ror;
            case 0x86:
            case 0x96:
            case 0x8e: return Opcode.Stx;
            case 0xc6:
            case 0xd6:
            case 0x3a:
            case 0xce:
            case 0xde: return Opcode.Dec;
            case 0xe6:
            case 0xf6:
            case 0x1a:
            case 0xee:
            case 0xfe: return Opcode.Inc;
            case 0x08: return Opcode.Php;
            case 0x18: return Opcode.Clc;
            case 0x28: return Opcode.Plp;
            case 0x38: return Opcode.Sec;
            case 0x48: return Opcode.Pha;
            case 0x58: return Opcode.Cli;
            case 0x68: return Opcode.Pla;
            case 0x78: return Opcode.Sei;
            case 0x88: return Opcode.Dey;
            case 0x98: return Opcode.Tya;
            case 0xa8: return Opcode.Tay;
            case 0xb8: return Opcode.Clv;
            case 0xc8: return Opcode.Iny;
            case 0xd8: return Opcode.Cld;
            case 0xe8: return Opcode.Inx;
            case 0xf8: return Opcode.Sed;
            case 0x5a: return Opcode.Phy;
            case 0x7a: return Opcode.Ply;
            case 0x8a: return Opcode.Txa;
            case 0x9a: return Opcode.Txs;
            case 0xaa: return Opcode.Tax;
            case 0xba: return Opcode.Tsx;
            case 0xca: return Opcode.Dex;
            case 0xda: return Opcode.Phx;
            case 0xea: return Opcode.Nop;
            case 0xfa: return Opcode.Plx;
            case 0x4c:
            case 0x6c:
            case 0x7c: return Opcode.Jmp;

            default:
                return Opcode.InvalidOpcode;
        }
    }
}