using System.Linq;
using Content.Server.Aliens.Components;
using Content.Server.Polymorph.Systems;
using Content.Server.Speech.Components;
using Content.Shared.Aliens.Components;
using Content.Shared.Hands.Components;
using Content.Shared.Humanoid;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Mobs.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Utility;

namespace Content.Server.Aliens.Systems;

/// <summary>
/// This handles...
/// </summary>
public sealed class AlienEggHatchSystem : EntitySystem
{
    /// <inheritdoc/>
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AlienEggHatchComponent, InteractHandEvent>(OnInteract);
    }

    private void OnInteract(EntityUid uid, AlienEggHatchComponent component, InteractHandEvent args)
    {
        _polymorph.PolymorphEntity(uid, component.PolymorphPrototype);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<AlienEggHatchComponent>();
        while (query.MoveNext(out var uid, out var alienEgg))
        {
            foreach (var entity in _lookup.GetEntitiesInRange(uid, alienEgg.ActivationRange)
                         .Where(entity => _inventory.HasSlot(entity, "mask")))
            {
                _polymorph.PolymorphEntity(uid, alienEgg.PolymorphPrototype);
            }
        }
    }
}
