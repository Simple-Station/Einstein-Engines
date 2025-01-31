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

    [Dependency] private readonly ILogManager _logManager = default!;
    private ISawmill _sawmill = default!;
    public override void Initialize()
    {
        _sawmill = _logManager.GetSawmill("factorio");

        base.Initialize();
        SubscribeLocalEvent<ScrapReprocessorComponent, InteractUsingEvent>(OnInteractUsing);
    }

    private void OnInteractUsing(EntityUid uid, ScrapReprocessorComponent component, InteractUsingEvent args)
    {
        // Check if inhand item is scrap
        if (!TryComp<ScrapComponent>(args.Used, out var scrapComponent))
        {
            _popupSystem.PopupPredicted(Loc.GetString("reprocessor-not-scrap"), uid, args.User);
            return;
        }

        // Check if reprocessor has a matsilo connected
        if (!TryComp<MaterialSiloUtilizerComponent>(uid, out var siloUtilizerComponent) || siloUtilizerComponent.Silo == null)
        {
            _popupSystem.PopupPredicted(Loc.GetString("reprocessor-no-silo"), args.Target, args.User);
            return;
        }

        // Play sound
        _audioSystem.PlayPvs(component.Sound, args.Target);



    }
}
