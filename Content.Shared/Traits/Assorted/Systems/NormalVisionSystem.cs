using Content.Shared.Abilities;
using Content.Shared.Traits.Assorted.Components;

namespace Content.Shared.Traits.Assorted.Systems;

/// <summary>
///     This handles removing species-specific vision traits.
/// </summary>
public sealed class NormalVisionSystem : EntitySystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<NormalVisionComponent, ComponentInit>(OnStartup);
    }


    private void OnStartup(EntityUid uid, NormalVisionComponent component, ComponentInit args)
    {
        RemComp<DogVisionComponent>(uid);
        RemComp<UltraVisionComponent>(uid);
    }
}
