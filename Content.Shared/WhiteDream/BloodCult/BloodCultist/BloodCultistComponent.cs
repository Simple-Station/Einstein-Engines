using System.Threading;
using Content.Shared.Antag;
using Content.Shared.FixedPoint;
using Content.Shared.Mind;
using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.WhiteDream.BloodCult.BloodCultist;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BloodCultistComponent : Component, IAntagStatusIconComponent
{
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public float HolyConvertTime = 15f;

    [AutoNetworkedField]
    public List<NetEntity?> SelectedEmpowers = new();

    [DataField]
    public int MaximumAllowedEmpowers = 4;

    [ViewVariables(VVAccess.ReadWrite)]
    public FixedPoint2 RitesBloodAmount = FixedPoint2.Zero;

    public Color OriginalEyeColor = Color.White;

    // [ViewVariables, NonSerialized]
    // public Entity<BloodSpearComponent>? BloodSpear;

    [ViewVariables, NonSerialized]
    public EntityUid? BloodSpearActionEntity;

    [ViewVariables, NonSerialized]
    public Entity<MindComponent>? OriginalMind;

    [DataField]
    public ProtoId<StatusIconPrototype> StatusIcon { get; set; } = "BloodCultMember";

    [DataField]
    public bool IconVisibleToGhost { get; set; } = true;

    public CancellationTokenSource? DeconvertToken { get; set; }
}
