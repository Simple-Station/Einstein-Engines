using Content.Server.Chat.Systems;
using Content.Shared.ActionBlocker;
using Content.Shared.Chat;
using Content.Shared.InteractionVerbs;

namespace Content.Server.InteractionVerbs.Actions;

/// <summary>
///     Makes the target or the user to send a chat message. <br/><br/>
///
///     Messages are locale-based, their keys follow the form of "interaction-[verb id]-[message loc prefix]-[index]".
///     The index parameter is a random integer from 1 to <see cref="NumMessages"/>. <br/><br/>
///
///     Similarly to interaction verb locales, {$user} and {$target} arguments are passed to the locales retrieved by this action.
/// </summary>
public sealed partial class ChatMessageAction : InteractionVerbAction
{
    [DataField]
    public string MessageLocPrefix = "message";

    /// <summary>
    ///     Number of messages in the dataset.
    /// </summary>
    [DataField]
    public int NumMessages = 1;

    [DataField]
    public InGameICChatType ChatType = InGameICChatType.Speak;

    /// <summary>
    ///     If true, makes the target speak. Otherwise, makes the user speak.
    /// </summary>
    [DataField]
    public bool TargetIsSource = true;

    private EntityUid GetSpeaker(EntityUid user, EntityUid target)
    {
        return TargetIsSource ? target : user;
    }

    public override bool CanPerform(EntityUid user, EntityUid target, bool beforeDelay, InteractionVerbPrototype proto, VerbDependencies deps)
    {
        return deps.EntMan.System<ActionBlockerSystem>().CanSpeak(GetSpeaker(user, target));
    }

    public override void Perform(EntityUid user, EntityUid target, InteractionVerbPrototype proto, VerbDependencies deps)
    {
        var index = NumMessages <= 1 ? 1 : deps.Random.Next(1, NumMessages + 1);
        var locString = $"interaction-{proto.ID}-{MessageLocPrefix}-{index}";

        if (!Loc.TryGetString(locString, out var message, ("user", user), ("target", target)))
        {
            Logger.GetSawmill("action.chat_message").Error($"No chat message found for interaction {proto.ID}! Loc string: {locString}.");
            return;
        }

        var speaker = GetSpeaker(user, target);
        deps.EntMan.System<ChatSystem>().TrySendInGameICMessage(speaker, message, ChatType, false);
    }
}
