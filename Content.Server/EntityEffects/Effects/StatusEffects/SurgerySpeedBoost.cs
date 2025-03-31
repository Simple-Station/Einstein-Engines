using Content.Shared.Chemistry.Reagent;
using Content.Shared.EntityEffects;
using Content.Shared.StatusEffect;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Server.EntityEffects.Effects.StatusEffects;

/// <summary>
///     this should let you temporarily add a component with one non-default arg
///     hopefully it works :]
///     it'll work the same as generic effects otherwise
/// </summary>
/// <remarks>
///     this is a SUPER hacky way of doing it but i'm mostly flying blind here so oh well
/// </remarks>
[UsedImplicitly]
public sealed partial class ArgStatusEffect : EntityEffect
{
    [DataField(required: true)]
    public string Key = default!;

    private string Component = String.SurgerySpeedModifierComponent ;

    [DataField]
    public string SpeedMod = 1.5f;

    [DataField]
    public float Time = 2.0f;

    /// <remarks>
    ///     true - refresh status effect time,  false - accumulate status effect time
    /// </remarks>
    [DataField]
    public bool Refresh = true;

    /// <summary>
    ///     Should this effect add the status effect, remove time from it, or set its cooldown?
    /// </summary>
    [DataField]
    public StatusEffectMetabolismType Type = StatusEffectMetabolismType.Add;

    public override void Effect(EntityEffectBaseArgs args)
    {
        var statusSys = args.EntityManager.EntitySysManager.GetEntitySystem<StatusEffectsSystem>();

        var time = Time;
        if (args is EntityEffectReagentArgs reagentArgs)
            time *= reagentArgs.Scale.Float();

        if (Type == StatusEffectMetabolismType.Add && Component != String.Empty)
        {
            statusSys.TryAddStatusEffect(args.TargetEntity, Key, TimeSpan.FromSeconds(time), Refresh, Component);
        }
        else if (Type == StatusEffectMetabolismType.Remove)
        {
            statusSys.TryRemoveTime(args.TargetEntity, Key, TimeSpan.FromSeconds(time));
        }
        else if (Type == StatusEffectMetabolismType.Set)
        {
            statusSys.TrySetTime(args.TargetEntity, Key, TimeSpan.FromSeconds(time));
        }
    }

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys) => Loc.GetString(
        "reagent-effect-guidebook-status-effect",
        ("chance", Probability),
        ("type", Type),
        ("time", Time),
        ("key", $"reagent-effect-status-effect-{Key}"));
}

public enum StatusEffectMetabolismType
{
    Add,
    Remove,
    Set
}
