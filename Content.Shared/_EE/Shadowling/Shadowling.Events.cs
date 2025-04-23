using Robust.Shared.Serialization;


namespace Content.Shared._EE.Shadowling;


/// <summary>
/// This handles when a Thrall gets added to the Shadowling.
/// </summary>
[Serializable, NetSerializable]
public sealed class ThrallAddedEvent : EntityEventArgs
{
}

/// <summary>
/// This handles when a Thrall gets removed to the Shadowling.
/// </summary>
[Serializable, NetSerializable]
public sealed class ThrallRemovedEvent : EntityEventArgs
{
}

/// <summary>
/// This handles the event which the phase of the Shadowling has changed.
/// </summary>
[Serializable, NetSerializable]
public sealed class PhaseChangedEvent : EntityEventArgs
{
    public ShadowlingPhases Phase;

    public PhaseChangedEvent(ShadowlingPhases phase)
    {
        Phase = phase;
    }
}
