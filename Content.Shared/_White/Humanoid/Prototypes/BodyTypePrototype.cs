using Content.Shared.Humanoid;
using Robust.Shared.Prototypes;

namespace Content.Shared._White.Humanoid.Prototypes;

[Prototype("bodyType")]
public sealed class BodyTypePrototype : IPrototype
{
    /// <summary>
    ///     Prototype ID of the body type.
    /// </summary>
    [IdDataField]
    public string ID { get; } = default!;

    /// <summary>
    ///     User visible name of the body type.
    /// </summary>
    [DataField(required: true)]
    public string Name { get; } = default!;

    /// <summary>
    ///     Sprites that this species will use on the given humanoid
    ///     visual layer. If a key entry is empty, it is assumed that the
    ///     visual layer will not be in use on this species, and will
    ///     be ignored.
    /// </summary>
    [DataField(required: true)]
    public Dictionary<HumanoidVisualLayers, string> Sprites = new();

    /// <summary>
    ///     Which sex can't use this body type.
    /// </summary>
    [DataField]
    public List<string> SexRestrictions = new();
}
