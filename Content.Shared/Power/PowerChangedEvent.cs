namespace Content.Shared.Power;

/// <summary>
/// Raised whenever an ApcPowerReceiver becomes powered / unpowered.
/// Does nothing on the client.
/// </summary>
[ByRefEvent]
public readonly record struct PowerChangedEvent(bool Powered, float ReceivingPower);


/// <summary>
/// Raised whenever power consumed as a side load changes.
/// Does nothing on the client.
/// </summary>
[ByRefEvent]
public readonly record struct SidePowerChangedEvent(float SideLoadFraction);
