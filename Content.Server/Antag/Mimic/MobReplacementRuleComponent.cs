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

    [DataField("numberToReplace")]
    public int NumberToReplace { get; set; }

    [DataField("announcement")]
    public string Announcement = "station-event-rampant-intelligence-announcement";

    /// <summary>
    /// Chance per-entity.
    /// </summary>
    [DataField]
    public float Chance = 0.001f;

    [DataField("doAnnouncement")]
    public bool DoAnnouncement = true;

    [DataField("mimicMeleeDamage")]
    public float MimicMeleeDamage = 20f;

    [DataField("mimicMoveSpeed")]
    public float MimicMoveSpeed = 1f;

    [DataField("mimicAIType")]
    public string MimicAIType = "SimpleHostileCompound";

    [DataField("mimicSmashGlass")]
    public bool MimicSmashGlass = true;

    [DataField("vendorModify")]
    public bool VendorModify = true;
}
