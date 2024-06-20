using Raylib_cs;

namespace Nescli;

/// <summary>
/// Adapter to make the control registers for the APU eligible for the CPU
/// and its data bus, IO is included here too since it is fully encompassed
/// in the address space
/// </summary>
public class ApuBusAdapter : IMemory
{
    private readonly Apu _apu;
    private byte _ioShiftRegisterP1;
    private byte _ioShiftRegisterP2;

    public ApuBusAdapter(Apu apu)
    {
        _apu = apu;
    }

    public byte Read(ushort position)
    {
        byte ret;
        switch (position)
        {
            case 0x16:
                ret = (byte)(_ioShiftRegisterP1 & 1);
                _ioShiftRegisterP1 >>= 1;
                break;
            case 0x17:
                ret = (byte)(_ioShiftRegisterP2 & 1);
                _ioShiftRegisterP2 >>= 1;
                break;
            default:
                throw new NotImplementedException(position.ToString());
        }

        return ret;
    }

    public void Write(ushort position, byte value)
    {
        switch (position)
        {
            case 0x11:
                _apu.SetDmcValue(value);
                break;
            case 0x15:
                _apu.SetStatus(value);
                break;
            case 0x16:
                if ((value & 1) == 0)
                {
                    Raylib.PollInputEvents();
                    _ioShiftRegisterP1 = 0;
                    _ioShiftRegisterP1 |= (byte)((Raylib.IsKeyDown(KeyboardKey.A) ? 1 : 0) << 0);
                    _ioShiftRegisterP1 |= (byte)((Raylib.IsKeyDown(KeyboardKey.B) ? 1 : 0) << 1);
                    _ioShiftRegisterP1 |= (byte)((Raylib.IsKeyDown(KeyboardKey.Backspace) ? 1 : 0) << 2);
                    _ioShiftRegisterP1 |= (byte)((Raylib.IsKeyDown(KeyboardKey.Enter) ? 1 : 0) << 3);
                    _ioShiftRegisterP1 |= (byte)((Raylib.IsKeyDown(KeyboardKey.Up) ? 1 : 0) << 4);
                    _ioShiftRegisterP1 |= (byte)((Raylib.IsKeyDown(KeyboardKey.Down) ? 1 : 0) << 5);
                    _ioShiftRegisterP1 |= (byte)((Raylib.IsKeyDown(KeyboardKey.Left) ? 1 : 0) << 6);
                    _ioShiftRegisterP1 |= (byte)((Raylib.IsKeyDown(KeyboardKey.Right) ? 1 : 0) << 7);
                }
                break;
            case 0x17:
                    _apu.SetFrameCounterOptions(value);
                break;
            // as of now no way of getting P2 input
            default:
                throw new NotImplementedException(position.ToString());
        }
    }
}