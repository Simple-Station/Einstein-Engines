using Content.Shared.Abilities.Psionics;
using Content.Shared.StatusEffect;
using Content.Shared.Psionics.Glimmer;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Damage.Events;
using Content.Shared.CCVar;
using Content.Server.Abilities.Psionics;
using Content.Server.Electrocution;
using Content.Server.NPC.Components;
using Content.Server.NPC.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Random;
using Content.Shared.Popups;
using Content.Shared.Chat;
using Robust.Server.Player;
using Content.Server.Chat.Managers;
using Robust.Shared.Prototypes;
using Content.Shared.Psionics;

namespace Content.Server.Psionics;

public sealed class PsionicsSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly PsionicAbilitiesSystem _psionicAbilitiesSystem = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly ElectrocutionSystem _electrocutionSystem = default!;
    [Dependency] private readonly MindSwapPowerSystem _mindSwapPowerSystem = default!;
    [Dependency] private readonly GlimmerSystem _glimmerSystem = default!;
    [Dependency] private readonly NpcFactionSystem _npcFactonSystem = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popups = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;

    private const string BaselineAmplification = "Baseline Amplification";
    private const string BaselineDampening = "Baseline Dampening";

    // Yes these are a mirror of what's normally default datafields on the PsionicPowerPrototype.
    // We haven't generated a prototype yet, and I'm not going to duplicate them on the PsionicComponent.
    private const string PsionicRollFailedMessage = "psionic-roll-failed";
    private const string PsionicRollFailedColor = "#8A00C2";
    private const int PsionicRollFailedFontSize = 12;
    private const ChatChannel PsionicRollFailedChatChannel = ChatChannel.Emotes;

    /// <summary>
    ///     Unfortunately, since spawning as a normal role and anything else is so different,
    ///     this is the only way to unify them, for now at least.
    /// </summary>
    Queue<(PsionicComponent component, EntityUid uid)> _rollers = new();
    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        foreach (var roller in _rollers)
            RollPsionics(roller.uid, roller.component, true);
        _rollers.Clear();
    }
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PsionicComponent, MapInitEvent>(OnStartup);
        SubscribeLocalEvent<AntiPsionicWeaponComponent, MeleeHitEvent>(OnMeleeHit);
        SubscribeLocalEvent<AntiPsionicWeaponComponent, TakeStaminaDamageEvent>(OnStamHit);

        SubscribeLocalEvent<PsionicComponent, ComponentStartup>(OnInit);
        SubscribeLocalEvent<PsionicComponent, ComponentRemove>(OnRemove);
    }

    private void OnStartup(EntityUid uid, PsionicComponent component, MapInitEvent args)
    {
        if (!component.Removable
            || !component.CanReroll)
            return;

        CheckPowerCost(uid, component);
        _rollers.Enqueue((component, uid));
    }

    /// <summary>
    ///     On MapInit, PsionicComponent isn't going to contain any powers.
    ///     So before we send a Latent Psychic into the roundstart roll queue, we need to calculate their power cost in advance.
    /// </summary>
    private void CheckPowerCost(EntityUid uid, PsionicComponent component)
    {
        if (!TryComp<InnatePsionicPowersComponent>(uid, out var innate))
            return;

        var powerCount = 0;
        foreach (var powerId in innate.PowersToAdd)
            if (_protoMan.TryIndex(powerId, out var power))
                powerCount += power.PowerSlotCost;

        component.NextPowerCost = 100 * MathF.Pow(2, powerCount);
    }

    private void OnMeleeHit(EntityUid uid, AntiPsionicWeaponComponent component, MeleeHitEvent args)
    {
        foreach (var entity in args.HitEntities)
            CheckAntiPsionic(entity, component, args);
    }

    private void CheckAntiPsionic(EntityUid entity, AntiPsionicWeaponComponent component, MeleeHitEvent args)
    {
        if (HasComp<PsionicComponent>(entity))
        {
            _audio.PlayPvs("/Audio/Effects/lightburn.ogg", entity);
            args.ModifiersList.Add(component.Modifiers);

            if (!_random.Prob(component.DisableChance))
                return;

            _statusEffects.TryAddStatusEffect(entity, component.DisableStatus, TimeSpan.FromSeconds(component.DisableDuration), true, component.DisableStatus);
        }

        if (TryComp<MindSwappedComponent>(entity, out var swapped))
            _mindSwapPowerSystem.Swap(entity, swapped.OriginalEntity, true);

        if (!component.Punish
            || HasComp<PsionicComponent>(entity)
            || !_random.Prob(component.PunishChances))
            return;

        _electrocutionSystem.TryDoElectrocution(args.User, null, component.PunishSelfDamage, TimeSpan.FromSeconds(component.PunishStunDuration), false);
    }

    private void OnInit(EntityUid uid, PsionicComponent component, ComponentStartup args)
    {
        component.AmplificationSources.Add(BaselineAmplification, _random.NextFloat(component.BaselineAmplification.Item1, component.BaselineAmplification.Item2));
        component.DampeningSources.Add(BaselineDampening, _random.NextFloat(component.BaselineDampening.Item1, component.BaselineDampening.Item2));

        if (!component.Removable
            || !TryComp<NpcFactionMemberComponent>(uid, out var factions)
            || _npcFactonSystem.ContainsFaction(uid, "GlimmerMonster", factions))
            return;

        _npcFactonSystem.AddFaction(uid, "PsionicInterloper");
    }

    private void OnRemove(EntityUid uid, PsionicComponent component, ComponentRemove args)
    {
        if (!HasComp<NpcFactionMemberComponent>(uid))
            return;

        _npcFactonSystem.RemoveFaction(uid, "PsionicInterloper");
    }

    private void OnStamHit(EntityUid uid, AntiPsionicWeaponComponent component, TakeStaminaDamageEvent args)
    {
        if (!HasComp<PsionicComponent>(args.Target))
            return;

        args.FlatModifier += component.PsychicStaminaDamage;
    }

    /// <summary>
    ///     Now we handle Potentia calculations, the more powers you have, the harder it is to obtain psionics, but the content of your roll carries over to the next roll.
    ///     Your first power costs 100(2^0 is always 1), your second power costs 200, your 3rd power costs 400, and so on. This also considers people with roundstart powers.
    ///     Such that a Mystagogue(who has 3 powers at roundstart) needs 800 Potentia to gain his 4th power.
    /// </summary>
    /// <remarks>
    ///     This exponential cost is mainly done to prevent stations from becoming "Space Hogwarts",
    ///     which was a common complaint with Psionic Refactor opening up the opportunity for people to have multiple powers.
    /// </remarks>
    private bool HandlePotentiaCalculations(EntityUid uid, PsionicComponent component, float psionicChance)
    {
        component.Potentia += _random.NextFloat(0 + psionicChance, 100 + psionicChance);

        if (component.Potentia < component.NextPowerCost)
            return false;

        component.Potentia -= component.NextPowerCost;
        _psionicAbilitiesSystem.AddPsionics(uid);
        component.NextPowerCost = 100 * MathF.Pow(2, component.PowerSlotsTaken);
        return true;
    }

    /// <summary>
    ///     Provide the player with feedback about their roll failure, so they don't just think nothing happened.
    ///     TODO: Add an audio cue to this and other areas of psionic player feedback.
    /// </summary>
    private void HandleRollFeedback(EntityUid uid)
    {
        if (!_playerManager.TryGetSessionByEntity(uid, out var session)
            || !Loc.TryGetString(PsionicRollFailedMessage, out var rollFailedMessage))
            return;

        _popups.PopupEntity(rollFailedMessage, uid, uid, PopupType.MediumCaution);

        // Popups only last a few seconds, and are easily ignored.
        // So we also put a message in chat to make it harder to miss.
        var feedbackMessage = $"[font size={PsionicRollFailedFontSize}][color={PsionicRollFailedColor}]{rollFailedMessage}[/color][/font]";
        _chatManager.ChatMessageToOne(
            PsionicRollFailedChatChannel,
            feedbackMessage,
            feedbackMessage,
            EntityUid.Invalid,
            false,
            session.Channel);
    }

    /// <summary>
    ///     This function attempts to generate a psionic power by incrementing a Psion's Potentia stat by a random amount, then checking if it beats a certain threshold.
    ///     Please consider going through RerollPsionics or PsionicAbilitiesSystem.InitializePsionicPower instead of this function, particularly if you don't have a good reason to call this directly.
    /// </summary>
    public void RollPsionics(EntityUid uid, PsionicComponent component, bool applyGlimmer = true, float rollEventMultiplier = 1f)
    {
        if (!_cfg.GetCVar(CCVars.PsionicRollsEnabled)
            || !component.Removable)
            return;

        // Calculate the initial odds based on the innate potential
        var baselineChance = component.Chance
            * component.PowerRollMultiplier
            + component.PowerRollFlatBonus
            + _random.NextFloat(0, 100);

        // Increase the initial odds based on Glimmer.
        // TODO: Change this equation when I do my Glimmer Refactor
        baselineChance += applyGlimmer
            ? (float) _glimmerSystem.Glimmer / 1000 //Convert from Glimmer to %chance
            : 0;

        // Certain sources of power rolls provide their own multiplier.
        baselineChance *= rollEventMultiplier;

        // Ask if the Roller has any other effects to contribute, such as Traits.
        var ev = new OnRollPsionicsEvent(uid, baselineChance);
        RaiseLocalEvent(uid, ref ev);

        if (HandlePotentiaCalculations(uid, component, ev.BaselineChance))
            return;

        HandleRollFeedback(uid);
    }

    /// <summary>
    ///     Each person has a single free reroll for their Psionics, which certain conditions can restore.
    ///     This function attempts to "Spend" a reroll, if one is available.
    /// </summary>
    public void RerollPsionics(EntityUid uid, PsionicComponent? psionic = null, float bonusMuliplier = 1f)
    {
        if (!Resolve(uid, ref psionic, false)
            || !psionic.Removable
            || !psionic.CanReroll)
            return;

        RollPsionics(uid, psionic, true, bonusMuliplier);
        psionic.CanReroll = false;
    }
}
