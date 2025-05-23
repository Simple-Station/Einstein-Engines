namespace Content.Shared.Contests;

[ImplicitDataDefinitionForInheritors]
public abstract partial class ContestFunction
{
    [DataField]
    public bool BypassClamp;

    [DataField]
    public float RangeFactor = 1f;

    public virtual float ComparatorFunction(
        EntityUid target,
        IEntityManager entityManager) => 1f;

    public virtual float ComparatorFunction(
        EntityUid performer,
        EntityUid target,
        IEntityManager entityManager) => 1f;
}

[ImplicitDataDefinitionForInheritors]
public abstract partial class MassContestSingle : ContestFunction
{
    [DataField]
    public float AverageMass = 71f;

    public override float ComparatorFunction(
        EntityUid target,
        IEntityManager entityManager)
    {
        var contestsSystem = entityManager.System<ContestsSystem>();
        return contestsSystem.MassContest(target, BypassClamp, RangeFactor, AverageMass);
    }
}

[ImplicitDataDefinitionForInheritors]
public abstract partial class MassContestDouble : ContestFunction
{
    public override float ComparatorFunction(
        EntityUid performer,
        EntityUid target,
        IEntityManager entityManager)
    {
        var contestsSystem = entityManager.System<ContestsSystem>();
        return contestsSystem.MassContest(performer, target, BypassClamp, RangeFactor);
    }
}

[ImplicitDataDefinitionForInheritors]
public abstract partial class StaminaContestSingle : ContestFunction
{
    public override float ComparatorFunction(
        EntityUid target,
        IEntityManager entityManager)
    {
        var contestsSystem = entityManager.System<ContestsSystem>();
        return contestsSystem.StaminaContest(target, BypassClamp, RangeFactor);
    }
}

[ImplicitDataDefinitionForInheritors]
public abstract partial class StaminaContestDouble : ContestFunction
{
    public override float ComparatorFunction(
        EntityUid performer,
        EntityUid target,
        IEntityManager entityManager)
    {
        var contestsSystem = entityManager.System<ContestsSystem>();
        return contestsSystem.StaminaContest(performer, target, BypassClamp, RangeFactor);
    }
}

[ImplicitDataDefinitionForInheritors]
public abstract partial class HealthContestSingle : ContestFunction
{
    public override float ComparatorFunction(
        EntityUid target,
        IEntityManager entityManager)
    {
        var contestsSystem = entityManager.System<ContestsSystem>();
        return contestsSystem.HealthContest(target, BypassClamp, RangeFactor);
    }
}

[ImplicitDataDefinitionForInheritors]
public abstract partial class HealthContestDouble : ContestFunction
{
    public override float ComparatorFunction(
        EntityUid performer,
        EntityUid target,
        IEntityManager entityManager)
    {
        var contestsSystem = entityManager.System<ContestsSystem>();
        return contestsSystem.HealthContest(performer, target, BypassClamp, RangeFactor);
    }
}

[ImplicitDataDefinitionForInheritors]
public abstract partial class MindContestSingle : ContestFunction
{
    [DataField]
    public float AveragePsionicPotential = 1.6f;

    public override float ComparatorFunction(
        EntityUid target,
        IEntityManager entityManager)
    {
        var contestsSystem = entityManager.System<ContestsSystem>();
        return contestsSystem.MindContest(target, BypassClamp, RangeFactor, AveragePsionicPotential);
    }
}

[ImplicitDataDefinitionForInheritors]
public abstract partial class MindContestDouble : ContestFunction
{
    public override float ComparatorFunction(
        EntityUid performer,
        EntityUid target,
        IEntityManager entityManager)
    {
        var contestsSystem = entityManager.System<ContestsSystem>();
        return contestsSystem.MindContest(performer, target, BypassClamp, RangeFactor);
    }
}

[ImplicitDataDefinitionForInheritors]
public abstract partial class MoodContestSingle : ContestFunction
{
    public override float ComparatorFunction(
        EntityUid target,
        IEntityManager entityManager)
    {
        var contestsSystem = entityManager.System<ContestsSystem>();
        return contestsSystem.MoodContest(target, BypassClamp, RangeFactor);
    }
}

[ImplicitDataDefinitionForInheritors]
public abstract partial class MoodContestDouble : ContestFunction
{
    public override float ComparatorFunction(
        EntityUid performer,
        EntityUid target,
        IEntityManager entityManager)
    {
        var contestsSystem = entityManager.System<ContestsSystem>();
        return contestsSystem.MoodContest(performer, target, BypassClamp, RangeFactor);
    }
}
