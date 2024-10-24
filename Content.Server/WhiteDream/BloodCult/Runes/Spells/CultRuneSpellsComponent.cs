using Content.Shared.RadialSelector;

namespace Content.Server.WhiteDream.BloodCult.Runes.Spells;

[RegisterComponent]
public sealed partial class CultRuneSpellsComponent : Component
{
    [DataField(required: true)]
    public List<RadialSelectorEntry> Prototypes = new();
}
