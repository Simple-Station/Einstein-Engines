using Content.Shared.SpawnPassport;

namespace Content.Server.Administration.Systems;

public sealed class SpawnPassportSystem : EntitySystem
{
    public void PerformSpawnPassport(EntityUid target)
    {
        RaiseLocalEvent(target, new SpawnPassportEvent());
    }
}
