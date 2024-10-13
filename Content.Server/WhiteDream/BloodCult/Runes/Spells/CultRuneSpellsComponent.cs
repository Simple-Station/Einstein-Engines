using Robust.Shared.Prototypes;

namespace Content.Server.WhiteDream.BloodCult.Runes.Spells;

[RegisterComponent]
public sealed partial class CultRuneSpellsComponent : Component
{
    [DataField(required: true)]
    public List<EntProtoId> Prototypes = new();
}
