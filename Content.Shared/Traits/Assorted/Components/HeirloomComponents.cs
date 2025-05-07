using Content.Shared.Mood;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Traits.Assorted.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class HeirloomHaverComponent : Component
{
    [AutoNetworkedField, ViewVariables(VVAccess.ReadOnly)]
    public EntityUid Heirloom;

    [AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public ProtoId<MoodEffectPrototype> Moodlet = "HeirloomSecure";
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class HeirloomComponent : Component
{
    [AutoNetworkedField, ViewVariables(VVAccess.ReadOnly)]
    public EntityUid HOwner;
}
