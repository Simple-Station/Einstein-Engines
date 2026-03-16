using Content.Corvax.Interfaces.Shared;
using Content.Shared.Preferences.Loadouts;
using Robust.Shared.Prototypes;

namespace Content.Client.Backmen.Sponsors;

public sealed class LoadoutsManager : ISharedLoadoutsManager
{
    [Dependency] private readonly ISharedSponsorsManager _sponsorsManager = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    public void Initialize()
    {
    }

    public List<string> GetClientPrototypes()
    {
        return _sponsorsManager.GetClientLoadouts();
    }

    public List<LoadoutPrototype> GetClientLoadoutPrototypes()
    {
        var r = new List<LoadoutPrototype>();
        foreach (var clientPrototype in GetClientPrototypes())
        {
            if(_prototype.TryIndex<LoadoutPrototype>(clientPrototype, out var loadout))
                r.Add(loadout);
        }
        return r;
    }
}
