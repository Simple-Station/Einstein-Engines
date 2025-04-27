using Content.Server.Chat.Systems;
using Content.Server.Popups;
using Content.Server.Storage.EntitySystems;
using Content.Shared._EE.Shadowling;
using Content.Shared._EE.Shadowling.Components;
using Content.Shared.Destructible;
using Content.Shared.Examine;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Timing;
using Robust.Shared.Utility;


namespace Content.Server._EE.Shadowling;


/// <summary>
/// This handles the Ascension Egg system.
/// </summary>
public sealed class ShadowlingAscensionEggSystem : EntitySystem
{
    [Dependency] private readonly EntityStorageSystem _entityStorage = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly ChatSystem _chat = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingAscensionEggComponent, GetVerbsEvent<Verb>>(OnGetVerbs);
        SubscribeLocalEvent<ShadowlingAscensionEggComponent, DestructionEventArgs>(OnDestruction);

        SubscribeLocalEvent<ShadowlingAscensionEggComponent, ExaminedEvent>(OnExamined);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ShadowlingAscensionEggComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!comp.StartTimer)
                continue;

            if (_timing.CurTime < comp.NextUpdateTime)
                continue;

            DoAscend(uid, comp);
        }
    }

    private void OnGetVerbs(EntityUid uid, ShadowlingAscensionEggComponent component, GetVerbsEvent<Verb> args)
    {
        if (component.Creator == null)
            return;

        if (args.User != component.Creator)
            return;

        args.Verbs.Add(
            new Verb
            {
                Act = () => TryAscend(args.User, args.Target, component),
                Text = Loc.GetString(component.VerbName),
                Icon = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/VerbIcons/settings.svg.192dpi.png")) //todo: custom icon
            });
    }

    private void OnDestruction(EntityUid uid, ShadowlingAscensionEggComponent component, DestructionEventArgs args)
    {
        if (component.Creator == null)
            return;

        // This indicates that the shadowling was inside the egg
        if (component.StartTimer)
        {
            RaiseLocalEvent(component.Creator.Value, new PhaseChangedEvent(ShadowlingPhases.FailedAscension));
            component.StartTimer = false;
        }
    }

    private void OnExamined(EntityUid uid, ShadowlingAscensionEggComponent component, ExaminedEvent args)
    {
        if (!component.StartTimer && component.Creator == args.Examiner)
        {
            args.PushMarkup($"[color=red]{Loc.GetString("shadowling-ascension-start-warning")}[/color]");
        }

        // todo:  add indicator for damage here?
    }

private void TryAscend(EntityUid uid, EntityUid eggUid, ShadowlingAscensionEggComponent component)
    {
        if (!HasComp<ShadowlingComponent>(uid))
        {
            _popup.PopupEntity(Loc.GetString("shadowling-ascension-not-shadowling"), uid, uid, PopupType.MediumCaution);
            return;
        }

        if (component.Creator == null)
        {
            _popup.PopupEntity(Loc.GetString("shadowling-ascension-not-creator"), uid, uid, PopupType.MediumCaution);
            return;
        }

        // Don't ascend if another shadowling is ascending.
        // If one shadowling finishes ascension, the remaining
        // shadowlings will gain Ascendance powers too, either way.
        var entQuery = EntityQueryEnumerator<ShadowlingComponent>();
        while (entQuery.MoveNext(out _, out var sling))
        {
            if (sling.CurrentPhase == ShadowlingPhases.Ascension)
            {
                _popup.PopupEntity(Loc.GetString("shadowling-ascension-already-ascended"), uid, uid, PopupType.MediumCaution);
                return;
            }

            if (sling.IsAscending)
            {
                _popup.PopupEntity(Loc.GetString("shadowling-ascension-ascending"), uid,uid, PopupType.MediumCaution);
                return;
            }
        }

        // Start Ascension
        var shadowling = EntityManager.GetComponent<ShadowlingComponent>(uid);

        shadowling.IsAscending = true;
        component.StartTimer = true;
        component.NextUpdateTime = _timing.CurTime + component.UpdateInterval;

        _entityStorage.Insert(uid, eggUid);

        _chat.DispatchGlobalAnnouncement(
            Loc.GetString("shadowling-ascension-message"),
            colorOverride: Color.Red); // todo: sound
    }

    private void DoAscend(EntityUid uid, ShadowlingAscensionEggComponent component)
    {
        if (component.Creator == null)
            return;

        component.StartTimer = false;

        _entityStorage.OpenStorage(uid);
        _entityStorage.Remove(component.Creator.Value, uid);

        var query = EntityQueryEnumerator<ShadowlingComponent>();
        while (query.MoveNext(out var slingUid, out var sling))
        {
            sling.CurrentPhase = ShadowlingPhases.Ascension;
            RaiseLocalEvent(slingUid, new PhaseChangedEvent(ShadowlingPhases.Ascension));
        }
    }
}
