namespace Nescli;

/// <summary>
/// Represents the Audio Processing Unit inside an NES. Does not have its own
/// memory controller and is purely controlled by the CPU over the bus adapter.
/// </summary>
public class Apu
{
    private bool _enablePulseChannel1;
    private bool _enablePulseChannel2;
    private bool _enableTriangle;
    private bool _enableNoise;
    private bool _enableDeltaModulationChannel;

    private byte _dmc;

    /// <summary>
    /// Used to enable or disable individual channels
    /// </summary>
    /// <param name="value">Value written to memory interface</param>
    public void SetStatus(byte value)
    {
        _enablePulseChannel1 = (value & 0b1) != 0;
        _enablePulseChannel2 = (value & 0b10) != 0;
        _enableTriangle = (value & 0b100) != 0;
        _enableNoise = (value & 0b1000) != 0;
        _enableDeltaModulationChannel = (value & 0b10000) != 0;
    }

    /// <summary>
    /// Sets the Delta Modulation Channel to a specific value
    /// </summary>
    /// <param name="value">>The value to set it to</param>
    public void SetDmcValue(byte value)
    {
        _dmc = (byte)(value & 0b1111111);
    }

    public void SetFrameCounterOptions(byte value)
    {

    }
}