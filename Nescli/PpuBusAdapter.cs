namespace Nescli;

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