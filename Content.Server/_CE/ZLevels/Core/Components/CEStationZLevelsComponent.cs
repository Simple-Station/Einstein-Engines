/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server._CE.ZLevels.Core.Components;

/// <summary>
/// Component that stores Z-level network configuration for a station, including maps to load above and below
/// the main station level and shared component overrides applied across all Z-levels.
/// </summary>
[RegisterComponent]
public sealed partial class CEStationZLevelsComponent : Component
{
    [DataField]
    public EntityUid? ZNetworkEntity;

    /// <summary>
    /// CrystallEdge: Additional maps loaded below the main map (at negative depth levels).
    /// Each map in the list is loaded at depth -N, -N+1, ..., -1, with <see cref="MapPath"/> at depth 0.
    /// </summary>
    [DataField]
    public List<ResPath> MapsBelow = new();

    /// <summary>
    /// CrystallEdge: additional maps loaded above the main map (at positive depth levels).
    /// Each map in the list is loaded at depth 1, 2, ..., N. <see cref="MapPath"/> works as depth 0.
    /// </summary>
    [DataField]
    public List<ResPath> MapsAbove = new();

    /// <summary>
    /// CrystallEdge: ability to setup shared components for all zLevels
    /// </summary>
    [DataField]
    public ComponentRegistry ZLevelsComponentOverrides = new();
}
