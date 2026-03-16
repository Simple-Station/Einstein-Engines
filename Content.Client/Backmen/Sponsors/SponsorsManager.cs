using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Corvax.Interfaces.Client;
using Content.Corvax.Interfaces.Shared;
using Content.Shared.Backmen.Sponsors;
using Robust.Shared.Network;

namespace Content.Client.Backmen.Sponsors;

public sealed class SponsorsManager : ISharedSponsorsManager
{
    [Dependency] private readonly IClientNetManager _netMgr = default!;

    public void Initialize()
    {
        _netMgr.RegisterNetMessage<MsgSponsorInfo>(OnUpdate);
        //_netMgr.RegisterNetMessage<Shared.Backmen.MsgWhitelist>(RxWhitelist); //backmen: whitelist
    }

    public List<string> GetClientPrototypes()
    {
        return Prototypes.ToList();
    }

    public List<string> GetClientLoadouts()
    {
        return Loadouts.ToList();
    }

    public bool IsClientAllRoles()
    {
        return OpenAllRoles;
    }

    public void Cleanup()
    {
        Reset();
    }

    //private void RxWhitelist(Shared.Backmen.MsgWhitelist message)
    //{
    //    Whitelisted = message.Whitelisted;
    //}

    private void OnUpdate(MsgSponsorInfo message)
    {
        Reset();

#if DEBUG
        foreach (var ghostProto in IoCManager.Resolve<Robust.Shared.Prototypes.IPrototypeManager>().EnumeratePrototypes<Shared.Backmen.GhostTheme.GhostThemePrototype>())
        {
            Prototypes.Add(ghostProto.ID);
        }
#endif

        if (message.Info == null)
        {
            return;
        }

        foreach (var markings in message.Info.AllowedMarkings)
        {
            Prototypes.Add(markings);
        }

        foreach (var loadout in message.Info.Loadouts)
        {
            Loadouts.Add(loadout);
        }

        OpenAllRoles = message.Info.OpenAllRoles;
        Tier = message.Info.Tier ?? 0;
        GhostTheme = message.Info.GhostTheme;
    }

    private void Reset()
    {
        Prototypes.Clear();
        Loadouts.Clear();
        OpenAllRoles = false;
        Tier = 0;
        GhostTheme = null;
    }

    public HashSet<string> Prototypes { get; } = new();
    public HashSet<string> Loadouts { get; } = new();
    public bool OpenAllRoles { get; private set; } = false;
    public int Tier { get; private set; } = 0;
    public bool Whitelisted { get; private set; } = false;
    public string? GhostTheme { get; private set; } = null;
}
