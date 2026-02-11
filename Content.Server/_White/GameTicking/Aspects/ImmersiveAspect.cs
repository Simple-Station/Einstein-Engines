using System.Numerics;
using Content.Server._White.GameTicking.Aspects.Components;
using Content.Server.Movement.Components;
using Content.Shared.GameTicking;
using Content.Shared.GameTicking.Components;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;

namespace Content.Server._White.GameTicking.Aspects;

public sealed class ImmersiveAspect : AspectSystem<ImmersiveAspectComponent>
{
    [Dependency] private readonly SharedContentEyeSystem _eye = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawn);
    }

    protected override void Started(
        EntityUid uid,
        ImmersiveAspectComponent component,
        GameRuleComponent gameRule,
        GameRuleStartedEvent args
        )
    {
        base.Started(uid, component, gameRule, args);

        OnStarted(component);
    }

    private void OnStarted(ImmersiveAspectComponent component)
    {
        var query = EntityQueryEnumerator<ContentEyeComponent>();

        while (query.MoveNext(out var entity, out _))
        {
            SetEyeZoom(entity, component.EyeModifier);
            EnsureComp<EyeCursorOffsetComponent>(entity);
        }
    }

    private void SetEyeZoom(EntityUid human, float modifier)
    {
        _eye.SetMaxZoom(human, new Vector2(modifier));
        _eye.SetZoom(human, new Vector2(modifier));
    }

    private void OnPlayerSpawn(PlayerSpawnCompleteEvent ev)
    {
        if (!HasComp<ContentEyeComponent>(ev.Mob))
            return;

        var query = EntityQueryEnumerator<ImmersiveAspectComponent, GameRuleComponent>();
        while (query.MoveNext(out var ruleEntity, out var immersiveAspect, out var gameRule))
        {
            if (!GameTicker.IsGameRuleAdded(ruleEntity, gameRule))
                continue;

            SetEyeZoom(ev.Mob, immersiveAspect.EyeModifier);
            EnsureComp<EyeCursorOffsetComponent>(ev.Mob);
        }
    }

    protected override void Ended(
        EntityUid uid,
        ImmersiveAspectComponent component,
        GameRuleComponent gameRule,
        GameRuleEndedEvent args
        )
    {
        base.Ended(uid, component, gameRule, args);

        var query = EntityQueryEnumerator<ContentEyeComponent>();

        while (query.MoveNext(out var entity, out _))
        {
            SetEyeZoom(entity, 1f);

            RemComp<EyeCursorOffsetComponent>(entity);
        }
    }
}
