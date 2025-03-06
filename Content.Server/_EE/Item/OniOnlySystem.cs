using Content.Server.Abilities.Oni;
using Content.Shared.Damage;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory.VirtualItem;
using Content.Shared.Item;
using Content.Shared.Popups;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Content.Shared.Timing;
using Content.Shared.Weapons.Melee.Components;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared.Wieldable.Components;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Timing;


namespace Content.Server._EE.Item;

public sealed class OniOnlySystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<OniOnlyComponent, AttemptMeleeEvent>(OnMeleeAttempt);
    }

    private void OnMeleeAttempt(EntityUid uid, OniOnlyComponent component, ref AttemptMeleeEvent args)
    {
        if (TryComp<WieldableComponent>(uid, out var wieldable) &&
            !wieldable.Wielded && !HasComp<OniComponent>(args.PlayerUid))
        {
            {
                args.Message = Loc.GetString("wieldable-component-requires-fumble", ("item", uid));
                var playSound = !_statusEffects.HasStatusEffect(args.PlayerUid, "KnockedDown");
                _stun.TryKnockdown(args.PlayerUid, TimeSpan.FromSeconds(1.5f), true);
                if (playSound)
                    _audioSystem.PlayPredicted(new SoundPathSpecifier("/Audio/Effects/slip.ogg"), args.PlayerUid, args.PlayerUid);
            }

            args.Cancelled = true;
        }
    }
}
