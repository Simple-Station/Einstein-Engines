using Content.Server.Actions;
using Content.Shared._EE.Shadowling;
using Content.Shared.Humanoid;


namespace Content.Server._EE.Shadowling;


/// <summary>
/// This handles Hypnosis.
/// Instant Thrall from afar!
/// </summary>
public sealed class ShadowlingHypnosisSystem : EntitySystem
{
    [Dependency] private readonly ActionsSystem _actions = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingHypnosisComponent, HypnosisEvent>(OnHypnosis);
    }

    private void OnHypnosis(EntityUid uid, ShadowlingHypnosisComponent component, HypnosisEvent args)
    {
        var target = args.Target;
        if (HasComp<ThrallComponent>(target))
            return;

        if (!HasComp<HumanoidAppearanceComponent>(target))
            return;

        EnsureComp<ThrallComponent>(target);
        // we dont need to raise any events here, the shadowling has already ascended so there's nothing to do
        _actions.StartUseDelay(args.Action);
    }
}
