using Content.Server.NPC.Components;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Roles;
using Content.Shared.WhiteDream.BloodCult.Components;
using Robust.Shared.Prototypes;

namespace Content.Server.WhiteDream.BloodCult.Gamerule;

[RegisterComponent, Access(typeof(BloodCultRuleSystem))]
public sealed partial class BloodCultRuleComponent : Component
{
    public List<Entity<BloodCultistComponent>> CultistMinds = new();

    [DataField]
    public ProtoId<AntagPrototype> CultistRolePrototype = "Cultist";

    // TODO: Do we need it?
    // [DataField]
    // public ProtoId<GamePresetPrototype> CultGamePresetPrototype = "Cult";

    [DataField]
    public ProtoId<NpcFactionPrototype> NanoTrasenFaction = "NanoTrasen";

    [DataField]
    public ProtoId<NpcFactionPrototype> BloodCultFaction = "GeometerOfBlood";

    [DataField]
    public ProtoId<EntityPrototype> ReaperPrototype = "ReaperConstruct";

    [ViewVariables(VVAccess.ReadOnly), DataField]
    public string CultFloor = "CultFloor";

    [DataField]
    public Color EyeColor = Color.FromHex("#f80000");

    // TODO: DO we need it here?
    public ProtoId<ReagentPrototype> HolyWaterReagent = "Holywater";

    // TODO: This two should probably be percent of server population
    [DataField]
    public int ReadEyeThreshold = 5;

    [DataField]
    public int PentagramThreshold = 8;

    public EntityUid? OfferingTarget;

    // public List<ConstructComponent> Constructs = new();

    public CultWinCondition WinCondition = CultWinCondition.Draw;

    public CultStage Stage = CultStage.Start;
}

public enum CultWinCondition : byte
{
    Draw,
    Win,
    Failure,
}

public enum CultStage : byte
{
    Start,
    RedEyes,
    Pentagram,
}

public sealed class CultNarsieSummoned : EntityEventArgs;
