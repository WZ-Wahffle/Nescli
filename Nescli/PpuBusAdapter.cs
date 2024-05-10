namespace Nescli;

/// <summary>
/// Adapter to make the control registers for the PPU eligible for the CPU
/// and its data bus
/// </summary>
public class PpuBusAdapter : IMemory
{
    private readonly Ppu _ppu;

    public PpuBusAdapter(Ppu ppu)
    {
        _ppu = ppu;
    }

    private enum PpuRegister
    {
        PpuCtrl,
        PpuMask,
        PpuStatus,
        OamAddr,
        OamData,
        PpuScroll,
        PpuAddr,
        PpuData,
        OamDma
    }

    /// <summary>
    /// External reading from PPU registers
    /// </summary>
    /// <param name="position">Position to read from, mod 8</param>
    /// <returns>The byte in the specified register</returns>
    /// <exception cref="MemoryAccessViolationException">Thrown if register is write-only</exception>
    public byte Read(ushort position)
    {
        return (PpuRegister)(position % 8) switch
        {
            PpuRegister.PpuCtrl => throw new MemoryAccessViolationException(),
            PpuRegister.PpuStatus => _ppu.ReadPpuStatus(),
            _ => throw new NotImplementedException()
        };
    }

    /// <summary>
    /// External writing to PPU registers
    /// </summary>
    /// <param name="position">Position to write to, mod 8</param>
    /// <param name="value">The value to write there</param>
    public void Write(ushort position, byte value)
    {
        switch ((PpuRegister)(position % 8))
        {
            case PpuRegister.PpuCtrl:
                _ppu.WritePpuCtrl(value);
                break;
            case PpuRegister.PpuMask:
                _ppu.WritePpuMask(value);
                break;
            case PpuRegister.OamAddr:
                _ppu.WriteOamAddr(value);
                break;
            case PpuRegister.OamData:
                _ppu.WriteOamData(value);
                break;
            case PpuRegister.PpuScroll:
                _ppu.WritePpuScroll(value);
                break;
            case PpuRegister.PpuAddr:
                _ppu.WritePpuAddr(value);
                break;
            case PpuRegister.PpuData:
                _ppu.WritePpuData(value);
                break;
            default:
                throw new NotImplementedException();
        }
    }
}