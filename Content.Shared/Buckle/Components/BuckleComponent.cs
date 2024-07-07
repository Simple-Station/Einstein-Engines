using Content.Shared.Interaction;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Content.Shared.Whitelist;
namespace Content.Shared.Buckle.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]

[Access(typeof(SharedBuckleSystem))]

public sealed partial class BuckleComponent : Component

{

    [DataField, ViewVariables(VVAccess.ReadWrite)]

    public float Range = SharedInteractionSystem.InteractionRange / 1.4f;


    [ViewVariables(VVAccess.ReadWrite)]

    [AutoNetworkedField]

    public bool Buckled;


    [ViewVariables]

    [AutoNetworkedField]

    public EntityUid? LastEntityBuckledTo;


    [ViewVariables(VVAccess.ReadWrite)]

    [DataField, AutoNetworkedField]

    public bool DontCollide;


    [ViewVariables(VVAccess.ReadWrite)]

    [DataField]

    public bool PullStrap;


    [DataField, ViewVariables(VVAccess.ReadWrite)]

    public TimeSpan Delay = TimeSpan.FromSeconds(0.25f);


    [ViewVariables]

    public TimeSpan BuckleTime;


    [ViewVariables]

    [AutoNetworkedField]

    public EntityUid? BuckledTo;


    [DataField, ViewVariables(VVAccess.ReadWrite)]

    public int Size = 100;


    [ViewVariables]

    public int? OriginalDrawDepth;


    [DataField, ViewVariables(VVAccess.ReadWrite)]

    public EntityWhitelist? AllowedBuckleTypes;

}

[ByRefEvent]
public record struct BuckleAttemptEvent(EntityUid StrapEntity, EntityUid BuckledEntity, EntityUid UserEntity, bool Buckling, bool Cancelled = false);

[ByRefEvent]
public readonly record struct BuckleChangeEvent(EntityUid StrapEntity, EntityUid BuckledEntity, bool Buckling);

[Serializable, NetSerializable]
public enum BuckleVisuals
{
    Buckled
}

