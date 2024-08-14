using System.Linq;
using Content.Shared.DoAfter;
using Content.Shared.InteractionVerbs.Events;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using static Content.Shared.InteractionVerbs.InteractionPopupPrototype.Prefix;
using static Content.Shared.InteractionVerbs.InteractionVerbPrototype.EffectTargetSpecifier;

namespace Content.Shared.InteractionVerbs;

public abstract class SharedInteractionVerbsSystem : EntitySystem
{
    private readonly InteractionAction.VerbDependencies _verbDependencies = new();
    private List<InteractionVerbPrototype> _globalPrototypes = default!;

    [Dependency] private readonly SharedDoAfterSystem _doAfters = default!;
    [Dependency] private readonly SharedPopupSystem _popups = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        IoCManager.InjectDependencies(_verbDependencies);

        LoadGlobalVerbs();
        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnPrototypesReloaded);

        SubscribeLocalEvent<InteractionVerbsComponent, GetVerbsEvent<InteractionVerb>>(OnGetOthersVerbs);
        SubscribeLocalEvent<OwnInteractionVerbsComponent, GetVerbsEvent<InnateVerb>>(OnGetOwnVerbs);
        SubscribeLocalEvent<InteractionVerbDoAfterEvent>(OnDoAfterFinished);
    }

    private void LoadGlobalVerbs()
    {
        _globalPrototypes = _protoMan.EnumeratePrototypes<InteractionVerbPrototype>()
            .Where(v => v is { Global: true, Abstract: false })
            .ToList();
    }

    #region event handling

    private void OnPrototypesReloaded(PrototypesReloadedEventArgs args)
    {
        if (!args.WasModified<InteractionVerbPrototype>())
            return;

        LoadGlobalVerbs();
    }

    private void OnGetOthersVerbs(Entity<InteractionVerbsComponent> entity, ref GetVerbsEvent<InteractionVerb> args)
    {
        // Global verbs are not added here since OnGetOwnVerbs already adds them
        AddAll(entity.Comp.AllowedVerbs.Select(_protoMan.Index), args, () => new InteractionVerb());
    }

    private void OnGetOwnVerbs(Entity<OwnInteractionVerbsComponent> entity, ref GetVerbsEvent<InnateVerb> args)
    {
        var allVerbs = entity.Comp.AllowedVerbs;

        var getVerbsEv = new GetInteractionVerbsEvent(allVerbs);
        RaiseLocalEvent(entity, ref getVerbsEv);

        // Global verbs are added here because they should be allowed even on entities that do not define any interactions
        AddAll(allVerbs.Select(_protoMan.Index).Union(_globalPrototypes), args, () => new InnateVerb());
    }

    private void OnDoAfterFinished(InteractionVerbDoAfterEvent ev)
    {
        if (ev.Cancelled || ev.Handled || !_protoMan.TryIndex(ev.VerbPrototype, out var proto))
            return;

        PerformVerb(proto, ev.VerbArgs);
        ev.Handled = true;
    }

    #endregion

    #region public api

    /// <summary>
    ///     Starts the verb, checking if it can be performed first, unless forced.
    ///     Upon success, this method will either start a do-after, or pass control to <see cref="PerformVerb"/>.
    /// </summary>
    public bool StartVerb(InteractionVerbPrototype proto, InteractionArgs args, bool force = false)
    {
        if (proto.Action is null)
            return false;

        if (!proto.Action.CanPerform(args, proto, true, _verbDependencies) && !force)
        {
            CreateVerbEffects(proto.EffectFailure, Fail, proto, args);
            return false;
        }

        var attemptEv = new InteractionVerbAttemptEvent(proto, args);
        RaiseLocalEvent(args.User, ref attemptEv);
        RaiseLocalEvent(args.Target, ref attemptEv);

        if (attemptEv.Cancelled)
        {
            CreateVerbEffects(proto.EffectFailure, Fail, proto, args);
            return false;
        }

        if (attemptEv.Handled)
            return true;

        if (proto.Delay <= TimeSpan.Zero)
        {
            PerformVerb(proto, args);
            return true;
        }

        // The pyramid strikes again... god forgive me for this
        var doAfter = new DoAfterArgs(proto.DoAfter)
        {
            User = args.User,
            Target = args.Target,
            EventTarget = EntityUid.Invalid,
            NetUser = GetNetEntity(args.User),
            NetTarget = GetNetEntity(args.Target),
            NetEventTarget = NetEntity.Invalid,
            Broadcast = true,
            BreakOnHandChange = proto.RequiresHands,
            NeedHand = proto.RequiresHands,
            Delay = proto.Delay,
            RequireCanInteract = proto.RequiresCanInteract,
            Event = new InteractionVerbDoAfterEvent(proto.ID, args)
        };

        var isSuccess = _doAfters.TryStartDoAfter(doAfter);
        if (isSuccess)
            CreateVerbEffects(proto.EffectDelayed, Delayed, proto, args);

        return isSuccess;
    }

    /// <summary>
    ///     Performs an additional CanPerform check (unless forced) and then actually performs the action of the verb
    ///     and shows a success/failure popup.
    /// </summary>
    public void PerformVerb(InteractionVerbPrototype proto, InteractionArgs args, bool force = false)
    {
        if (_net.IsClient)
            return; // this leads to issues

        if (!proto.Action!.CanPerform(args, proto, false, _verbDependencies) && !force
            || !proto.Action.Perform(args, proto, _verbDependencies))
        {
            CreateVerbEffects(proto.EffectFailure, Fail, proto, args);
            return;
        }

        CreateVerbEffects(proto.EffectSuccess, Success, proto, args);
    }

    #endregion

    #region private api

    /// <summary>
    ///     Creates verbs for all listed prototypes that match their own requirements. Uses the provided factory to create new verb instances.
    /// </summary>
    // Note: using `where T : Verb, new()` here results in a sandbox violation... Yea we peasants don't get OOP in ss14.
    private void AddAll<T>(IEnumerable<InteractionVerbPrototype> verbs, GetVerbsEvent<T> args, Func<T> factory) where T : Verb
    {
        foreach (var proto in verbs)
        {
            DebugTools.AssertNotEqual(proto.Abstract, true, "Attempted to add a verb with an abstract prototype.");

            var name = proto.Name;
            if (!proto.AllowSelfInteract && args.User == args.Target
                || args.Verbs.Any(v => v.Text == name)
                || !Transform(args.User).Coordinates.TryDistance(EntityManager, Transform(args.Target).Coordinates, out var distance)
            )
                continue;

            var isInvalid = proto.RequiresHands && args.Hands is null
                || proto.RequiresCanInteract && !args.CanInteract
                || !proto.Range.IsInRange(distance);

            var verbArgs = InteractionArgs.From(args);
            var isRequirementMet = proto.Requirement?.IsMet(verbArgs, proto, _verbDependencies) != false;
            if (!isRequirementMet && proto.HideByRequirement)
                continue;

            var isAllowed = proto.Action?.IsAllowed(verbArgs, proto, _verbDependencies) == true;
            if (!isAllowed && proto.HideWhenInvalid)
                continue;

            var verb = factory.Invoke();
            CopyVerbData(proto, verb);
            verb.Disabled = isInvalid || !isRequirementMet || !isAllowed;
            if (!verb.Disabled)
                verb.Act = () => StartVerb(proto, verbArgs);

            args.Verbs.Add(verb);
        }
    }

    private void CopyVerbData(InteractionVerbPrototype proto, Verb verb)
    {
        verb.Text = proto.Name;
        verb.Message = proto.Description;
        verb.DoContactInteraction = proto.DoContactInteraction;
        verb.Priority = proto.Priority;
        verb.Icon = proto.Icon;
        verb.Category = VerbCategory.Interaction;
    }

    private void CreateVerbEffects(InteractionVerbPrototype.EffectSpecifier? specifier, InteractionPopupPrototype.Prefix prefix, InteractionVerbPrototype proto, InteractionArgs args)
    {
        // Not doing effects on client because it causes issues
        if (specifier is null || _net.IsClient)
            return;

        var (user, target, used) = (args.User, args.Target, args.Used);

        // Effect targets for different players
        var userTarget = specifier.EffectTarget is User or TargetThenUser ? user : target;
        var targetTarget = specifier.EffectTarget is User or TargetThenUser ? user : target;
        var othersTarget = specifier.EffectTarget is not User ? target : user;
        var othersFilter = Filter.Pvs(othersTarget).RemoveWhereAttachedEntity(ent => ent == user || ent == target);

        // Popups
        if (_protoMan.TryIndex(specifier.Popup, out var popup))
        {
            var locPrefix = $"interaction-{proto.ID}-{prefix.ToString().ToLower()}";

            (string, object)[] localeArgs = [("user", user), ("target", target), ("used", used ?? EntityUid.Invalid), ("selfTarget", user == target)];

            // User popup
            var userSuffix = popup.SelfSuffix ?? popup.OthersSuffix;
            if (userSuffix is not null)
                PopupEffects(Loc.GetString($"{locPrefix}-{userSuffix}-popup", localeArgs), userTarget, Filter.Entities(user), false, popup);

            // Target popup
            var targetSuffix = popup.TargetSuffix ?? popup.OthersSuffix;
            if (targetSuffix is not null && user != target)
                PopupEffects(Loc.GetString($"{locPrefix}-{targetSuffix}-popup", localeArgs), targetTarget, Filter.Entities(target), false, popup);

            // Others popup
            var othersSuffix = popup.OthersSuffix;
            if (othersSuffix is not null)
                PopupEffects(Loc.GetString($"{locPrefix}-{othersSuffix}-popup", localeArgs), othersTarget, othersFilter, true, popup);
        }

        // Sounds
        if (specifier.Sound is { } sound)
        {
            // TODO we have a choice between having an accurate sound source or saving on an entity spawn...
            _audio.PlayEntity(sound, Filter.Entities(user, target), target, false, specifier.SoundParams);

            if (specifier.SoundPerceivedByOthers)
                _audio.PlayEntity(sound, othersFilter, othersTarget, false, specifier.SoundParams);
        }
    }

    private void PopupEffects(string message, EntityUid target, Filter filter, bool recordReplay, InteractionPopupPrototype popup)
    {
        // Sending a chat message will result in a popup anyway
        // TODO this needs to be fixed probably. Popups and chat messages should be independent.
        if (popup.LogPopup)
            SendChatLog(message, target, filter, popup);
        else
            _popups.PopupEntity(message, target, filter, recordReplay, popup.PopupType);
    }

    protected virtual void SendChatLog(string message, EntityUid source, Filter filter, InteractionPopupPrototype popup)
    {
    }

    #endregion
}
