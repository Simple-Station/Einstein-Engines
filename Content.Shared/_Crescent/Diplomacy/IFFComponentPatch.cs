using Content.Shared._Crescent.Diplomacy;
using Color = Robust.Shared.Maths.Color;
using Component = Robust.Shared.GameObjects.Component;
using VVAccess = Robust.Shared.ViewVariables.VVAccess;
using Content.Shared.Shuttles.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared.Shuttles.Components;

/*
public sealed partial class IFFComponent : Component
{
    /// <summary>
    /// Which faction this ship is advertising as.
    /// Use the IDs of Diplomacy prototypes to have it work properly, otherwise it'll show up as neutral.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly), DataField, AutoNetworkedField]
    public string Faction = "Neutral";

    /// <summary>
    /// Cache faction relations.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly), DataField, AutoNetworkedField]
    public Dictionary<string, Relations> Relations = new();
}
*/
