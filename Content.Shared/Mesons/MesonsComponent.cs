using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Mesons;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MesonsComponent : Component
{
    [DataField, AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public EntProtoId Action = "ActionToggleMesons";

    [DataField, AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public EntityUid? ActionEntity;

    [DataField, AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public bool Enabled;

    [DataField, AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public MesonsViewType MesonsType = MesonsViewType.Walls;

    [DataField, AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public SoundSpecifier EnableSound = new SoundPathSpecifier("/Audio/Items/Mesons/turn_on.ogg");
}

public enum MesonsViewType
{
    Walls,
    Radiation
}
