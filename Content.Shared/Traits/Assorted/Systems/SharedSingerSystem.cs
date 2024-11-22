using Content.Shared.Actions;
using Content.Shared.DeltaV.Harpy.Components;
using Content.Shared.Instruments;
using Content.Shared.Traits.Assorted.Components;
using Content.Shared.Traits.Assorted.Prototypes;
using Content.Shared.Zombies;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Shared.Traits.Assorted.Systems;

public abstract class SharedSingerSystem : EntitySystem
{
    [Dependency] protected readonly IPrototypeManager ProtoMan = default!;

    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedInstrumentSystem _instrument = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EntityZombifiedEvent>(OnZombified);
        SubscribeLocalEvent<SingerComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<SingerComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<SingerComponent, BoundUIClosedEvent>(OnBoundUIClosed);
        SubscribeLocalEvent<SingerComponent, BoundUIOpenedEvent>(OnBoundUIOpened);
        SubscribeLocalEvent<SingerComponent, PlayerDetachedEvent>(OnPlayerDetached);
    }

    private void OnStartup(Entity<SingerComponent> ent, ref ComponentStartup args)
    {
        if (!ProtoMan.TryIndex(ent.Comp.Proto, out var singer))
            return;

        _actionsSystem.AddAction(ent, ref ent.Comp.MidiAction, ent.Comp.MidiActionId);

        var instrumentComp = EnsureInstrumentComp(ent);
        var defaultData = singer.InstrumentList[singer.DefaultInstrument];
        _instrument.SetInstrumentProgram(ent.Owner, instrumentComp, defaultData.Item1, defaultData.Item2);
        SetUpSwappableInstrument(ent, singer);
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

    private void OnPlayerDetached(EntityUid uid, SingerComponent component, PlayerDetachedEvent args)
    {
        CloseMidiUi(uid);
    }

    /// <summary>
    ///     Closes the MIDI UI if it is open. Does nothing on client side.
    /// </summary>
    public virtual void CloseMidiUi(EntityUid uid)
    {
    }

    /// <summary>
    ///     Sets up the swappable instrument on the entity, only on the server.
    /// </summary>
    protected virtual void SetUpSwappableInstrument(EntityUid uid, SingerInstrumentPrototype singer)
    {
    }

    /// <summary>
    ///     Ensures an InstrumentComponent on the entity. Uses client-side comp on client and server-side comp on the server.
    /// </summary>
    protected abstract SharedInstrumentComponent EnsureInstrumentComp(EntityUid uid);
}
