using Content.Shared.Actions;
using Robust.Shared.GameStates;


namespace Content.Shared._EE.Shadowling;


/// <summary>
/// This is used for Action Events for the Sling
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
public sealed partial class GlareEvent : EntityTargetActionEvent { }
public sealed partial class VeilEvent : InstantActionEvent { }
public sealed partial class RapidRehatchEvent : InstantActionEvent { }
public sealed partial class ShadowWalkEvent : InstantActionEvent { }
public sealed partial class IcyVeinsEvent : InstantActionEvent { }

public sealed partial class DestroyEnginesEvent : InstantActionEvent { }
public sealed partial class CollectiveMindEvent : InstantActionEvent { }

#endregion

#region Events - Thrall Required

public sealed partial class BlindnessSmokeEvent : InstantActionEvent { }
public sealed partial class NullChargeEvent : InstantActionEvent { }
public sealed partial class SonicScreechEvent : InstantActionEvent { }
#endregion
