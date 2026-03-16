using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Content.Corvax.Interfaces.Server;
using Content.Corvax.Interfaces.Shared;
using Content.Shared.Backmen.Sponsors;
using Content.Shared.Backmen.CCVar;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Network;
using Robust.Shared.Utility;
using Exception = System.Exception;

namespace Content.Server.Backmen.Sponsors;

public sealed class SponsorsManager : ISharedSponsorsManager
{
    [Dependency] private readonly IServerNetManager _netMgr = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    private readonly HttpClient _httpClient = new();

    private ISawmill _sawmill = default!;
    private string _apiUrl = string.Empty;

    private readonly Dictionary<NetUserId, SponsorInfo> _cachedSponsors = new();

    private readonly ReaderWriterLockSlim _lock = new();

    public void Initialize()
    {
        _sawmill = Logger.GetSawmill("sponsors");
        _cfg.OnValueChanged(CCVars.SponsorsApiUrl, s => _apiUrl = s, true);

        _netMgr.RegisterNetMessage<MsgSponsorInfo>();

        _netMgr.Connecting += OnConnecting;
        _netMgr.Connected += OnConnected;
    }

    public bool TryGetServerPrototypes(NetUserId userId, [NotNullWhen(true)] out List<string>? prototypes)
    {
        if (_cachedSponsors.TryGetValue(userId, out var sponsor))
        {
            prototypes = sponsor.AllowedMarkings.ToList();
            return true;
        }

        prototypes = null;
        return false;
    }

    public bool TryGetServerOocColor(NetUserId userId, [NotNullWhen(true)] out Color? color)
    {
        if (_cachedSponsors.TryGetValue(userId, out var sponsor))
        {
            color = Color.TryFromHex(sponsor.OOCColor);
            return color != null;
        }

        color = null;
        return false;
    }

    public int GetServerExtraCharSlots(NetUserId userId)
    {
        return _cachedSponsors.TryGetValue(userId, out var sponsor) ? sponsor.ExtraSlots : 0;
    }

    public bool HaveServerPriorityJoin(NetUserId userId)
    {
        return _cachedSponsors.TryGetValue(userId, out var sponsor) && sponsor.HavePriorityJoin;
    }

    private async Task OnConnecting(NetConnectingArgs e)
    {
        SponsorInfo? info;
        try
        {
            info = await LoadSponsorInfo(e.UserId);
            if (info?.Tier == null)
            {
                _cachedSponsors.Remove(e.UserId); // Remove from cache if sponsor expired
                return;
            }
        }
        catch (Exception err)
        {
            _sawmill.Error(err.ToString());
            return;
        }


        //DebugTools.Assert(!_cachedSponsors.ContainsKey(e.UserId), "Cached data was found on client connect");

        _lock.EnterWriteLock();
        try
        {
            _cachedSponsors[e.UserId] = info;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    private void OnConnected(object? sender, NetChannelArgs e)
    {
        _lock.EnterReadLock();
        try
        {
            var info = _cachedSponsors.TryGetValue(e.Channel.UserId, out var sponsor) ? sponsor : null;
            _netMgr.ServerSendMessage(new MsgSponsorInfo() { Info = info }, e.Channel);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public void Cleanup()
    {
        _lock.EnterWriteLock();
        try
        {
            var online = _playerManager.SessionsDict.Keys.ToArray();
            foreach (var userId in _cachedSponsors.Keys.Where(x => !online.Contains(x)).ToArray())
            {
                _cachedSponsors.Remove(userId);
            }
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    private async Task<SponsorInfo?> LoadSponsorInfo(NetUserId userId)
    {
        if (string.IsNullOrEmpty(_apiUrl))
            return null;

        var url = $"{_apiUrl}/sponsors/{userId.ToString()}";
        var response = await _httpClient.GetAsync(url);
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        if (response.StatusCode != HttpStatusCode.OK)
        {
            var errorText = await response.Content.ReadAsStringAsync();
            _sawmill.Error(
                "Failed to get player sponsor OOC color from API: [{StatusCode}] {Response}",
                response.StatusCode,
                errorText);
            return null;
        }

        return await response.Content.ReadFromJsonAsync<SponsorInfo>();
    }

    public bool TryGetGhostTheme(NetUserId userId, [NotNullWhen(true)] out string? ghostTheme)
    {
        if (!_cachedSponsors.ContainsKey(userId) || string.IsNullOrEmpty(_cachedSponsors[userId].GhostTheme))
        {
            ghostTheme = null;
            return false;
        }

        ghostTheme = _cachedSponsors[userId].GhostTheme!;
        return true;
    }

    public bool TryGetPrototypes(NetUserId userId, [NotNullWhen(true)] out List<string>? prototypes)
    {
        if (!_cachedSponsors.ContainsKey(userId) || _cachedSponsors[userId].AllowedMarkings.Length == 0)
        {
            prototypes = null;
            return false;
        }

        prototypes = new List<string>();
        prototypes.AddRange(_cachedSponsors[userId].AllowedMarkings);

        return true;
    }

    public bool TryGetLoadouts(NetUserId userId, [NotNullWhen(true)] out List<string>? prototypes)
    {
        if (!_cachedSponsors.ContainsKey(userId) || _cachedSponsors[userId].Loadouts.Length == 0)
        {
            prototypes = null;
            return false;
        }

        prototypes = new List<string>();
        prototypes.AddRange(_cachedSponsors[userId].Loadouts);

        return true;
    }

    public bool IsServerAllRoles(NetUserId userId)
    {
        return _cachedSponsors.ContainsKey(userId) && _cachedSponsors[userId].OpenAllRoles;
    }

    public bool TryGetOocColor(NetUserId userId, [NotNullWhen(true)] out Color? color)
    {
        if (!_cachedSponsors.ContainsKey(userId) || _cachedSponsors[userId].OOCColor == null)
        {
            color = null;
            return false;
        }

        color = Color.TryFromHex(_cachedSponsors[userId].OOCColor);

        return color != null;
    }

    public int GetExtraCharSlots(NetUserId userId)
    {
        if (!_cachedSponsors.ContainsKey(userId))
        {
            return 0;
        }

        return _cachedSponsors[userId].ExtraSlots;
    }

    public bool HavePriorityJoin(NetUserId userId)
    {
        if (!_cachedSponsors.ContainsKey(userId))
        {
            return false;
        }

        return _cachedSponsors[userId].HavePriorityJoin;
    }
}
