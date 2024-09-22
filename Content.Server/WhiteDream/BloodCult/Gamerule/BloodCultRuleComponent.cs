using Content.Server.NPC.Components;
using Content.Shared.Roles;
using Content.Shared.WhiteDream.BloodCult.BloodCultist;
using Robust.Shared.Prototypes;

namespace Content.Server.WhiteDream.BloodCult.Gamerule;

[RegisterComponent]
public sealed partial class BloodCultRuleComponent : Component
{
    public List<Entity<BloodCultistComponent>> CultistMinds = new();

    [DataField]
    public ProtoId<AntagPrototype> CultistRolePrototype = "Cultist";

    [DataField]
    public ProtoId<NpcFactionPrototype> NanoTrasenFaction = "NanoTrasen";

    [DataField]
    public ProtoId<NpcFactionPrototype> BloodCultFaction = "GeometerOfBlood";

    [DataField]
    public ProtoId<EntityPrototype> ReaperPrototype = "ReaperConstruct";

    [DataField]
    public Color EyeColor = Color.FromHex("#f80000");

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
