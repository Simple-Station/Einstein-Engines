namespace Content.Server.WhiteDream.BloodCult.Empower;

[RegisterComponent]
public sealed partial class BloodCultEmpoweredComponent : Component
{
    [DataField]
    public float DamageMultiplier = 0.5f;

    [DataField]
    public float TimeMultiplier = 0.5f;

    [DataField]
    public TimeSpan TimeRemaining = TimeSpan.Zero;

    [DataField]
    public float NearbyCultTileRadius = 1f;

    [DataField]
    public string CultTile = "CultFloor";

    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan DefaultTime = TimeSpan.FromSeconds(20);
}
