using Content.Shared.Actions;
using Content.Shared.Charges.Components;
using Content.Shared.Charges.Systems;
using Content.Shared.Examine;
using Content.Shared.Extinguisher;
using Content.Shared.IdentityManagement;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Verbs;
using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server.Extinguisher;

public sealed partial class SelfExtinguisherSystem : EntitySystem
{
    [Dependency] private readonly FlammableSystem _flammable = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedChargesSystem _charges = default!;
    [Dependency] private readonly IgniteFromGasSystem _ignite = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly ActionContainerSystem _actionContainer = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;

    private const float UpdateTimer = 1f;
    private float _timer;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SelfExtinguisherComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SelfExtinguisherComponent, ExaminedEvent>(OnExamined);

        SubscribeLocalEvent<SelfExtinguisherComponent, InventoryRelayedEvent<GetVerbsEvent<EquipmentVerb>>>(GetRelayedVerbs);
        SubscribeLocalEvent<SelfExtinguisherComponent, GetItemActionsEvent>(OnGetActions);
        SubscribeLocalEvent<SelfExtinguisherComponent, GetVerbsEvent<EquipmentVerb>>(OnGetVerbs);

        SubscribeLocalEvent<SelfExtinguisherComponent, SelfExtinguishEvent>(OnSelfExtinguish);
    }

    private void GetRelayedVerbs(EntityUid uid, SelfExtinguisherComponent component, InventoryRelayedEvent<GetVerbsEvent<EquipmentVerb>> args)
    {
        OnGetVerbs(uid, component, args.Args);
    }

    private void OnGetVerbs(EntityUid uid, SelfExtinguisherComponent component, GetVerbsEvent<EquipmentVerb> args)
    {
        if (!_inventory.TryGetContainingSlot(uid, out var _))
            return;

        var verb = new EquipmentVerb()
        {
            Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/snow.svg.192dpi.png")),
            Text = Loc.GetString("self-extinguisher-verb"),
        };

        verb.EventTarget = uid;
        verb.ExecutionEventArgs = new SelfExtinguishEvent() { Performer = args.User };

        args.Verbs.Add(verb);
    }

    private void OnGetActions(EntityUid uid, SelfExtinguisherComponent component, GetItemActionsEvent args)
    {
        if (component.ActionEntity != null && !args.InHands)
            args.AddAction(component.ActionEntity.Value);
    }

    private void OnMapInit(EntityUid uid, SelfExtinguisherComponent component, MapInitEvent args)
    {
        _actionContainer.EnsureAction(uid, ref component.ActionEntity, out var action, component.Action);
    }

    private void OnSelfExtinguish(EntityUid uid, SelfExtinguisherComponent component, SelfExtinguishEvent args)
    {
        TryExtinguish(args.Performer, uid, component);
    }

    private void TryExtinguish(EntityUid user, EntityUid uid, SelfExtinguisherComponent? selfExtinguisher = null, FlammableComponent? flammable = null)
    {
        if (!_container.TryGetContainingContainer(uid, out var container) ||
            !Resolve(container.Owner, ref flammable) ||
            !Resolve(uid, ref selfExtinguisher))
            return;
        var target = container.Owner;
        var targetIdentity = Identity.Entity(target, EntityManager);
        var suffix = user == target ? "self" : "other";

        if (!flammable.OnFire)
        {
            _popup.PopupEntity(Loc.GetString($"self-extinguisher-not-on-fire-{suffix}", ("item", uid), ("target", targetIdentity)), target, user);
            return;
        }

        TryComp<LimitedChargesComponent>(uid, out var charges);
        if (_charges.IsEmpty(uid, charges))
        {
            _popup.PopupEntity(Loc.GetString("self-extinguisher-no-charges", ("item", uid)), target, user);
            return;
        }

        var curTime = _timing.CurTime;
        if (selfExtinguisher.NextExtinguish > curTime)
        {
            _popup.PopupEntity(Loc.GetString($"self-extinguisher-on-cooldown", ("item", uid)), target, user);
            return;
        }


        if (selfExtinguisher.RequiresIgniteFromGasImmune &&
            ((TryComp<IgniteFromGasComponent>(target, out var ignite) && !ignite.HasImmunity) ||
            false)) // TODO check for ignite immunity using another way
        {
            _popup.PopupEntity(Loc.GetString($"self-extinguisher-not-immune-to-fire-{suffix}", ("item", uid), ("target", targetIdentity)), target, user);
            return;
        }

        _flammable.Extinguish(target, flammable);

        _popup.PopupPredicted(
            Loc.GetString("self-extinguisher-extinguish-other", ("item", uid), ("target", targetIdentity)),
            target, target
        );
        _popup.PopupEntity(
            Loc.GetString("self-extinguisher-extinguish-self", ("item", uid)),
            target, target
        );

        if (charges != null)
            _charges.UseCharge(uid, charges);
        selfExtinguisher.NextExtinguish = curTime + selfExtinguisher.Cooldown;

        _audio.PlayPvs(selfExtinguisher.Sound, uid, selfExtinguisher.Sound.Params.WithVariation(0.125f));
    }

    private void OnExamined(EntityUid uid, SelfExtinguisherComponent component, ExaminedEvent args)
    {
        var curTime = _timing.CurTime;
        if (component.NextExtinguish > curTime)
            args.PushMarkup(Loc.GetString("self-extinguisher-examine-cooldown", ("cooldown", (component.NextExtinguish - curTime).Seconds)));
    }
}
