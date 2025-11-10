using Content.Shared.Chat;
using Content.Shared.Interaction.Events;
using Content.Shared._Crescent.BookOfKane.Components;


namespace Content.Shared._Crescent.BookOfKane;


public sealed class BookOfKaneSystem : EntitySystem
{
    private const string KaneSpeechBaseKey = "kane-speech-";

    [Dependency] private readonly SharedChatSystem _chat = default!;

    private List<string> _speechLocalizationKeys = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BookOfKaneComponent, UseInHandEvent>(OnUseInHand);

        var i = 1;
        while (Loc.HasString($"{KaneSpeechBaseKey}{i}"))
        {
            _speechLocalizationKeys.Add($"{KaneSpeechBaseKey}{i}");
            i++;
        }
    }

    private void OnUseInHand(Entity<BookOfKaneComponent> ent, ref UseInHandEvent args)
    {
        if (_speechLocalizationKeys == null || _speechLocalizationKeys.Count == 0)
            return;

        var message = _speechLocalizationKeys[System.Random.Shared.Next(_speechLocalizationKeys.Count)];
        _chat.TrySendInGameICMessage(
            args.User,
            Loc.GetString(message),
            InGameICChatType.Speak,
            hideChat: false,
            hideLog: false);

        args.Handled = true;
    }
}
