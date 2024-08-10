using Content.Shared.Actions;
using Content.Shared.DeltaV.Harpy.Components;
using Content.Shared.Instruments;
using Content.Shared.Traits.Assorted.Components;
using Content.Shared.Zombies;

namespace Content.Shared.Traits.Assorted.Systems;

public abstract class SharedSingerSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EntityZombifiedEvent>(OnZombified);
        SubscribeLocalEvent<SingerComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<SingerComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<SingerComponent, BoundUIClosedEvent>(OnBoundUIClosed);
        SubscribeLocalEvent<SingerComponent, BoundUIOpenedEvent>(OnBoundUIOpened);
    }

    private void OnStartup(Entity<SingerComponent> ent, ref ComponentStartup args)
    {
        _actionsSystem.AddAction(ent, ref ent.Comp.MidiAction, ent.Comp.MidiActionId);

        if (ent.Comp.MidiUi is { } data && !_ui.TryGetUi(ent, data.UiKey, out _))
            _ui.AddUi(ent.Owner, data); // Stinky
    }

    private void OnShutdown(Entity<SingerComponent> ent, ref ComponentShutdown args)
    {
        _actionsSystem.RemoveAction(ent, ent.Comp.MidiAction);
    }

    private void OnZombified(ref EntityZombifiedEvent args)
    {
        CloseMidiUi(args.Target);
    }

    private void OnBoundUIClosed(EntityUid uid, SingerComponent component, BoundUIClosedEvent args)
    {
        if (args.UiKey is not InstrumentUiKey)
            return;

        TryComp(uid, out AppearanceComponent? appearance);
        _appearance.SetData(uid, HarpyVisualLayers.Singing, SingingVisualLayer.False, appearance);
    }

    private void OnBoundUIOpened(EntityUid uid, SingerComponent component, BoundUIOpenedEvent args)
    {
        if (args.UiKey is not InstrumentUiKey)
            return;

        TryComp(uid, out AppearanceComponent? appearance);
        _appearance.SetData(uid, HarpyVisualLayers.Singing, SingingVisualLayer.True, appearance);
    }

    /// <summary>
    /// Closes the MIDI UI if it is open. Does nothing on client side.
    /// </summary>
    public virtual void CloseMidiUi(EntityUid uid)
    {
    }
}
