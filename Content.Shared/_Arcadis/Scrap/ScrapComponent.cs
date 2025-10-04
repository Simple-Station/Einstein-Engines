using Content.Shared.Random;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Arcadis.Scrap;

/// <summary>
/// Real salvagers only use REAL scrap!
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ScrapComponent : Component
{

    // Output item prototypes, such as rare parts or tech boards
    [DataField]
    public ProtoId<WeightedRandomEntityPrototype>? OutputItems;

    // Output materials, like steel, glass, plastic, and HOLMIUM (when I port it)
    [DataField]
    public ProtoId<WeightedRandomPrototype> OutputMaterials;

    // Chance to spit out an item
    [DataField]
    public float? ItemChance = 0.005f;
}
