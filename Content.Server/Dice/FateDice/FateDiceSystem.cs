using System.Linq;
using Content.Server.Ghost;
using Content.Server.Light.Components;
using Content.Server.Xenoarchaeology.XenoArtifacts.Events;
using Content.Shared.Coordinates;
using Content.Shared.Dice;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Item;
using Content.Shared.Xenoarchaeology.XenoArtifacts;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Timing;

namespace Content.Server.Dice.FateDice;

public sealed class FateDiceSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IComponentFactory _componentFactory = default!;
    [Dependency] private readonly ISerializationManager _serialization = default!;
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly GhostSystem _ghost = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FateDiceComponent, DiceRollEvent>(OnDiceRoll);
        SubscribeLocalEvent<FateDiceComponent, PickupAttemptEvent>(OnPickupAttempt);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = _entity.EntityQueryEnumerator<FateDiceComponent>();
        while (query.MoveNext(out var uid, out var dice))
        {
            if (dice.ActTime <= _timing.CurTime)
                ActivateEffect(uid, dice);
            if (dice.DelTime <= _timing.CurTime)
                RemoveDice(uid, dice);
        }
    }
    private void RemoveDice(EntityUid uid, FateDiceComponent dice)
    {
        // TODO: Add some guards to prevent spawning of invalid items
        var replaced = SpawnAtPosition(dice.ToReplaceWhenUsedUp, uid.ToCoordinates());
        _audio.PlayPvs(dice.DeletedSound, replaced);

        QueueDel(uid);
    }
    private void ActivateEffect(EntityUid uid, FateDiceComponent dice)
    {
        // do ghost boo
        var lights = GetEntityQuery<PoweredLightComponent>();
        foreach (var light in _lookup.GetEntitiesInRange(uid, dice.BooRadius, LookupFlags.StaticSundries))
        {
            if (!lights.HasComponent(light))
                continue;

            if (!_random.Prob(0.75f))
                continue;

            _ghost.DoGhostBooEvent(light);
        }

        var ev = new ArtifactActivatedEvent
        {
            Activator = dice.LastUser
        };
        RaiseLocalEvent(uid, ev, true);

        dice.ActTime = null;

        RemoveEffecComponentsFromDice(uid, dice);
    }


    // This is unshamefully yanked from EnterNode function from ArtifactSystem
    private void AddEffectComponetsToDice(EntityUid uid, FateDiceComponent dice)
    {
        if (dice.LastRolledNumber is null)
            return;

        var effectProto = _prototype.Index<ArtifactEffectPrototype>(dice.Effects[(int)dice.LastRolledNumber - 1]);
        foreach (var (name, entry) in effectProto.Components.Concat(effectProto.PermanentComponents))
        {
            var reg = _componentFactory.GetRegistration(name);

            if (EntityManager.HasComponent(uid, reg.Type))
            {
                // Don't re-add permanent components unless this is the first time you've entered this node
                if (effectProto.PermanentComponents.ContainsKey(name))
                    continue;

                EntityManager.RemoveComponent(uid, reg.Type);
            }

            var comp = (Component) _componentFactory.GetComponent(reg);
            comp.Owner = uid;

            var temp = (object) comp;
            _serialization.CopyTo(entry.Component, ref temp);
            EntityManager.RemoveComponent(uid, temp!.GetType());
            EntityManager.AddComponent(uid, (Component) temp!);
        }
    }

    private void RemoveEffecComponentsFromDice(EntityUid uid, FateDiceComponent dice)
    {
        if (dice.LastRolledNumber is null)
            return;

        var effect = _prototype.Index<ArtifactEffectPrototype>(dice.Effects[(int) dice.LastRolledNumber - 1]);

        var entityPrototype = MetaData(uid).EntityPrototype;
        var toRemove = effect.Components.Keys;

        foreach (var name in toRemove)
        {
            // if the entity prototype contained the component originally
            if (entityPrototype?.Components.TryGetComponent(name, out var entry) ?? false)
            {
                var comp = (Component) _componentFactory.GetComponent(name);
                comp.Owner = uid;
                var temp = (object) comp;
                _serialization.CopyTo(entry, ref temp);
                EntityManager.RemoveComponent(uid, temp!.GetType());
                EntityManager.AddComponent(uid, (Component) temp);
                continue;
            }

            EntityManager.RemoveComponentDeferred(uid, _componentFactory.GetRegistration(name).Type);
        }
    }


    // basically select all components from the effect prototype and apply them directly to the dice
    public void OnDiceRoll(EntityUid uid, FateDiceComponent fateDice, DiceRollEvent args)
    {
        if (fateDice.RemainingUses <= 0)
            return;

        fateDice.ActTime = _timing.CurTime + TimeSpan.FromSeconds(fateDice.TimeToActivate);

        fateDice.LastRolledNumber = args.RolledNumber;

        AddEffectComponetsToDice(uid, fateDice);

        // This is needed for some artifact effects properly work.
        RaiseLocalEvent(uid, new ArtifactNodeEnteredEvent(_random.Next()));

        // TODO: Drop dice on the ground if it's in a mob's container.

        fateDice.RemainingUses--;
        if (fateDice.RemainingUses <= 0)
            fateDice.DelTime = _timing.CurTime + TimeSpan.FromSeconds(fateDice.TimeToDelete);
    }

    public void OnPickupAttempt(EntityUid uid, FateDiceComponent fateDice, PickupAttemptEvent args)
    {
        if (fateDice.RemainingUses <= 0)
        {
            args.Cancel();
            return;
        }
        fateDice.LastUser = args.User;
    }
}
