using Content.Server.Aliens.Components;
using Content.Server.Weapons.Melee;
using Content.Shared.Aliens.Components;
using Content.Shared.Coordinates;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Tag;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Server.Aliens.Systems;

/// <summary>
/// This handles...
/// </summary>
public sealed class AlienAcidSystem : EntitySystem
{
    /// <inheritdoc/>
    [Dependency] private readonly MeleeWeaponSystem _meleeWeapon = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<AlienAcidComponent, MeleeHitEvent>(OnHit);
    }

    private void OnHit(EntityUid uid, AlienAcidComponent component, MeleeHitEvent args)
    {
        foreach (var hitEntity in args.HitEntities)
        {
            if (_tag.HasTag(hitEntity, "Wall"))
            {
                Spawn(component.AcidPrototype, hitEntity.ToCoordinates());
            }
        }
    }
}
