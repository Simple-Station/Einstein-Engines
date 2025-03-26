using Content.Shared._EE.Contractors.Components;
using Content.Shared.Interaction.Events;
using Robust.Shared.Timing;


namespace Content.Shared._EE.Contractors.Systems;

public class SharedPassportSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PassportComponent, UseInHandEvent>(OnUseInHand);
    }

    private void OnUseInHand(Entity<PassportComponent> passport, ref UseInHandEvent evt)
    {
        if (evt.Handled || !_timing.IsFirstTimePredicted)
            return;

        evt.Handled = true;
        passport.Comp.IsClosed = !passport.Comp.IsClosed;

        var passportEvent = new PassportToggleEvent();
        RaiseLocalEvent(passport, ref passportEvent);
    }

    [ByRefEvent]
    public sealed class PassportToggleEvent : HandledEntityEventArgs {}
}
