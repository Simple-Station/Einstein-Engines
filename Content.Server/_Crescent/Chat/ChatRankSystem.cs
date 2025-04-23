using Content.Server.Chat.Systems;
using Content.Shared.Chat;


namespace Content.Server.Crescent.Chat;
public sealed class ChatRankSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ChatRankComponent, TransformSpeakerNameEvent>(AddRank);
    }

    private void AddRank(EntityUid uid, ChatRankComponent component, TransformSpeakerNameEvent args)
    {
        if (!args.fromRadio)
            return;

        var name = Loc.GetString("rank-ordering", ("rank", Loc.GetString(component.Rank)), ("name", args.VoiceName));

        args.VoiceName = name;
    }
}
