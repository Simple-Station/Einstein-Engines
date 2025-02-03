using Content.Shared.WhiteDream.BloodCult.Items.BaseAura;

namespace Content.Server.WhiteDream.BloodCult.Items.StunAura;

[RegisterComponent]
public sealed partial class StunAuraComponent : BaseAuraComponent
{
    [DataField]
    public TimeSpan ParalyzeDuration = TimeSpan.FromSeconds(16);

    [DataField]
    public TimeSpan MuteDuration = TimeSpan.FromSeconds(12);
}
