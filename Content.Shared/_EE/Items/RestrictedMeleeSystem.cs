using Content.Server.Abilities.Oni;
using Content.Shared.Ghost;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using System.Collections.Generic;

namespace Content.Shared._EE.Item;

public sealed class RestrictedMeleeSystem : EntitySystem
{
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RestrictedMeleeComponent, AttemptMeleeEvent>(OnMeleeAttempt);
    }

    private bool CanUse(EntityUid uid, RestrictedMeleeComponent comp)
    {
        foreach (var (_, data) in comp.AllowedComponents)
        {
            if (EntityManager.HasComponent(uid, data.GetType()))
                return true;
        }
        return false;
    }

    private void OnMeleeAttempt(EntityUid uid, RestrictedMeleeComponent comp, ref AttemptMeleeEvent args)
    {
        if (CanUse(args.PlayerUid, comp))
            return;

        args.Message = Loc.GetString(comp.FailText, ("item", uid));

        var playSound = !_statusEffects.HasStatusEffect(args.PlayerUid, "KnockedDown");

        if (comp.DoKnockdown)
            _stun.TryKnockdown(args.PlayerUid, comp.KnockdownDuration, true);

        if (comp.ForceDrop)
            _hands.TryDrop(args.PlayerUid);

        if (playSound)
            _audioSystem.PlayPredicted(comp.FallSound, args.PlayerUid, args.PlayerUid);

        _popupSystem.PopupClient(args.Message, uid, PopupType.Large);
        args.Cancelled = true;
    }
}
