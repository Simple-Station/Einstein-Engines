using Content.Shared.NPC.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;


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

    /// Can this be stunned by ethereal stun objects?
    [DataField]
    public bool CanBeStunned = true;

    /// Drain Mana if this entity is psionic?
    [DataField]
    public bool DrainMana = true;

    public List<EntityUid> DarkenedLights = new();

    public float OldManaGain;

    public float DarkenAccumulator;

    public int OldMobMask;

    public int OldMobLayer;

    public List<ProtoId<NpcFactionPrototype>> SuppressedFactions = new();
    public bool HasDoorBumpTag;
}
