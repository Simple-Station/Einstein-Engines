using Content.Server.Chat.Systems;
using Content.Shared.Mind.Components;
using Robust.Shared.Random;
using Content.Server.Speech.Components;

namespace Content.Server.Speech.Systems;

public sealed class RandomBarkSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly EntityManager _entity = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RandomBarkComponent, ComponentInit>(OnInit);
    }


    private void OnInit(EntityUid uid, RandomBarkComponent barker, ComponentInit args)
    {
        barker.BarkAccumulator = _random.NextFloat(barker.MinTime, barker.MaxTime) * barker.BarkMultiplier;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<RandomBarkComponent>();
        while (query.MoveNext(out var uid, out var barker))
        {
            barker.BarkAccumulator -= frameTime;
            if (barker.BarkAccumulator > 0)
                continue;

            barker.BarkAccumulator = _random.NextFloat(barker.MinTime, barker.MaxTime) * barker.BarkMultiplier;
            if (_entity.TryGetComponent<MindContainerComponent>(uid, out var actComp) &&
                actComp.HasMind)
                continue;

            _chat.TrySendInGameICMessage(uid, _random.Pick(barker.Barks), InGameICChatType.Speak, barker.ChatLog ? ChatTransmitRange.Normal : ChatTransmitRange.HideChat);
        }
    }
}
