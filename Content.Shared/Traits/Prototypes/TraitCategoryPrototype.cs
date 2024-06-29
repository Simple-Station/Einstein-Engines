using Robust.Shared.Prototypes;

namespace Content.Shared.Traits;


/// <summary>
///     A prototype defining a valid category for <see cref="TraitPrototype"/>s to go into.
/// </summary>
[Prototype("traitCategory")]
public sealed partial class TraitCategoryPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; } = default!;
}
