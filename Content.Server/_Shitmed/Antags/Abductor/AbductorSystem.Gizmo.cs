﻿using Content.Shared._Shitmed.Antags.Abductor;
using Content.Shared._Shitmed.Medical.Surgery;
using Content.Shared.ActionBlocker;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Tag;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server._Shitmed.Antags.Abductor;

public sealed partial class AbductorSystem : SharedAbductorSystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly ActionBlockerSystem _actionBlockerSystem = default!;
    [Dependency] private readonly IGameTiming _time = default!;
    private static readonly ProtoId<TagPrototype> Abductor = "Abductor";
    public void InitializeGizmo()
    {
        SubscribeLocalEvent<AbductorGizmoComponent, AfterInteractEvent>(OnGizmoInteract);
        SubscribeLocalEvent<AbductorGizmoComponent, MeleeHitEvent>(OnGizmoHitInteract);

        SubscribeLocalEvent<AbductorGizmoComponent, AbductorGizmoMarkDoAfterEvent>(OnGizmoDoAfter);
    }

    private void OnGizmoHitInteract(Entity<AbductorGizmoComponent> ent, ref MeleeHitEvent args)
    {
        if (args.HitEntities.Count != 1) return;
        var target = args.HitEntities[0];
        if (!HasComp<SurgeryTargetComponent>(target)) return;
        GizmoUse(ent, target, args.User);
    }

    private void OnGizmoInteract(Entity<AbductorGizmoComponent> ent, ref AfterInteractEvent args)
    {
        if (!_actionBlockerSystem.CanInstrumentInteract(args.User, args.Used, args.Target)) return;
        if (!args.Target.HasValue) return;

        if (TryComp<AbductorConsoleComponent>(args.Target, out var console))
        {
            console.Target = ent.Comp.Target;
            _popup.PopupClient(Loc.GetString("abductors-ui-gizmo-transferred"), args.User);
            _color.RaiseEffect(Color.FromHex("#00BA00"), new List<EntityUid>(2) { ent.Owner, args.Target.Value }, Filter.Pvs(args.User, entityManager: EntityManager));
            UpdateGui(console.Target, (args.Target.Value, console));
            return;
        }

        if (HasComp<SurgeryTargetComponent>(args.Target))
            GizmoUse(ent, args.Target.Value, args.User);
    }

    private void GizmoUse(Entity<AbductorGizmoComponent> ent, EntityUid target, EntityUid user)
    {
        var time = TimeSpan.FromSeconds(6);
        if (_tags.HasTag(target, Abductor))
            time = TimeSpan.FromSeconds(0.5);

        var doAfter = new DoAfterArgs(EntityManager, user, time, new AbductorGizmoMarkDoAfterEvent(), ent, target, ent.Owner)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            DistanceThreshold = 1f
        };
        _doAfter.TryStartDoAfter(doAfter);
    }

    private void OnGizmoDoAfter(Entity<AbductorGizmoComponent> ent, ref AbductorGizmoMarkDoAfterEvent args)
    {
        if (args.Target is null) return;
        ent.Comp.Target = GetNetEntity(args.Target);
        EnsureComp<AbductorVictimComponent>(args.Target.Value, out var victimComponent);
        victimComponent.LastActivation = _time.CurTime + TimeSpan.FromMinutes(5);

        victimComponent.Position ??= EnsureComp<TransformComponent>(args.Target.Value).Coordinates;
    }
}
