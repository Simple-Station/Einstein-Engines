/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._CE.ZLevels.Mapping.Prototypes;

[Prototype("zMap")]
public sealed partial class CEZLevelMapPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public List<ResPath> Maps = new();

    /// <summary>
    /// Shared components for all zLevels maps
    /// </summary>
    [DataField]
    public ComponentRegistry Components = new();
}
