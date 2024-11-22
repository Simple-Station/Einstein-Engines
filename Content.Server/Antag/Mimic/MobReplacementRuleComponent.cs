using Robust.Shared.Prototypes;

namespace Content.Server.Antag.Mimic;

/// <summary>
/// Replaces the relevant entities with mobs when the game rule is started.
/// </summary>
[RegisterComponent]
public sealed partial class MobReplacementRuleComponent : Component
{
    // If you want more components use generics, using a whitelist would probably kill the server iterating every single entity.

    [DataField]
    public EntProtoId Proto = "MobMimic";

    [DataField]
    public int NumberToReplace { get; set; }

    [DataField]
    public string Announcement = "station-event-rampant-intelligence-announcement";

    /// <summary>
    /// Chance per-entity.
    /// </summary>
    [DataField]
    public float Chance = 0.004f;

    [DataField]
    public bool DoAnnouncement = true;

    [DataField]
    public float MimicMeleeDamage = 20f;

    [DataField]
    public float MimicMoveSpeed = 1f;

    [DataField]
    public string MimicAIType = "SimpleHostileCompound";

    [DataField]
    public bool MimicSmashGlass = true;

    [DataField]
    public bool VendorModify = true;
}
