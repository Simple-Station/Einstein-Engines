using Robust.Shared.GameStates;

namespace Content.Server.Silicons.Borgs.Components;

/// <summary>
/// Server side indicator for a jetpack module. Used as conditional for inserting in canisters.
/// </summary>
[RegisterComponent]
public sealed partial class BorgJetpackComponent : Component
{
    public EntityUid? JetpackUid = null;
}