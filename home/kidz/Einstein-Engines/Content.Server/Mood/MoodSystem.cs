using Content.Server.Chat.Managers;
using Content.Server.Popups;
using Content.Shared.Alert;
using Content.Shared.Chat;
using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Mood;
using Content.Shared.Overlays;
using Content.Shared.Popups;
using Content.Shared.Traits.Assorted.Components;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Timer = Robust.Shared.Timing.Timer;
using Robust.Server.Player;
using Robust.Shared.Player;
using Robust.Shared.Configuration;
using Content.Shared.CCVar;

namespace Content.Server.Mood;

public sealed class MoodSystem : EntitySystem
{
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifier = default!;
    [Dependency] private readonly SharedJetpackSystem _jetpack = default!;
    [Dependency] private readonly MobThresholdSystem _mobThreshold = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;

#if RELEASE
    // Disable Mood for tests, because of a stupid race condition where if it spawns an Urist McHarpy,
    // the Harpy will choke during the test, creating a mood alert.
    // And then cause a debug assert.
    private bool _debugMode;
#else
    private bool _debugMode = true;
#endif


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MoodComponent, ComponentStartup>(OnInit);
        SubscribeLocalEvent<MoodComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<MoodComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<MoodComponent, MoodEffectEvent>(OnMoodEffect);
        SubscribeLocalEvent<MoodComponent, DamageChangedEvent>(OnDamageChange);
        SubscribeLocalEvent<MoodComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMoveSpeed);
        SubscribeLocalEvent<MoodComponent, MoodRemoveEffectEvent>(OnRemoveEffect);
    }

    private void OnShutdown(EntityUid uid, MoodComponent component, ComponentShutdown args)
    {
        _alerts.ClearAlertCategory(uid, AlertCategory.Mood);
    }

    private void OnRemoveEffect(EntityUid uid, MoodComponent component, MoodRemoveEffectEvent args)
    {
        if (_debugMode)
            return;

        if (component.UncategorisedEffects.TryGetValue(args.EffectId, out _))
            RemoveTimedOutEffect(uid, args.EffectId);
        else
            foreach (var (category, id) in component.CategorisedEffects)
                if (id == args.EffectId)
                {
                    RemoveTimedOutEffect(uid, args.EffectId, category);
                    return;
                }
    }

    private void OnRefreshMoveSpeed(EntityUid uid, MoodComponent component, RefreshMovementSpeedModifiersEvent args)
    {
        if (_debugMode
            || component.CurrentMoodThreshold is > MoodThreshold.Meh and < MoodThreshold.Good or MoodThreshold.Dead
            || _jetpack.IsUserFlying(uid))
            return;

        // This ridiculous math serves a purpose making high mood less impactful on movement speed than low mood
        var modifier =
            Math.Clamp(
                (component.CurrentMoodLevel >= component.MoodThresholds[MoodThreshold.Neutral])
                    ? _config.GetCVar(CCVars.MoodIncreasesSpeed)
                        ? MathF.Pow(1.003f, component.CurrentMoodLevel - component.MoodThresholds[MoodThreshold.Neutral])
                        : 1
                    : _config.GetCVar(CCVars.MoodDecreasesSpeed)
                        ? 2 - component.MoodThresholds[MoodThreshold.Neutral] / component.CurrentMoodLevel
                        : 1,
                component.MinimumSpeedModifier,
                component.MaximumSpeedModifier);

        args.ModifySpeed(1, modifier);
    }

    private void OnMoodEffect(EntityUid uid, MoodComponent component, MoodEffectEvent args)
    {
        if (_debugMode
            || !_config.GetCVar(CCVars.MoodEnabled)
            || !_prototypeManager.TryIndex<MoodEffectPrototype>(args.EffectId, out var prototype) )
            return;

        var ev = new OnMoodEffect(uid, args.EffectId, args.EffectModifier, args.EffectOffset);
        RaiseLocalEvent(uid, ref ev);

        ApplyEffect(uid, component, prototype, ev.EffectModifier, ev.EffectOffset);
    }

    private void ApplyEffect(EntityUid uid, MoodComponent component, MoodEffectPrototype prototype, float eventModifier = 1, float eventOffset = 0)
    {
        // Apply categorised effect
        if (prototype.Category != null)
        {
            if (component.CategorisedEffects.TryGetValue(prototype.Category, out var oldPrototypeId))
            {
                if (!_prototypeManager.TryIndex<MoodEffectPrototype>(oldPrototypeId, out var oldPrototype))
                    return;

                // Don't send the moodlet popup if we already have the moodlet.
                if (!component.CategorisedEffects.ContainsValue(prototype.ID))
                    SendEffectText(uid, prototype);

                if (prototype.ID != oldPrototype.ID)
                    component.CategorisedEffects[prototype.Category] = prototype.ID;
            }
            else
                component.CategorisedEffects.Add(prototype.Category, prototype.ID);

            if (prototype.Timeout != 0)
                Timer.Spawn(TimeSpan.FromSeconds(prototype.Timeout), () => RemoveTimedOutEffect(uid, prototype.ID, prototype.Category));
        }
        // Apply uncategorised effect
        else
        {
            if (component.UncategorisedEffects.TryGetValue(prototype.ID, out _))
                return;

            var moodChange = prototype.MoodChange * eventModifier + eventOffset;
            if (moodChange == 0)
                return;

            // Don't send the moodlet popup if we already have the moodlet.
            if (!component.UncategorisedEffects.ContainsKey(prototype.ID))
                SendEffectText(uid, prototype);

            component.UncategorisedEffects.Add(prototype.ID, moodChange);

            if (prototype.Timeout != 0)
                Timer.Spawn(TimeSpan.FromSeconds(prototype.Timeout), () => RemoveTimedOutEffect(uid, prototype.ID));
        }

        RefreshMood(uid, component);
    }

    private void SendEffectText(EntityUid uid, MoodEffectPrototype prototype)
    {
        if (prototype.Hidden)
            return;

        _popup.PopupEntity(prototype.Description, uid, uid, (prototype.MoodChange > 0) ? PopupType.Medium : PopupType.MediumCaution);
    }

    private void RemoveTimedOutEffect(EntityUid uid, string prototypeId, string? category = null)
    {
        if (!TryComp<MoodComponent>(uid, out var comp))
            return;

        if (category == null)
        {
            if (!comp.UncategorisedEffects.ContainsKey(prototypeId))
                return;
            comp.UncategorisedEffects.Remove(prototypeId);
        }
        else
        {
            if (!comp.CategorisedEffects.TryGetValue(category, out var currentProtoId)
                || currentProtoId != prototypeId
                || !_prototypeManager.HasIndex<MoodEffectPrototype>(currentProtoId))
                return;
            comp.CategorisedEffects.Remove(category);
        }

        ReplaceMood(uid, prototypeId);
        RefreshMood(uid, comp);
    }

    /// <summary>
    ///     Some moods specifically create a moodlet upon expiration. This is normally used for "Addiction" type moodlets,
    ///     such as a positive moodlet from an addictive substance that becomes a negative moodlet when a timer ends.
    /// </summary>
    /// <remarks>
    ///     Moodlets that use this should probably also share a category with each other, but this isn't necessarily required.
    ///     Only if you intend that "Re-using the drug" should also remove the negative moodlet.
    /// </remarks>
    private void ReplaceMood(EntityUid uid, string prototypeId)
    {
        if (!_prototypeManager.TryIndex<MoodEffectPrototype>(prototypeId, out var proto)
            || proto.MoodletOnEnd is null)
            return;

        var ev = new MoodEffectEvent(proto.MoodletOnEnd);
        EntityManager.EventBus.RaiseLocalEvent(uid, ev);
    }

    private void OnMobStateChanged(EntityUid uid, MoodComponent component, MobStateChangedEvent args)
    {
        if (_debugMode)
            return;

        if (args.NewMobState == MobState.Dead && args.OldMobState != MobState.Dead)
        {
            var ev = new MoodEffectEvent("Dead");
            RaiseLocalEvent(uid, ev);
        }
        else if (args.OldMobState == MobState.Dead && args.NewMobState != MobState.Dead)
        {
            var ev = new MoodRemoveEffectEvent("Dead");
            RaiseLocalEvent(uid, ev);
        }
        RefreshMood(uid, component);
    }

    // <summary>
    //      Recalculate the mood level of an entity by summing up all moodlets.
    // </summary>
    private void RefreshMood(EntityUid uid, MoodComponent component)
    {
        var amount = 0f;

        foreach (var (_, protoId) in component.CategorisedEffects)
        {
            if (!_prototypeManager.TryIndex<MoodEffectPrototype>(protoId, out var prototype))
                continue;

            amount += prototype.MoodChange;
        }

        foreach (var (_, value) in component.UncategorisedEffects)
            amount += value;

        SetMood(uid, amount, component, refresh: true);
    }

    private void OnInit(EntityUid uid, MoodComponent component, ComponentStartup args)
    {
        if (_debugMode)
            return;

        if (_config.GetCVar(CCVars.MoodModifiesThresholds)
            && TryComp<MobThresholdsComponent>(uid, out var mobThresholdsComponent)
            && _mobThreshold.TryGetThresholdForState(uid, MobState.Critical, out var critThreshold, mobThresholdsComponent))
            component.CritThresholdBeforeModify = critThreshold.Value;

        EnsureComp<NetMoodComponent>(uid);
        RefreshMood(uid, component);
    }

    private void SetMood(EntityUid uid, float amount, MoodComponent? component = null, bool force = false, bool refresh = false)
    {
        if (!_config.GetCVar(CCVars.MoodEnabled)
            || !Resolve(uid, ref component)
            || component.CurrentMoodThreshold == MoodThreshold.Dead && !refresh)
            return;

        var neutral = component.MoodThresholds[MoodThreshold.Neutral];
        var ev = new OnSetMoodEvent(uid, amount, false);
        RaiseLocalEvent(uid, ref ev);

        if (ev.Cancelled)
            return;
        else
        {
            uid = ev.Receiver;
            amount = ev.MoodChangedAmount;
        }

        var newMoodLevel = amount + neutral;
        if (!force)
            newMoodLevel = Math.Clamp(amount + neutral,
                component.MoodThresholds[MoodThreshold.Dead],
                component.MoodThresholds[MoodThreshold.Perfect]);

        component.CurrentMoodLevel = newMoodLevel;

        if (TryComp<NetMoodComponent>(uid, out var mood))
        {
            mood.CurrentMoodLevel = component.CurrentMoodLevel;
            mood.NeutralMoodThreshold = component.MoodThresholds.GetValueOrDefault(MoodThreshold.Neutral);
        }

        UpdateCurrentThreshold(uid, component);
    }

    private void UpdateCurrentThreshold(EntityUid uid, MoodComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        var calculatedThreshold = GetMoodThreshold(component);
        if (calculatedThreshold == component.CurrentMoodThreshold)
            return;

        component.CurrentMoodThreshold = calculatedThreshold;

        DoMoodThresholdsEffects(uid, component);
    }

    private void DoMoodThresholdsEffects(EntityUid uid, MoodComponent? component = null, bool force = false)
    {
        if (!Resolve(uid, ref component)
            || component.CurrentMoodThreshold == component.LastThreshold && !force)
            return;

        var modifier = GetMovementThreshold(component.CurrentMoodThreshold);

        // Modify mob stats
        if (modifier != GetMovementThreshold(component.LastThreshold))
        {
            _movementSpeedModifier.RefreshMovementSpeedModifiers(uid);
            SetCritThreshold(uid, component, modifier);
            RefreshShaders(uid, modifier);
        }

        // Modify interface
        if (component.MoodThresholdsAlerts.TryGetValue(component.CurrentMoodThreshold, out var alertId))
            _alerts.ShowAlert(uid, alertId);
        else
            _alerts.ClearAlertCategory(uid, AlertCategory.Mood);

        component.LastThreshold = component.CurrentMoodThreshold;
    }

    private void RefreshShaders(EntityUid uid, int modifier)
    {
        if (modifier == -1)
            EnsureComp<SaturationScaleOverlayComponent>(uid);
        else
            RemComp<SaturationScaleOverlayComponent>(uid);
    }

    private void SetCritThreshold(EntityUid uid, MoodComponent component, int modifier)
    {
        if (!_config.GetCVar(CCVars.MoodModifiesThresholds)
            || !TryComp<MobThresholdsComponent>(uid, out var mobThresholds)
            || !_mobThreshold.TryGetThresholdForState(uid, MobState.Critical, out var key))
            return;

        var newKey = modifier switch
        {
            1 => FixedPoint2.New(key.Value.Float() * component.IncreaseCritThreshold),
            -1 => FixedPoint2.New(key.Value.Float() * component.DecreaseCritThreshold),
            _ => component.CritThresholdBeforeModify
        };

        component.CritThresholdBeforeModify = key.Value;
        _mobThreshold.SetMobStateThreshold(uid, newKey, MobState.Critical, mobThresholds);
    }

    private MoodThreshold GetMoodThreshold(MoodComponent component, float? moodLevel = null)
    {
        moodLevel ??= component.CurrentMoodLevel;
        var result = MoodThreshold.Dead;
        var value = component.MoodThresholds[MoodThreshold.Perfect];

        foreach (var threshold in component.MoodThresholds)
            if (threshold.Value <= value && threshold.Value >= moodLevel)
            {
                result = threshold.Key;
                value = threshold.Value;
            }

        return result;
    }

    private int GetMovementThreshold(MoodThreshold threshold)
    {
        return threshold switch
        {
            >= MoodThreshold.Good => 1,
            <= MoodThreshold.Meh => -1,
            _ => 0
        };
    }

    private void OnDamageChange(EntityUid uid, MoodComponent component, DamageChangedEvent args)
    {
        if (!_mobThreshold.TryGetPercentageForState(uid, MobState.Critical, args.Damageable.TotalDamage, out var damage))
            return;

        var protoId = "HealthNoDamage";
        var value = component.HealthMoodEffectsThresholds["HealthNoDamage"];

        foreach (var threshold in component.HealthMoodEffectsThresholds)
            if (threshold.Value <= damage && threshold.Value >= value)
            {
                protoId = threshold.Key;
                value = threshold.Value;
            }

        var ev = new MoodEffectEvent(protoId);
        RaiseLocalEvent(uid, ev);
    }
}

[UsedImplicitly]
[DataDefinition]
public sealed partial class ShowMoodEffects : IAlertClick
{
    public void AlertClicked(EntityUid uid)
    {
        var entityManager = IoCManager.Resolve<IEntityManager>();
        var prototypeManager = IoCManager.Resolve<IPrototypeManager>();
        var chatManager = IoCManager.Resolve<IChatManager>();
        var playerManager = IoCManager.Resolve<IPlayerManager>();

        if (!entityManager.TryGetComponent<MoodComponent>(uid, out var comp)
            || comp.CurrentMoodThreshold == MoodThreshold.Dead
            || !playerManager.TryGetSessionByEntity(uid, out var session)
            || session == null)
            return;

        var msgStart = Loc.GetString("mood-show-effects-start");
        chatManager.ChatMessageToOne(ChatChannel.Emotes, msgStart, msgStart, EntityUid.Invalid, false,
            session.Channel);

        foreach (var (_, protoId) in comp.CategorisedEffects)
        {
            if (!prototypeManager.TryIndex<MoodEffectPrototype>(protoId, out var proto)
                || proto.Hidden)
                continue;

            SendDescToChat(proto, session);
        }

        foreach (var (protoId, _) in comp.UncategorisedEffects)
        {
            if (!prototypeManager.TryIndex<MoodEffectPrototype>(protoId, out var proto)
                || proto.Hidden)
                continue;

            SendDescToChat(proto, session);
        }
    }

    private void SendDescToChat(MoodEffectPrototype proto, ICommonSession session)
    {
        if (session == null)
            return;

        var chatManager = IoCManager.Resolve<IChatManager>();

        var color = (proto.MoodChange > 0) ? "#008000" : "#BA0000";
        var msg = $"[font size=10][color={color}]{proto.Description}[/color][/font]";

        chatManager.ChatMessageToOne(ChatChannel.Emotes, msg, msg, EntityUid.Invalid, false,
            session.Channel);
    }
}
