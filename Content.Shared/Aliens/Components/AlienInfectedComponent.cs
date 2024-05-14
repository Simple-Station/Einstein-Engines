using System.Threading;
using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Aliens.Components;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class AlienInfectedComponent : Component
{

    [DataField]
    public float GrowTime = 2f;

    [DataField]
    public EntProtoId Prototype = "MobAlienLarva";

    [DataField]
    public EntProtoId OrganProtoId = "AlienLarvaOrgan";

    [DataField]
    public EntProtoId PartProtoId = "AlienLarvaPart";

    public readonly HashSet<ProtoId<StatusIconPrototype>> InfectedIcons =
    [
        "AlienInfectedIconStageZero",
        "AlienInfectedIconStageOne",
        "AlienInfectedIconStageTwo",
        "AlienInfectedIconStageThree",
        "AlienInfectedIconStageFour",
        "AlienInfectedIconStageFive"
    ];

    [ViewVariables]
    public int GrowthStage = 0;

    [DataField]
    public float GrowProb = 0.03f;

    [DataField]
    public TimeSpan NextGrowRoll = TimeSpan.Zero;
}
