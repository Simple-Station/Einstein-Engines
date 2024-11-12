using Robust.Shared.GameStates;

namespace Content.Shared.Shadowkin;

[RegisterComponent, NetworkedComponent]
public sealed partial class EtherealComponent : Component
{
    /// <summary>
    ///     Does the Ent, Dark lights around it?
    /// </summary>
    [DataField]
    public bool Darken = false;

    /// <summary>
    ///     Range of the Darken Effect.
    /// </summary>
    [DataField]
    public float DarkenRange = 5;

    /// <summary>
    ///     Darken Effect Rate.
    /// </summary>
    [DataField]
    public float DarkenRate = 0.084f;

    /// <summary>
    ///     Can be stunned by ethereal stun objects?
    /// </summary>
    [DataField]
    public bool CanBeStunned = true;

    /// <summary>
    ///     Drain Mana is ent is psionic?
    /// </summary>
    [DataField]
    public bool DrainMana = true;

    public List<EntityUid> DarkenedLights = new();

    public float OldManaGain;

    public float DarkenAccumulator;

    public int OldMobMask;

    public int OldMobLayer;

    public List<string> SuppressedFactions = new();
    public bool HasDoorBumpTag;
}
