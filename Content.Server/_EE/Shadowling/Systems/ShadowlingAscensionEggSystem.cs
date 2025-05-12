using Content.Server.Actions;
using Content.Server.Chat.Systems;
using Content.Server.Light.Components;
using Content.Server.Light.EntitySystems;
using Content.Server.Polymorph.Systems;
using Content.Server.Popups;
using Content.Server.Storage.EntitySystems;
using Content.Shared._EE.Nightmare.Components;
using Content.Shared._EE.Shadowling;
using Content.Shared._EE.Shadowling.Components;
using Content.Shared._EE.Shadowling.Systems;
using Content.Shared.Destructible;
using Content.Shared.Examine;
using Content.Shared.Light.Components;
using Content.Shared.Popups;
using Content.Shared.Storage;
using Content.Shared.Verbs;
using Robust.Server.GameObjects;
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
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    [Dependency] private readonly ShadowlingSystem _shadowling = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly PoweredLightSystem _poweredLight = default!;

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

            if (_timing.CurTime >= comp.NextUpdateTime - comp.AscendingEffectInterval && !comp.AscendingEffectAdded)
            {
                var effectEnt = Spawn(comp.AscendingEffect, _transform.GetMapCoordinates(uid));
                _transform.SetParent(effectEnt, uid);
                comp.AscendingEffectEntity = effectEnt;
                comp.AscendingEffectAdded = true;
            }

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
        if (component.AscendingEffectEntity != null)
            QueueDel(component.AscendingEffectEntity);

        if (component.Creator == null)
            return;

        // This indicates that the shadowling was inside the egg
        if (component.StartTimer)
        {
            var shadowlingComp = EntityManager.GetComponent<ShadowlingComponent>(component.Creator.Value);
            _shadowling.OnPhaseChanged(component.Creator.Value, shadowlingComp, ShadowlingPhases.FailedAscension);

            component.StartTimer = false;
        }
    }

    private void OnExamined(EntityUid uid, ShadowlingAscensionEggComponent component, ExaminedEvent args)
    {
        if (!component.StartTimer && component.Creator == args.Examiner)
        {
            args.PushMarkup($"[color=red]{Loc.GetString("shadowling-ascension-start-warning")}[/color]");
        }
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

        _actions.RemoveAction(shadowling.ActionAscendanceEntity);

        shadowling.IsAscending = true;
        component.StartTimer = true;
        component.NextUpdateTime = _timing.CurTime + component.UpdateInterval;

        _entityStorage.Insert(uid, eggUid);

        _chat.DispatchGlobalAnnouncement(
            Loc.GetString("shadowling-ascension-message"),
            colorOverride: Color.Red); // todo: sound

        var effectEnt = Spawn(component.ShadowlingInside, _transform.GetMapCoordinates(eggUid));
        _transform.SetParent(effectEnt, eggUid);

        component.ShadowlingInsideEntity = effectEnt;
    }

    private void DoAscend(EntityUid uid, ShadowlingAscensionEggComponent component)
    {
        if (component.ShadowlingInsideEntity != null)
            QueueDel(component.ShadowlingInsideEntity);

        if (component.Creator == null)
            return;

        component.StartTimer = false;

        DestroyLights();

        _entityStorage.OpenStorage(uid);
        _entityStorage.Remove(component.Creator.Value, uid);

        var shadowlings = new List<EntityUid>();
        var thralls = new List<EntityUid>();

        var query = EntityQueryEnumerator<ShadowlingComponent>();
        while (query.MoveNext(out var slingUid, out _))
        {
            shadowlings.Add(slingUid);
        }

        var queryThrall = EntityQueryEnumerator<ThrallComponent>();
        while (queryThrall.MoveNext(out var thrallUid, out _))
        {
            thralls.Add(thrallUid);
        }

        foreach (var sling in shadowlings)
        {
            var newUid = _polymorph.PolymorphEntity(sling, "ShadowlingAscendantPolymorph");

            if (newUid == null)
                return;

            if (!TryComp<ShadowlingComponent>(newUid.Value, out var ascendant))
                continue;

            _actions.RemoveAction(ascendant.ActionHatchEntity);
            ascendant.CurrentPhase = ShadowlingPhases.Ascension;

            _shadowling.OnPhaseChanged(newUid.Value, ascendant, ShadowlingPhases.Ascension);
        }

        foreach (var thrall in thralls)
        {
            if (HasComp<LesserShadowlingComponent>(thrall))
            {
                EnsureComp<NightmareComponent>(thrall);
                continue; // Don't polymorph the lesser again
            }

            var newUid = _polymorph.PolymorphEntity(thrall, "ShadowPolymorph");

            if (newUid == null)
                return;

            EnsureComp<NightmareComponent>(newUid.Value);
        }
    }

    private void DestroyLights()
    {
        var lights = EntityQueryEnumerator<PoweredLightComponent>();
        while (lights.MoveNext(out var uid, out var light))
            _poweredLight.TryDestroyBulb(uid, light);
    }
}
