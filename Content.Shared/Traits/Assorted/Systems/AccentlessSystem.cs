namespace Content.Shared.Traits.Assorted.Systems;

/// <summary>
/// This handles removing accents when using the accentless trait.
/// </summary>
public sealed class AccentlessSystem : EntitySystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<Components.AccentlessComponent, ComponentStartup>(RemoveAccents);
    }

    private void RemoveAccents(EntityUid uid, Components.AccentlessComponent component, ComponentStartup args)
    {
        foreach (var accent in component.RemovedAccents.Values)
        {
            var accentComponent = accent.Component;
            RemComp(uid, accentComponent.GetType());
        }
    }
}
