using Content.Shared.WhiteDream.BloodCult.Components;

namespace Content.Shared.WhiteDream.BloodCult.Systems;

public sealed class BloodCultistSystem : EntitySystem
{
    public override void Initialize()
    {
        // SubscribeLocalEvent<ConstructComponent, ComponentStartup>(OnInit);
        // SubscribeLocalEvent<ConstructComponent, ComponentShutdown>(OnRemove);
        SubscribeLocalEvent<BloodCultistComponent, ComponentStartup>(OnInit);
        SubscribeLocalEvent<BloodCultistComponent, ComponentShutdown>(OnRemove);
    }

    private void OnInit<T>(EntityUid uid, T component, ComponentStartup args)
    {
        RaiseLocalEvent(new EventCultistComponentState(true));
    }

    private void OnRemove<T>(EntityUid uid, T component, ComponentShutdown args)
    {
        RaiseLocalEvent(new EventCultistComponentState(false));
    }
}

// TODO: Do we really need it?
public sealed class EventCultistComponentState(bool state)
{
    public bool Created { get; } = state;
}
