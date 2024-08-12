using System.Linq;
using Content.Shared.DoAfter;using Content.Shared.InteractionVerbs;
using Content.Shared.InteractionVerbs.Events;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using static Content.Shared.InteractionVerbs.InteractionVerbPrototype.PopupTargetSpecifier;
using PopupSpecifier = Content.Shared.InteractionVerbs.InteractionVerbPrototype.PopupSpecifier;

namespace Content.Shared.InteractionVerbs;

public abstract class SharedInteractionVerbsSystem : EntitySystem
{
    private readonly InteractionAction.VerbDependencies _verbDependencies = new();
    private List<InteractionVerbPrototype> _globalPrototypes = default!;

    [Dependency] private readonly SharedDoAfterSystem _doAfters = default!;
    [Dependency] private readonly SharedPopupSystem _popups = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly INetManager _net = default!;

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

        PerformVerb(proto, ev.User, ev.Target!.Value);
        ev.Handled = true;
    }

    #endregion

    #region public api

    /// <summary>
    ///     Starts the verb, checking if it can be performed first, unless forced.
    ///     Upon success, this method will either start a do-after, or pass control to <see cref="PerformVerb"/>.
    /// </summary>
    public bool StartVerb(InteractionVerbPrototype proto, EntityUid user, EntityUid target, bool force = false)
    {
        if (proto.Action is null)
            return false;

        if (!proto.Action.CanPerform(user, target, true, proto, _verbDependencies) && !force)
        {
            ShowVerbPopups(proto.FailurePopup, proto, user, target);
            return false;
        }

        if (proto.Delay <= TimeSpan.Zero)
        {
            PerformVerb(proto, user, target);
            return true;
        }

        // The pyramid strikes again... god forgive me for this
        var doAfter = new DoAfterArgs(proto.DoAfter)
        {
            User = user,
            Target = target,
            EventTarget = EntityUid.Invalid,
            NetUser = GetNetEntity(user),
            NetTarget = GetNetEntity(target),
            NetEventTarget = NetEntity.Invalid,
            Broadcast = true,
            BreakOnHandChange = proto.RequiresHands,
            NeedHand = proto.RequiresHands,
            Delay = proto.Delay,
            RequireCanInteract = proto.RequiresCanInteract,
            Event = new InteractionVerbDoAfterEvent(proto.ID)
        };

        var isSuccess = _doAfters.TryStartDoAfter(doAfter);
        if (isSuccess)
            ShowVerbPopups(proto.DelayedPopup, proto, user, target);

        return isSuccess;
    }

    /// <summary>
    ///     Performs an additional CanPerform check (unless forced) and then actually performs the action of the verb
    ///     and shows a success popup.
    /// </summary>
    public void PerformVerb(InteractionVerbPrototype proto, EntityUid user, EntityUid target, bool force = false)
    {
        if (_net.IsClient)
            return; // guh

        if (!proto.Action!.CanPerform(user, target, false, proto, _verbDependencies) && !force)
        {
            ShowVerbPopups(proto.FailurePopup, proto, user, target);
            return;
        }

        proto.Action.Perform(user, target, proto, _verbDependencies);
        ShowVerbPopups(proto.SuccessPopup, proto, user, target);
    }

    #endregion

    #region private api

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

            var isRequirementMet = proto.Requirement?.IsMet(args.User, args.Target, proto, args.CanAccess, args.CanInteract, _verbDependencies) != false;
            if (!isRequirementMet && proto.HideByRequirement)
                continue;

            var isAllowed = proto.Action?.IsAllowed(args.User, args.Target, proto, args.CanAccess, args.CanInteract, _verbDependencies) == true;
            if (!isAllowed && proto.HideWhenInvalid)
                continue;

            var verb = factory.Invoke();
            CopyVerbData(proto, verb, args.User, args.Target);

            verb.Disabled = isInvalid || !isRequirementMet || !isAllowed;
            if (verb.Disabled)
                verb.Act = null;

            args.Verbs.Add(verb);
        }
    }

    private void CopyVerbData(InteractionVerbPrototype proto, Verb verb, EntityUid user, EntityUid target)
    {
        verb.Text = proto.Name;
        verb.Message = proto.Description;
        verb.DoContactInteraction = proto.DoContactInteraction;
        verb.Priority = proto.Priority;
        verb.Icon = proto.Icon;
        verb.Category = VerbCategory.Interaction;
        verb.Act = () => StartVerb(proto, user, target);
    }

    private void ShowVerbPopups(PopupSpecifier? specifier, InteractionVerbPrototype proto, EntityUid user, EntityUid target)
    {
        // Not showing popups on client because it causes issues.
        if (specifier is not { } popup || _net.IsClient)
            return;

        var locPrefix = $"interaction-{proto.ID}-{popup.PopupPrefix}";

        (string, object)[] localeArgs = [("user", user), ("target", target)];

        // User popup
        var userSuffix = popup.SelfSuffix ?? popup.OthersSuffix;
        var userTarget = popup.PopupTarget is User or TargetThenUser ? user : target;
        if (userSuffix is not null)
            PopupAndLog(Loc.GetString($"{locPrefix}-{userSuffix}-popup", localeArgs), userTarget, Filter.Entities(user), false, popup);

        // Target popup
        if (target != user)
        {
            var targetSuffix = popup.TargetSuffix ?? popup.OthersSuffix;
            var targetTarget = popup.PopupTarget is not User ? target : user;
            if (targetSuffix is not null)
                PopupAndLog(Loc.GetString($"{locPrefix}-{targetSuffix}-popup", localeArgs), targetTarget, Filter.Entities(target), false, popup);
        }

        // Others popup
        var othersSuffix = popup.OthersSuffix;
        var othersTarget = popup.PopupTarget is User or TargetThenUser ? user : target;
        var othersFilter = Filter.Pvs(othersTarget).RemoveWhereAttachedEntity(ent => ent == user || ent == target);
        if (othersSuffix is not null)
            PopupAndLog(Loc.GetString($"{locPrefix}-{othersSuffix}-popup", localeArgs), othersTarget, othersFilter, true, popup);
    }

    private void PopupAndLog(string message, EntityUid target, Filter filter, bool recordReplay, PopupSpecifier specifier)
    {
        // Sending a chat message will result in a popup anyway
        // TODO this needs to be fixed probably. Popups and chat messages should be independent.
        if (specifier.LogPopup)
            SendChatLog(message, target, filter, specifier);
        else
            _popups.PopupEntity(message, target, filter, recordReplay, specifier.PopupType);
    }

    protected virtual void SendChatLog(string message, EntityUid source, Filter filter, PopupSpecifier specifier)
    {
    }

    #endregion
}
