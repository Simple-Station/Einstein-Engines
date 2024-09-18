using Content.Shared.Damage;
using Content.Shared.FixedPoint;

namespace Content.Server.WhiteDream.BloodCult.Runes.Offering;

[RegisterComponent]
public sealed partial class CultRuneOfferingComponent : Component
{
    [DataField]
    public float OfferingRange = 0.5f; // Half a tile

    /// <summary>
    ///     The amount of charges revive rune system should recieve on sacrifice/convert.
    /// </summary>
    [DataField]
    public int ReviveChargesPerSacrifice = 1;

    [DataField]
    public DamageSpecifier ConvertHealing = new()
    {
        DamageDict = new Dictionary<string, FixedPoint2>
        {
            ["Brute"] = -40,
            ["Burn"] = -40
        }
    };
}
