using Content.Server.Chat.Systems;
using Content.Shared.Mind.Components;
using Robust.Shared.Random;
using Content.Server.Speech.Components;
using Content.Shared.Chat;

namespace Content.Server.Speech.Systems;

public sealed class RandomBarkSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ChatSystem _chat = default!;

    private static readonly string[] AddedPunctuation = [".", "...", "!", "..!", "!!"];


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RandomBarkComponent, MapInitEvent>(OnInit);
    }


    private void OnInit(Entity<RandomBarkComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.BarkAccumulator = _random.NextFloat(ent.Comp.MinTime, ent.Comp.MaxTime) * ent.Comp.BarkMultiplier;
        ent.Comp.BarkLocaleCount ??= GetBarkLocaleCount(ent);
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
            if (TryComp<MindContainerComponent>(uid, out var actComp) && actComp.HasMind
                || GetNextBark((uid, barker)) is not { } bark)
                continue;

            _chat.TrySendInGameICMessage(uid, bark, InGameICChatType.Speak, barker.ChatLog ? ChatTransmitRange.Normal : ChatTransmitRange.HideChat);
        }
    }

    /// <summary>
    ///     Tries to get the next bark for the given entity. Returns null if it fails.
    /// </summary>
    public string? GetNextBark(Entity<RandomBarkComponent> ent)
    {
        var count = GetBarkLocaleCount(ent);
        if (count <= 0)
            return null;

        var index = _random.Next(0, count) + 1;
        if (!Loc.TryGetString($"bark-{ent.Comp.BarkType}-{index}", out var bark))
        {
            Log.Error($"Could not find bark with index {index} and type {ent.Comp.BarkType} for entity {ent.Owner}.");
            return null;
        }

        // If the last char of the string is an alphanumeric one, then add a random punctuation mark
        if (bark.Length > 0 && char.IsLetterOrDigit(bark[^1]))
            bark += _random.Pick(AddedPunctuation);

        return bark;
    }

    private int GetBarkLocaleCount(Entity<RandomBarkComponent> ent)
    {
        if (ent.Comp.BarkLocaleCount is { } localeCount)
            return localeCount;

        // All the error logging should cause certain integration tests to fail should someone setup randombark incorrectly
        if (!Loc.TryGetString($"bark-{ent.Comp.BarkType}-count", out var localeCountStr))
        {
            Log.Error($"Entity {ent.Owner} has a bark type {ent.Comp.BarkType} which does not have a respective bark count locale.");
            return 0;
        }

        if (!int.TryParse(localeCountStr, out localeCount) || localeCount < 0)
        {
            Log.Error($"Entity {ent.Owner} has a bark type {ent.Comp.BarkType} whose respective bark count locale is not a valid number.");
            return 0;
        }

        return localeCount;
    }
}
