namespace Nescli;

/// <summary>
/// Adapter to make the control registers for the APU eligible for the CPU
/// and its data bus
/// </summary>
public class ApuBusAdapter : IMemory
{
    private Apu _apu;

    public ApuBusAdapter(Apu apu)
    {
        _apu = apu;
    }

    public byte Read(ushort position)
    {
        throw new NotImplementedException();
    }

    public void Write(ushort position, byte value)
    {
        switch (position)
        {
            case 0x15:
                _apu.SetStatus(value);
                break;
            default:
                throw new NotImplementedException();
        }
    }
}