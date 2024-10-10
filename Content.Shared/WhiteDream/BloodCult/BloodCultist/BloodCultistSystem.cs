namespace Content.Shared.WhiteDream.BloodCult.BloodCultist;

public sealed class BloodCultistSystem : EntitySystem
{
    public override void Initialize()
    {
        // SubscribeLocalEvent<ConstructComponent, ComponentStartup>(OnInit);
        // SubscribeLocalEvent<ConstructComponent, ComponentShutdown>(OnRemove);
        SubscribeLocalEvent<BloodCultistComponent, ComponentStartup>(OnInit);
        SubscribeLocalEvent<BloodCultistComponent, ComponentShutdown>(OnRemove);
    }

    private void OnInit(Entity<BloodCultistComponent> ent, ref ComponentStartup args)
    {
        RaiseLocalEvent(new EventCultistComponentState(true));
    }

    private void OnRemove(Entity<BloodCultistComponent> ent, ref ComponentShutdown args)
    {
        RaiseLocalEvent(new EventCultistComponentState(false));
    }
}

// TODO: Do we really need it?
public sealed class EventCultistComponentState(bool state)
{
    public bool Created { get; } = state;
}
