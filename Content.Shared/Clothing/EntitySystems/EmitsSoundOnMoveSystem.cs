using Content.Shared.Clothing.Components;
using Content.Shared.Gravity;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Events;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Components;
using Robust.Shared.Timing;

namespace Content.Shared.Clothing.Systems;

public sealed class EmitsSoundOnMoveSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedGravitySystem _gravity = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private EntityQuery<PhysicsComponent> _physicsQuery;
    private EntityQuery<ClothingComponent> _clothingQuery;

    public override void Initialize()
    {
        base.Initialize();
        _physicsQuery = GetEntityQuery<PhysicsComponent>();
        _clothingQuery = GetEntityQuery<ClothingComponent>();
        SubscribeLocalEvent<EmitsSoundOnMoveComponent, GotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<EmitsSoundOnMoveComponent, GotUnequippedEvent>(OnUnequipped);
        SubscribeLocalEvent<EmitsSoundOnMoveComponent, InventoryRelayedEvent<MakeFootstepSoundEvent>>(OnFootstep);
    }

    private void OnEquipped(EntityUid uid, EmitsSoundOnMoveComponent component, GotEquippedEvent args)
    {
        component.IsSlotValid = !args.SlotFlags.HasFlag(SlotFlags.POCKET);
    }

    private void OnUnequipped(EntityUid uid, EmitsSoundOnMoveComponent component, GotUnequippedEvent args)
    {
        component.IsSlotValid = true;
    }

    private void OnFootstep(Entity<EmitsSoundOnMoveComponent> ent, ref InventoryRelayedEvent<MakeFootstepSoundEvent> args)
    {
        var uid = ent.Owner;
        var component = ent.Comp;

        if (!_physicsQuery.TryGetComponent(uid, out var physics)
            || !_timing.IsFirstTimePredicted)
            return;

        var xform = Transform(uid);
        // Space does not transmit sound
        if (xform.GridUid is null)
            return;

        if (component.RequiresGravity && _gravity.IsWeightless(uid, physics, xform))
            return;

        var parent = xform.ParentUid;

        var isWorn = parent is { Valid: true } &&
                     _clothingQuery.TryGetComponent(uid, out var clothing)
                     && clothing.InSlot != null
                     && component.IsSlotValid;

        if (component.RequiresWorn && !isWorn)
            return;

        var sound = component.SoundCollection;
        var audioParams = sound.Params
            .WithVolume(sound.Params.Volume)
            .WithVariation(sound.Params.Variation ?? 0f);

        _audio.PlayPredicted(sound, uid, uid, audioParams);
    }
}
