using System.Linq;
using Content.Server.Chat.Managers;
using Content.Shared.InteractionVerbs;
using Content.Shared.Popups;
using Robust.Shared.Player;

namespace Content.Server.InteractionVerbs;

public sealed class InteractionVerbsSystem : SharedInteractionVerbsSystem
{
    [Dependency] private readonly IChatManager _chatManager = default!;

    protected override void SendChatLog(string message, EntityUid source, Filter filter, InteractionVerbPrototype.PopupSpecifier specifier)
    {
        if (filter.Count <= 0)
            return;

        var color = specifier.LogColor ?? InferColor(specifier.PopupType);
        var wrappedMessage = message; // TODO: custom chat wraps maybe?

        if (filter.Count == 1)
            _chatManager.ChatMessageToOne(specifier.LogChannel, message, wrappedMessage, source, false, filter.Recipients.First().Channel, color);
        else
            _chatManager.ChatMessageToManyFiltered(filter, specifier.LogChannel, message, wrappedMessage, source, false, false, color);
    }

    private Color InferColor(PopupType popup) => popup switch
    {
        // These are all hardcoded on client-side, so we have to improvise
        PopupType.LargeCaution or PopupType.MediumCaution or PopupType.SmallCaution => Color.Red,
        PopupType.Medium or PopupType.Small => Color.LightGray,
        _ => Color.White
    };
}
