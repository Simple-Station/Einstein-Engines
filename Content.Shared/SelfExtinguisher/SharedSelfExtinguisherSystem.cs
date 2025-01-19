using Content.Shared.Actions;
using Content.Shared.Charges.Components;
using Content.Shared.Examine;
using Content.Shared.Inventory;
using Content.Shared.Verbs;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Shared.SelfExtinguisher;

public abstract partial class SharedSelfExtinguisherSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly ActionContainerSystem _actionContainer = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SelfExtinguisherComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SelfExtinguisherComponent, ExaminedEvent>(OnExamined);

        SubscribeLocalEvent<SelfExtinguisherComponent, InventoryRelayedEvent<GetVerbsEvent<EquipmentVerb>>>(GetRelayedVerbs);
        SubscribeLocalEvent<SelfExtinguisherComponent, GetItemActionsEvent>(OnGetActions);
        SubscribeLocalEvent<SelfExtinguisherComponent, GetVerbsEvent<EquipmentVerb>>(OnGetVerbs);
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
            // TODO add fire extinguisher svg sprite
            Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/snow.svg.192dpi.png")),
            Text = Loc.GetString("self-extinguisher-verb"),
            EventTarget = uid,
            ExecutionEventArgs = new SelfExtinguishEvent() { Performer = args.User }
        };

        args.Verbs.Add(verb);
    }

    private void OnGetActions(EntityUid uid, SelfExtinguisherComponent component, GetItemActionsEvent args)
    {
        if (component.ActionEntity == null || args.InHands)
            return;

        args.AddAction(component.ActionEntity.Value);
    }

    private void OnMapInit(EntityUid uid, SelfExtinguisherComponent component, MapInitEvent args)
    {
        if (!_actionContainer.EnsureAction(uid, ref component.ActionEntity, out _, component.Action))
            return;

        // The components SelfExtinguisherComponent and LimitedChargesComponent will be the source of truth
        // regarding the cooldowns and charges, and the action component will just mirror any changes.
        _actions.SetUseDelay(component.ActionEntity, component.Cooldown);
        if (TryComp<LimitedChargesComponent>(uid, out var charges))
        {
            _actions.SetCharges(component.ActionEntity, charges.Charges);
            _actions.SetMaxCharges(component.ActionEntity, charges.MaxCharges);
        }
    }

    private void OnExamined(EntityUid uid, SelfExtinguisherComponent component, ExaminedEvent args)
    {
        if (TryComp<LimitedChargesComponent>(uid, out var charges) && charges.Charges == 0)
            return;

        var curTime = _timing.CurTime;
        if (component.NextExtinguish > curTime)
        {
            args.PushMarkup(Loc.GetString("self-extinguisher-examine-cooldown-recharging",
                ("cooldown", Math.Ceiling((component.NextExtinguish - curTime).TotalSeconds))));
            return;
        }

        args.PushMarkup(Loc.GetString("self-extinguisher-examine-cooldown-ready"));
    }
}

// <summary>
//   Raised on an attempt to self-extinguish.
// </summary>
public sealed partial class SelfExtinguishEvent : InstantActionEvent { }
