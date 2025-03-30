using Content.Server.Bible.Components;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Administration.Logs;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Database;
using Content.Shared.Chapel;
using Content.Shared.DoAfter;
using Content.Shared.Humanoid;
using Content.Shared.Mind;
using Content.Shared.Popups;
using Content.Shared.Psionics.Glimmer;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.Chapel;

public sealed class SacrificialAltarSystem : SharedSacrificialAltarSystem
{
    [Dependency] private readonly GlimmerSystem _glimmer = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SacrificialAltarComponent, SacrificeDoAfterEvent>(OnDoAfter);
    }

    private void OnDoAfter(Entity<SacrificialAltarComponent> ent, ref SacrificeDoAfterEvent args)
    {
        ent.Comp.SacrificeStream = _audio.Stop(ent.Comp.SacrificeStream);
        ent.Comp.DoAfter = null;

        if (args.Cancelled || args.Handled || args.Args.Target is not { } target
            || !TryComp<PsionicComponent>(target, out var psionic)
            || !_mind.TryGetMind(target, out var _, out var _))
            return;

        _adminLogger.Add(LogType.Action, LogImpact.Extreme, $"{ToPrettyString(args.Args.User):player} sacrificed {ToPrettyString(target):target} on {ToPrettyString(ent):altar}");

        // lower glimmer by a random amount
        _glimmer.DeltaGlimmerInput(ent.Comp.GlimmerReduction * psionic.CurrentAmplification);

        if (ent.Comp.RewardPool is not null && _random.Prob(ent.Comp.BaseItemChance * psionic.CurrentDampening))
        {
            var proto = _proto.Index(_random.Pick(ent.Comp.RewardPool));
            Spawn(proto.ToString(), Transform(ent).Coordinates);
        }
        // TODO GOLEMS: create a soul crystal and transfer mind into it

        // finally gib the targets old body
        if (TryComp<BodyComponent>(target, out var body))
            _body.GibBody(target, gibOrgans: false, body, launchGibs: true);
        else
            QueueDel(target);
    }

    protected override void AttemptSacrifice(Entity<SacrificialAltarComponent> ent, EntityUid user, EntityUid target)
    {
        if (ent.Comp.DoAfter != null)
            return;

        // can't sacrifice yourself
        if (user == target)
        {
            _popup.PopupEntity(Loc.GetString("altar-failure-reason-self"), ent, user, PopupType.SmallCaution);
            return;
        }

        // you need to be psionic OR bible user
        if (!HasComp<PsionicComponent>(user) && !HasComp<BibleUserComponent>(user))
        {
            _popup.PopupEntity(Loc.GetString("altar-failure-reason-user"), ent, user, PopupType.SmallCaution);
            return;
        }

        // and no golems or familiars or whatever should be sacrificing
        if (!HasComp<HumanoidAppearanceComponent>(user))
        {
            _popup.PopupEntity(Loc.GetString("altar-failure-reason-user-humanoid"), ent, user, PopupType.SmallCaution);
            return;
        }

        // prevent psichecking SSD people...
        // notably there is no check in OnDoAfter so you can't alt f4 to survive being sacrificed
        if (!HasComp<ActorComponent>(target) || _mind.GetMind(target) == null)
        {
            _popup.PopupEntity(Loc.GetString("altar-failure-reason-target-catatonic", ("target", target)), ent, user, PopupType.SmallCaution);
            return;
        }

        // TODO: there should be a penalty to the user for psichecking like this
        if (!HasComp<PsionicComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("altar-failure-reason-target", ("target", target)), ent, user, PopupType.SmallCaution);
            return;
        }

        if (!HasComp<HumanoidAppearanceComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("altar-failure-reason-target-humanoid", ("target", target)), ent, user, PopupType.SmallCaution);
            return;
        }

        _popup.PopupEntity(Loc.GetString("altar-sacrifice-popup", ("user", user), ("target", target)), ent, PopupType.LargeCaution);

        ent.Comp.SacrificeStream = _audio.PlayPvs(ent.Comp.SacrificeSound, ent)?.Entity;

        var ev = new SacrificeDoAfterEvent();
        var args = new DoAfterArgs(EntityManager, user, ent.Comp.SacrificeTime, ev, target: target, eventTarget: ent)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            BreakOnWeightlessMove = true,
            NeedHand = true
        };
        DoAfter.TryStartDoAfter(args, out ent.Comp.DoAfter);
    }
}
