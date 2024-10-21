using Content.Server.Body.Systems;
using Content.Server.Chat.Systems;
using Content.Server.Popups;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Medical.Surgery;
using Content.Shared.Medical.Surgery.Conditions;
using Content.Shared.Medical.Surgery.Effects.Step;
using Content.Shared.Medical.Surgery.Tools;
//using Content.Shared.Medical.Wounds;
using Content.Shared.Interaction;
using Content.Shared.Prototypes;
using Robust.Server.GameObjects;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using System.Linq;

namespace Content.Server.Medical.Surgery;

public sealed class SurgerySystem : SharedSurgerySystem
{
    [Dependency] private readonly BodySystem _body = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    //[Dependency] private readonly WoundsSystem _wounds = default!;

    private readonly List<EntProtoId> _surgeries = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SurgeryToolComponent, AfterInteractEvent>(OnToolAfterInteract);

        SubscribeLocalEvent<SurgeryStepBleedEffectComponent, SurgeryStepEvent>(OnStepBleedComplete);
        SubscribeLocalEvent<SurgeryClampBleedEffectComponent, SurgeryStepEvent>(OnStepClampBleedComplete);
        SubscribeLocalEvent<SurgeryStepAffixPartEffectComponent, SurgeryStepEvent>(OnStepAffixPartComplete);
        SubscribeLocalEvent<SurgeryStepEmoteEffectComponent, SurgeryStepEvent>(OnStepScreamComplete);
        SubscribeLocalEvent<SurgeryStepSpawnEffectComponent, SurgeryStepEvent>(OnStepSpawnComplete);

        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnPrototypesReloaded);

        LoadPrototypes();
    }

    protected override void RefreshUI(EntityUid body)
    {
        if (!HasComp<SurgeryTargetComponent>(body))
            return;

        var surgeries = new Dictionary<NetEntity, List<EntProtoId>>();
        foreach (var surgery in _surgeries)
        {
            if (GetSingleton(surgery) is not { } surgeryEnt)
                continue;

            foreach (var part in _body.GetBodyChildren(body))
            {
                var ev = new SurgeryValidEvent(body, part.Id);
                RaiseLocalEvent(surgeryEnt, ref ev);

                if (ev.Cancelled)
                    continue;

                surgeries.GetOrNew(GetNetEntity(part.Id)).Add(surgery);
            }

        }

        _ui.TrySetUiState(body, SurgeryUIKey.Key, new SurgeryBuiState(surgeries));
    }

    private void OnToolAfterInteract(Entity<SurgeryToolComponent> ent, ref AfterInteractEvent args)
    {
        var user = args.User;
        if (args.Handled ||
            !args.CanReach ||
            args.Target == null ||
            !HasComp<SurgeryTargetComponent>(args.Target) ||
            !TryComp(args.User, out ActorComponent? actor))
        {
            return;
        }
        /* lmao bet
        if (user == args.Target)
        {
            _popup.PopupEntity("You can't perform surgery on yourself!", user, user);
            return;
        }*/

        args.Handled = true;
        _ui.TryOpen(args.Target.Value, SurgeryUIKey.Key, actor.PlayerSession);

        RefreshUI(args.Target.Value);
    }

    private void OnStepBleedComplete(Entity<SurgeryStepBleedEffectComponent> ent, ref SurgeryStepEvent args)
    {
        //_wounds.AddWound(args.Body, ent.Comp.Damage, WoundType.Surgery, TimeSpan.MaxValue);
    }

    private void OnStepClampBleedComplete(Entity<SurgeryClampBleedEffectComponent> ent, ref SurgeryStepEvent args)
    {
        //_wounds.RemoveWounds(ent.Owner, WoundType.Surgery);
    }

    private void OnStepAffixPartComplete(Entity<SurgeryStepAffixPartEffectComponent> ent, ref SurgeryStepEvent args)
    {
        if (!TryComp(args.Surgery, out SurgeryPartRemovedConditionComponent? removedComp))
            return;

        var targetPart = _body.GetBodyChildrenOfType(args.Body, removedComp.Part, symmetry: removedComp.Symmetry).FirstOrDefault();

        if (targetPart != default)
        {
            var ev = new BodyPartEnableChangedEvent(true);
            RaiseLocalEvent(targetPart.Id, ref ev);
            var healingValue = 20f;
            if (targetPart.Component.Integrity + healingValue > 100)
                healingValue = 100 - targetPart.Component.Integrity;

            _body.TryChangeIntegrity(targetPart, -healingValue, false,
                _body.GetTargetBodyPart(targetPart.Component.PartType, targetPart.Component.Symmetry), out _);
        }

    }

    private void OnStepScreamComplete(Entity<SurgeryStepEmoteEffectComponent> ent, ref SurgeryStepEvent args)
    {
        _chat.TryEmoteWithChat(args.Body, ent.Comp.Emote);
    }
    private void OnStepSpawnComplete(Entity<SurgeryStepSpawnEffectComponent> ent, ref SurgeryStepEvent args)
    {
        if (TryComp(args.Body, out TransformComponent? xform))
            SpawnAtPosition(ent.Comp.Entity, xform.Coordinates);
    }

    private void OnPrototypesReloaded(PrototypesReloadedEventArgs args)
    {
        if (args.WasModified<EntityPrototype>())
            LoadPrototypes();
    }

    private void LoadPrototypes()
    {
        _surgeries.Clear();
        foreach (var entity in _prototypes.EnumeratePrototypes<EntityPrototype>())
        {
            if (entity.HasComponent<SurgeryComponent>())
                _surgeries.Add(new EntProtoId(entity.ID));
        }
    }
}