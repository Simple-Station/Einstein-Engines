using Content.Shared.DeviceLinking;
using Content.Shared.DeviceLinking.Events;
using Content.Shared.Hands.EntitySystems;

namespace Content.Shared.Silicons.Bots;

public sealed class FillbotSystem : EntitySystem
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly SharedDeviceLinkSystem _deviceLink = default!;

    [Dependency] private SharedHandsSystem _sharedHandsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FillbotComponent, NewLinkEvent>(OnLinked);
        SubscribeLocalEvent<FillbotComponent, PortDisconnectedEvent>(OnUnlinked);
    }

    // The bot can only be linked to one thing at a time, or it'll freak out.
    private void OnLinked(EntityUid uid, FillbotComponent comp, ref NewLinkEvent args)
    {
        _sharedHandsSystem.TryDrop(uid);
        var newSink = args.Sink;
        var deviceLinkSourceComponent = _entityManager.GetComponent<DeviceLinkSourceComponent>(uid);
        _deviceLink.RemoveAllFromSource(uid, deviceLinkSourceComponent, o => o != newSink);
        comp.LinkedSinkEntity = newSink;
    }

    private void OnUnlinked(EntityUid uid, FillbotComponent comp, ref PortDisconnectedEvent args)
    {
        _sharedHandsSystem.TryDrop(uid);
        var newSink = args.RemovedPortUid;

        if (comp.LinkedSinkEntity == newSink)
            comp.LinkedSinkEntity = null;
    }
}
