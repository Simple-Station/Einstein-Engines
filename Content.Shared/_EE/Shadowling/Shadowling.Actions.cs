using Content.Shared.Actions;
using Robust.Shared.GameStates;


namespace Content.Shared._EE.Shadowling;


/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingActionComponent : Component
{
}

#region Events - First Phase
public sealed partial class HatchEvent : InstantActionEvent { }

#endregion

#region Events - Second Phase

public sealed partial class EnthrallEvent : EntityTargetActionEvent { }

#endregion
