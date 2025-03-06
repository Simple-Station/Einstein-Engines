using Content.Server.Abilities.Oni;
using Content.Shared.Ghost;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;


namespace Content.Shared._EE.Item;

public sealed class OniOnlySystem : EntitySystem
{
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<OniOnlyComponent, AttemptMeleeEvent>(OnMeleeAttempt);
    }

    private void OnMeleeAttempt(EntityUid uid, OniOnlyComponent component, ref AttemptMeleeEvent args)
    {
        bool CanUse(EntityUid? uid) => HasComp<OniComponent>(uid) || HasComp<GhostComponent>(uid);

        // Allow the melee attempt if the user is either an Oni or a Ghost.
        if (CanUse(args.PlayerUid))
            return;

        // Get the text
        args.Message = Loc.GetString("wieldable-component-requires-fumble", ("item", uid));

        // Check if the user isn't already knocked down before playing the sound.
        var playSound = !_statusEffects.HasStatusEffect(args.PlayerUid, "KnockedDown");

        // Apply knockdown using the specified duration and force-drop any held item.
        _stun.TryKnockdown(args.PlayerUid, component.KnockdownDuration, true);
        _hands.TryDrop(args.PlayerUid);

        if (playSound)
            _audioSystem.PlayPredicted(new SoundPathSpecifier("/Audio/Effects/slip.ogg"), args.PlayerUid, args.PlayerUid);

        // Display the message to the player and cancel the melee attempt.
        _popupSystem.PopupClient(args.Message, uid, args.PlayerUid);
        args.Cancelled = true;
    }
}
