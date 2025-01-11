using Content.Server.Instruments;
using Content.Server.Speech.Components;
using Content.Server.UserInterface;
using Content.Shared.ActionBlocker;
using Content.Shared.Bed.Sleep;
using Content.Shared.Damage;
using Content.Shared.Damage.ForceSay;
using Content.Shared.FixedPoint;
using Content.Shared.Instruments;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Mobs;
using Content.Shared.Popups;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Content.Shared.Traits.Assorted.Components;
using Content.Shared.Traits.Assorted.Prototypes;
using Content.Shared.Traits.Assorted.Systems;
using Content.Shared.UserInterface;
using Content.Shared.Zombies;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server.Traits.Assorted;

public sealed class SingerSystem : SharedSingerSystem
{
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly InstrumentSystem _instrument = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GotEquippedEvent>(OnEquip);
        SubscribeLocalEvent<InstrumentComponent, MobStateChangedEvent>(OnMobStateChangedEvent);
        SubscribeLocalEvent<InstrumentComponent, KnockedDownEvent>(OnKnockedDown);
        SubscribeLocalEvent<InstrumentComponent, StunnedEvent>(OnStunned);
        SubscribeLocalEvent<InstrumentComponent, SleepStateChangedEvent>(OnSleep);
        SubscribeLocalEvent<InstrumentComponent, StatusEffectAddedEvent>(OnStatusEffect);
        SubscribeLocalEvent<InstrumentComponent, DamageChangedEvent>(OnDamageChanged);
        // This is intended to intercept and cancel the UI event before it reaches ActivatableUISystem.
        SubscribeLocalEvent<SingerComponent, OpenUiActionEvent>(OnInstrumentOpen, before: [typeof(ActivatableUISystem)]);
    }

    protected override SharedInstrumentComponent EnsureInstrumentComp(EntityUid uid)
    {
        return EnsureComp<InstrumentComponent>(uid);
    }

    protected override void SetUpSwappableInstrument(EntityUid uid, SingerInstrumentPrototype singer)
    {
        if (singer.InstrumentList.Count <= 1)
            return;

        var swappableComp = EnsureComp<SwappableInstrumentComponent>(uid);
        swappableComp.InstrumentList = singer.InstrumentList;
    }

    private void OnEquip(GotEquippedEvent args)
    {
        // Check if an item that makes the singer mumble is equipped to their face
        // (not their pockets!). As of writing, this should just be the muzzle.
        if (TryComp<AddAccentClothingComponent>(args.Equipment, out var accent) &&
            accent.ReplacementPrototype == "mumble" &&
            args.Slot == "mask")
            CloseMidiUi(args.Equipee);
    }

    private void OnMobStateChangedEvent(EntityUid uid, SharedInstrumentComponent component, MobStateChangedEvent args)
    {
        if (args.NewMobState is MobState.Critical or MobState.Dead)
            CloseMidiUi(args.Target);
    }

    private void OnKnockedDown(EntityUid uid, SharedInstrumentComponent component, ref KnockedDownEvent args)
    {
        CloseMidiUi(uid);
    }

    private void OnStunned(EntityUid uid, SharedInstrumentComponent component, ref StunnedEvent args)
    {
        CloseMidiUi(uid);
    }

    private void OnSleep(EntityUid uid, SharedInstrumentComponent component, ref SleepStateChangedEvent args)
    {
        if (args.FellAsleep)
            CloseMidiUi(uid);
    }

    private void OnStatusEffect(EntityUid uid, SharedInstrumentComponent component, StatusEffectAddedEvent args)
    {
        if (args.Key == "Muted")
            CloseMidiUi(uid);
    }

    /// <summary>
    /// Almost a copy of Content.Server.Damage.ForceSay.DamageForceSaySystem.OnDamageChanged.
    /// Done so because DamageForceSaySystem doesn't output an event, and my understanding is
    /// that we don't want to change upstream code more than necessary to avoid merge conflicts
    /// and maintenance overhead. It still reuses the values from DamageForceSayComponent, so
    /// any tweaks to that will keep ForceSay consistent with singing interruptions.
    /// </summary>
    private void OnDamageChanged(EntityUid uid, SharedInstrumentComponent instrumentComponent, DamageChangedEvent args)
    {
        if (!TryComp<DamageForceSayComponent>(uid, out var component) ||
            args.DamageDelta == null ||
            !args.DamageIncreased ||
            args.DamageDelta.GetTotal() < component.DamageThreshold ||
            component.ValidDamageGroups == null)
            return;

        var totalApplicableDamage = FixedPoint2.Zero;
        foreach (var (group, value) in args.DamageDelta.GetDamagePerGroup(ProtoMan))
        {
            if (!component.ValidDamageGroups.Contains(group))
                continue;

            totalApplicableDamage += value;
        }

        if (totalApplicableDamage >= component.DamageThreshold)
            CloseMidiUi(uid);
    }

    /// <summary>
    /// Prevent the player from opening the MIDI UI under some circumstances.
    /// </summary>
    private void OnInstrumentOpen(EntityUid uid, SingerComponent component, OpenUiActionEvent args)
    {
        // CanSpeak covers all reasons you can't talk, including being incapacitated
        // (crit/dead), asleep, or for any reason mute inclding glimmer or a mime's vow.
        // TODO why the fuck is any of this hardcoded?
        var canNotSpeak = !_actionBlocker.CanSpeak(uid);
        var zombified = TryComp<ZombieComponent>(uid, out var _);
        var muzzled = _inventory.TryGetSlotEntity(uid, "mask", out var maskUid) &&
                      TryComp<AddAccentClothingComponent>(maskUid, out var accent) &&
                      accent.ReplacementPrototype == "mumble";

        // Set this event as handled when the singer should be incapable of singing in order
        // to stop the ActivatableUISystem event from opening the MIDI UI.
        args.Handled = canNotSpeak || muzzled || zombified;

        // Tell the user that they can not sing.
        if (args.Handled)
            _popup.PopupEntity(Loc.GetString("no-sing-while-no-speak"), uid, uid, PopupType.Medium);
    }

    public override void CloseMidiUi(EntityUid uid)
    {
        if (HasComp<ActiveInstrumentComponent>(uid) &&
            TryComp<ActorComponent>(uid, out var actor))
            _instrument.ToggleInstrumentUi(uid, actor.PlayerSession.AttachedEntity ?? EntityUid.Invalid);
    }
}
