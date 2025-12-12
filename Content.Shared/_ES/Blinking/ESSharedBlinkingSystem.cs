using Content.Shared._ES.Blinking.Components;
using Content.Shared.Bed.Sleep;
using Content.Shared.Mobs;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared._ES.Blinking;

public abstract class ESSharedBlinkingSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] protected readonly SharedAppearanceSystem Appearance = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<ESBlinkerComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<ESBlinkerComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<ESBlinkerComponent, SleepStateChangedEvent>(OnSleepStateChanged);
    }

    private void OnMapInit(Entity<ESBlinkerComponent> ent, ref MapInitEvent args)
    {
        ResetBlink(ent);
    }

    private void OnMobStateChanged(Entity<ESBlinkerComponent> ent, ref MobStateChangedEvent args)
    {
        SetEnabled(ent.AsNullable(), args.NewMobState != MobState.Dead);
    }

    private void OnSleepStateChanged(Entity<ESBlinkerComponent> ent, ref SleepStateChangedEvent args)
    {
        Appearance.SetData(ent.Owner, ESBlinkVisuals.EyesClosed, args.FellAsleep);
    }

    private void ResetBlink(Entity<ESBlinkerComponent> ent)
    {
        ent.Comp.NextBlinkTime = _timing.CurTime + _random.Next(ent.Comp.MinBlinkDelay, ent.Comp.MaxBlinkDelay);
        Dirty(ent);
    }

    public void SetEnabled(Entity<ESBlinkerComponent?> ent, bool enabled)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return;

        if (ent.Comp.Enabled == enabled)
            return;

        ent.Comp.Enabled = enabled;
        Dirty(ent);

        if (enabled)
            ResetBlink((ent, ent.Comp));
    }

    public virtual void Blink(Entity<ESBlinkerComponent> ent)
    {
        ResetBlink(ent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ESBlinkerComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!comp.Enabled)
                continue;
            if (_timing.CurTime < comp.NextBlinkTime)
                continue;
            Blink((uid, comp));
        }
    }
}
