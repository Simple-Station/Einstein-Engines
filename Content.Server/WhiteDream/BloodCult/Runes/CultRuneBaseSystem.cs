using System.Linq;
using System.Numerics;
using Content.Server.Bible.Components;
using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Server.Chat.Systems;
using Content.Server.Chemistry.Components;
using Content.Server.Chemistry.Containers.EntitySystems;
using Content.Server.DoAfter;
using Content.Server.Fluids.Components;
using Content.Server.Popups;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Maps;
using Content.Shared.WhiteDream.BloodCult.Components;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Physics.Events;
using Robust.Shared.Prototypes;

namespace Content.Server.WhiteDream.BloodCult.Runes;

public sealed partial class CultRuneBaseSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _protoManager = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly BloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        // Drawing rune
        SubscribeLocalEvent<RuneDrawerComponent, RuneDrawerSelectedMessage>(OnRuneSelected);
        SubscribeLocalEvent<BloodCultistComponent, DrawRuneDoAfter>(OnDrawRune);

        // Erasing rune
        SubscribeLocalEvent<CultRuneBaseComponent, InteractUsingEvent>(EraseOnInteractUsing);
        SubscribeLocalEvent<CultRuneBaseComponent, RuneEraseDoAfterEvent>(OnRuneErase);
        SubscribeLocalEvent<CultRuneBaseComponent, StartCollideEvent>(EraseOnCollding);

        // Rune invoking
        SubscribeLocalEvent<CultRuneBaseComponent, ActivateInWorldEvent>(OnRuneActivate);
    }

    private void OnRuneSelected(Entity<RuneDrawerComponent> ent, ref RuneDrawerSelectedMessage args)
    {
        if (!args.Session.AttachedEntity.HasValue ||
            !_protoManager.TryIndex(args.SelectedRune, out var runeSelector))
        {
            return;
        }

        var timeToDraw = runeSelector.DrawTime;

        // TODO: Buff rune
        // if (HasComp<CultBuffComponent>(whoCalled))
        //     _timeToDraw /= 2;

        var user = args.Session.AttachedEntity.Value;
        if (!CanDrawRune(user))
            return;

        var ev = new DrawRuneDoAfter
        {
            Rune = args.SelectedRune
        };

        var argsDoAfterEvent = new DoAfterArgs(EntityManager, user, timeToDraw, ev, user)
        {
            BreakOnUserMove = true,
            NeedHand = true
        };

        if (!_doAfter.TryStartDoAfter(argsDoAfterEvent))
            return;

        _audio.PlayPvs("/Audio/WhiteDream/BloodCult/butcher.ogg", user, AudioParams.Default.WithMaxDistance(2f));
    }

    private void OnDrawRune(Entity<BloodCultistComponent> ent, ref DrawRuneDoAfter args)
    {
        if (args.Cancelled || !_protoManager.TryIndex(args.Rune, out var runeSelector))
        {
            return;
        }

        var damage = new DamageSpecifier(runeSelector.DrawDamage);
        var bloodToTake = runeSelector.BloodCost;
        // if (HasComp<CultBuffComponent>(args.User))
        //     bloodToTake /= 2;

        if (TryComp<BloodstreamComponent>(args.User, out var bloodstreamComponent))
        {
            _bloodstream.TryModifyBloodLevel(args.User, bloodToTake, bloodstreamComponent, createPuddle: false);
        }
        else
        {
            damage.DamageDict["Slash"] *= 1.5;
        }

        _audio.PlayPvs("/Audio/WhiteDream/BloodCult/blood.ogg", args.User, AudioParams.Default.WithMaxDistance(2f));
        _damageable.TryChangeDamage(args.User, damage, true);
        SpawnRune(args.User, runeSelector.Prototype);
    }

    private void EraseOnInteractUsing(Entity<CultRuneBaseComponent> ent, ref InteractUsingEvent args)
    {
        // Logic for bible erasing
        if (TryComp<BibleComponent>(args.Used, out var bible) && HasComp<BibleUserComponent>(args.User))
        {
            _popup.PopupEntity(Loc.GetString("cult-erased-rune"), args.User, args.User);
            _audio.PlayPvs(bible.HealSoundPath, args.User);
            EntityManager.DeleteEntity(args.Target);
            return;
        }

        if (!TryComp(args.Used, out RuneDrawerComponent? runeDrawer))
        {
            return;
        }

        var ev = new RuneEraseDoAfterEvent();

        var argsDoAfterEvent = new DoAfterArgs(EntityManager, args.User, runeDrawer.EraseTime, ev, ent)
        {
            BreakOnUserMove = true,
            BreakOnDamage = true,
            NeedHand = true
        };

        if (_doAfter.TryStartDoAfter(argsDoAfterEvent))
        {
            _popup.PopupEntity(Loc.GetString("cult-started-erasing-rune"), args.User, args.User);
        }
    }

    private void OnRuneErase(Entity<CultRuneBaseComponent> ent, ref RuneEraseDoAfterEvent args)
    {
        if (args.Cancelled)
            return;

        _popup.PopupEntity(Loc.GetString("cult-erased-rune"), ent, args.User);
        EntityManager.DeleteEntity(ent);
    }

    private void EraseOnCollding(Entity<CultRuneBaseComponent> ent, ref StartCollideEvent args)
    {
        if (!TryComp<SolutionContainerManagerComponent>(args.OtherEntity, out var solutionContainer) ||
            !HasComp<VaporComponent>(args.OtherEntity) && !HasComp<SprayComponent>(args.OtherEntity))
        {
            return;
        }

        if (_solutionContainer.EnumerateSolutions((args.OtherEntity, solutionContainer)).Any(solution =>
                solution.Solution.Comp.Solution.ContainsPrototype(ent.Comp.HolyWaterPrototype)))
        {
            EntityManager.DeleteEntity(ent);
        }
    }

    private void OnRuneActivate(Entity<CultRuneBaseComponent> ent, ref ActivateInWorldEvent args)
    {
        if (args.Handled || !HasComp<BloodCultistComponent>(args.User))
            return;

        args.Handled = true;
        var cultists = new HashSet<EntityUid>
        {
            args.User
        };

        if (ent.Comp.RequiredInvokers > 1)
        {
            cultists = GatherCultists(ent);
        }

        if (cultists.Count < ent.Comp.RequiredInvokers)
        {
            _popup.PopupEntity(Loc.GetString("cult-sacrifice-not-enough-cultists"), args.User, args.User);
            return;
        }

        var tryInvokeEv = new TryInvokeCultRuneEvent(args.User, cultists);
        RaiseLocalEvent(ent, tryInvokeEv);

        if (tryInvokeEv.Cancelled)
            return;

        foreach (var cultist in cultists)
        {
            _chat.TrySendInGameICMessage(cultist, ent.Comp.InvokePhrase, ent.Comp.InvokeChatType, false,
                checkRadioPrefix: false);
        }
    }

    #region UtilityMethods

    private void SpawnRune(EntityUid user, EntProtoId rune)
    {
        var transform = Transform(user);

        var snappedLocalPosition = new Vector2(MathF.Floor(transform.LocalPosition.X) + 0.5f,
            MathF.Floor(transform.LocalPosition.Y) + 0.5f);
        var spawnPosition = _transform.GetMapCoordinates(user);
        var runeEntity = EntityManager.Spawn(rune, spawnPosition);
        _transform.SetLocalPosition(runeEntity, snappedLocalPosition);
    }

    private bool CanDrawRune(EntityUid uid)
    {
        var transform = Transform(uid);
        var gridUid = transform.GridUid;
        var tile = transform.Coordinates.GetTileRef();

        if (!gridUid.HasValue)
        {
            _popup.PopupEntity(Loc.GetString("cult-cant-draw-rune"), uid, uid);
            return false;
        }

        if (tile.HasValue)
            return true;
        _popup.PopupEntity(Loc.GetString("cult-cant-draw-rune"), uid, uid);
        return false;
    }

    private HashSet<EntityUid> GatherCultists(Entity<CultRuneBaseComponent> ent)
    {
        var entities = _lookup.GetEntitiesInRange(ent, ent.Comp.InvokersGatherRange, LookupFlags.Dynamic);
        // TODO: Add constructs support: && !HasComp<ConstructComponent>(x)
        entities.RemoveWhere(x => !HasComp<BloodCultistComponent>(x));
        return entities;
    }

    #endregion
}
