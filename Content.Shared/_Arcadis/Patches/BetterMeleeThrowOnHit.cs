using System.Numerics;
using Content.Shared.Construction.Components;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Melee.Components;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.GameStates;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Timing;

namespace Content.Shared._Arcadis.Patches;

/// <summary>
/// tl;dr yeet
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BetterMeleeThrowOnHitComponent : Component
{
    [DataField, AutoNetworkedField]
    public float ThrowingForce = 0.1f;

    [DataField, AutoNetworkedField]
    public float ThrowingSpeed = 5f;
}

/// <summary>
/// This handles <see cref="BetterMeleeThrowOnHitComponent"/>
/// </summary>
public sealed class BetterMeleeThrowOnHitSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<BetterMeleeThrowOnHitComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnMeleeHit(Entity<BetterMeleeThrowOnHitComponent> ent, ref MeleeHitEvent args)
    {
        if (!args.IsHit)
            return;

        foreach (var hit in args.HitEntities)
        {
            var attackerPos = _transform.GetWorldPosition(ent.Owner);
            var targetPos = _transform.GetWorldPosition(hit);
            var delta = targetPos - attackerPos;
            var normalizedDelta = Vector2.Normalize(delta);
            var flingVector = normalizedDelta * ent.Comp.ThrowingForce;
            _throwing.TryThrow(hit, flingVector, ent.Comp.ThrowingSpeed);
        }
    }
}
