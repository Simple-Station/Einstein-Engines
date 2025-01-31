using Content.Shared.DeviceLinking;
using Content.Shared.DeviceLinking.Events;
using Content.Shared.Interaction;
using Content.Shared.Materials;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;

namespace Content.Shared._Arcadis.Scrap;

public sealed class ScrapReprocessorSystem : EntitySystem
{

    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly SharedDeviceLinkSystem _deviceLinkSystem = default!;
    [Dependency] private readonly SharedMaterialStorageSystem _matStorSys = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ScrapReprocessorComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<ScrapReprocessorComponent, NewLinkEvent>(OnNewLink);
        SubscribeLocalEvent<ScrapReprocessorComponent, PortDisconnectedEvent>(OnPortDisconnected);
    }

    private void OnInteractUsing(EntityUid uid, ScrapReprocessorComponent component, InteractUsingEvent args)
    {
        // Check if inhand item is scrap
        if (!TryComp<ScrapComponent>(args.Used, out var scrapComponent))
        {
            _popupSystem.PopupPredicted(Loc.GetString("reprocessor-not-scrap"), args.Target, args.User);
            return;
        }

        // Check if reprocessor has a matsilo connected
        if (!component.MatSilo.HasValue)
        {
            _popupSystem.PopupPredicted(Loc.GetString("reprocessor-no-silo"), args.Target, args.User);
            return;
        }

        // Play sound
        _audioSystem.PlayPvs(component.Sound, args.Target);

        _popupSystem.PopupPredicted(Loc.GetString("debug"), args.Target, args.User);


    }

    private void OnNewLink(EntityUid uid, ScrapReprocessorComponent component, NewLinkEvent args)
    {
        if (args.Source != uid)
            return;

        if (TryComp<MaterialSiloComponent>(args.Sink, out var siloComponent))
        {
            component.MatSilo = args.Sink;
        }
        else
        {
            _popupSystem.PopupPredicted(Loc.GetString("debug"), args.Source, args.User);
        }
    }

    private void OnPortDisconnected(EntityUid uid, ScrapReprocessorComponent component, PortDisconnectedEvent args)
    {
        if (args.Port == "MaterialSilo")
        {
            component.MatSilo = null;
        }
    }
}
