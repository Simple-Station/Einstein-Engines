using Content.Shared.Crescent.Radar;
using Content.Server.Shuttles.Systems;

namespace Content.Server.Crescent.Radar;

public sealed partial class TurretIFFSystem : SharedTurretIFFSystem
{
    [Dependency] private readonly ShuttleConsoleSystem _shuttleConsole = default!;
    [Dependency] private readonly RadarConsoleSystem _radarConsole = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TurretIFFComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<TurretIFFComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(EntityUid uid, TurretIFFComponent component, ComponentStartup args)
    {
        if (EntityManager.GetComponent<MetaDataComponent>(uid).EntityLifeStage < EntityLifeStage.Initialized)
        {
            return;
        }

        _shuttleConsole.RefreshIFFState();
        _radarConsole.RefreshIFFState();
    }

    private void OnShutdown(EntityUid uid, TurretIFFComponent component, ComponentShutdown args)
    {
        if (EntityManager.GetComponent<MetaDataComponent>(uid).EntityLifeStage > EntityLifeStage.MapInitialized)
        {
            return;
        }

        _shuttleConsole.RefreshIFFState();
        _radarConsole.RefreshIFFState();
    }
}
