using Content.Shared.Actions;
using Content.Shared.Aliens.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Stunnable;
using Robust.Shared.Audio.Systems;

namespace Content.Shared.Aliens.Systems;

/// <summary>
/// This handles...
/// </summary>
public sealed class TailLashSystem : EntitySystem
{
    /// <inheritdoc/>
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TailLashComponent, ComponentInit>(OnComponentInit);

        SubscribeLocalEvent<TailLashComponent, TailLashActionEvent>(OnLash);
    }

    private void OnComponentInit(EntityUid uid, TailLashComponent component, ComponentInit args)
    {
        _actions.AddAction(uid, ref component.TailLashActionEntity, component.TailLashAction, uid);
    }

    private void OnLash(EntityUid uid, TailLashComponent component, TailLashActionEvent args)
    {
        _audio.PlayPredicted(component.LashSound, uid, uid);
        foreach (var entity in _lookup.GetEntitiesInRange(uid, component.LashRange))
        {
            if (HasComp<MobStateComponent>(entity))
            {
                _stun.TryParalyze(entity, TimeSpan.FromSeconds(component.StunTime), true);
            }
        }
        _actions.SetCooldown(component.TailLashActionEntity, TimeSpan.FromSeconds(component.Cooldown));
    }
}
