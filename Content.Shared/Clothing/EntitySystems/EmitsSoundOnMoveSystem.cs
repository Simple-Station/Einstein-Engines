using System.Numerics;
using Content.Shared.Clothing.Components;
using Content.Shared.Gravity;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Components;
using Robust.Shared.Timing;

namespace Content.Shared.Clothing.Systems;

public sealed class EmitsSoundOnMoveSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedMapSystem _grid = default!;
    [Dependency] private readonly SharedGravitySystem _gravity = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private EntityQuery<InputMoverComponent> _moverQuery;
    private EntityQuery<PhysicsComponent> _physicsQuery;
    private EntityQuery<ClothingComponent> _clothingQuery;

    public override void Initialize()
    {
        _moverQuery = GetEntityQuery<InputMoverComponent>();
        _physicsQuery = GetEntityQuery<PhysicsComponent>();
        _clothingQuery = GetEntityQuery<ClothingComponent>();

        SubscribeLocalEvent<EmitsSoundOnMoveComponent, GotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<EmitsSoundOnMoveComponent, GotUnequippedEvent>(OnUnequipped);
    }

    private void OnEquipped(EntityUid uid, EmitsSoundOnMoveComponent component, GotEquippedEvent args)
    {
        component.IsSlotValid = !args.SlotFlags.HasFlag(SlotFlags.POCKET);
    }

    private void OnUnequipped(EntityUid uid, EmitsSoundOnMoveComponent component, GotUnequippedEvent args)
    {
        component.IsSlotValid = true;
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<EmitsSoundOnMoveComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            UpdateSound(uid, comp);
        }
        query.Dispose();
    }

    private void UpdateSound(EntityUid uid, EmitsSoundOnMoveComponent component)
    {
        if (!_physicsQuery.TryGetComponent(uid, out var physics)
            || !_timing.IsFirstTimePredicted)
            return;

        // Space does not transmit sound
        if (Transform(uid).GridUid == null)
            return;

        if (component.RequiresGravity && _gravity.IsWeightless(uid, physics, Transform(uid)))
            return;

        var parent = Transform(uid).ParentUid;

        var isWorn = parent is { Valid: true } &&
                     _clothingQuery.TryGetComponent(uid, out var clothing)
                     && clothing.InSlot != null
                     && component.IsSlotValid;

        if (component.RequiresWorn && !isWorn)
            return;

        // If this entity is worn by another entity, use that entity's coordinates
        var coordinates = isWorn ? Transform(parent).Coordinates : Transform(uid).Coordinates;
        var distanceNeeded = (isWorn && _moverQuery.TryGetComponent(parent, out var mover) && mover.Sprinting)
            ? component.DistanceWalking // The parent is a mob that is currently sprinting
            : component.DistanceSprinting; // The parent is not a mob or is not sprinting

        if (!coordinates.TryDistance(EntityManager, component.LastPosition, out var distance) || distance > distanceNeeded)
            component.SoundDistance = distanceNeeded;
        else
            component.SoundDistance += distance;

        component.LastPosition = coordinates;
        if (component.SoundDistance < distanceNeeded)
            return;
        component.SoundDistance -= distanceNeeded;

        var sound = component.SoundCollection;
        var audioParams = sound.Params
            .WithVolume(sound.Params.Volume)
            .WithVariation(sound.Params.Variation ?? 0f);

        _audio.PlayPredicted(sound, uid, uid, audioParams);
    }
}
