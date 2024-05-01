namespace Nescli;

/// <summary>
/// Adapter to make the control registers for the PPU eligible for the CPU
/// and its data bus
/// </summary>
public class PpuBusAdapter : IMemory
{
    private Ppu _ppu;

    public PpuBusAdapter(Ppu ppu)
    {
        _ppu = ppu;
    }

    public byte Read(ushort position)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// External writing to PPU
    /// </summary>
    /// <param name="position">Position to write to, mod 8</param>
    /// <param name="value">The value to write there</param>
    public void Write(ushort position, byte value)
    {
        switch (position % 8)
        {
            case 0:
                _ppu.PpuCtrl(value);
                break;
        }
    }
}