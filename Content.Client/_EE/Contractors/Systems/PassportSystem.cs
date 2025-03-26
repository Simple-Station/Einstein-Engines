using Content.Shared._EE.Contractors.Components;
using Content.Shared._EE.Contractors.Systems;
using Robust.Client.GameObjects;
using Robust.Client.Timing;
using Robust.Shared.Timing;


namespace Content.Client._EE.Contractors.Systems;

public sealed class PassportSystem : EntitySystem
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IClientGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PassportComponent, SharedPassportSystem.PassportToggleEvent>(OnPassportToggled);
    }

    private void OnPassportToggled(Entity<PassportComponent> passport, ref SharedPassportSystem.PassportToggleEvent evt)
    {
        if (!_timing.IsFirstTimePredicted || evt.Handled || !_entityManager.TryGetComponent<SpriteComponent>(passport, out var sprite))
            return;

        var currentState = sprite.LayerGetState(0);

        if (currentState.Name == null)
            return;

        evt.Handled = true;

        sprite.LayerSetVisible(1, !passport.Comp.IsClosed);

        var oldState = passport.Comp.IsClosed? "open" : "closed";
        var newState = passport.Comp.IsClosed ? "closed" : "open";

        var newStateName = currentState.Name.Replace(oldState, newState);

        sprite.LayerSetState(0, newStateName);
    }
}
