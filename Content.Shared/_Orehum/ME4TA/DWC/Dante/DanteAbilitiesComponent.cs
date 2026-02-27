using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._Orehum.ME4TA.Dante;

[RegisterComponent, NetworkedComponent]
public sealed partial class DanteAbilitiesComponent : Component
{
    [DataField] public EntityUid? RainStormActionEntity;
    [DataField] public EntityUid? StingerActionEntity;
}

public sealed partial class DanteRainStormEvent : InstantActionEvent { }

public sealed partial class DanteStingerEvent : WorldTargetActionEvent { }
