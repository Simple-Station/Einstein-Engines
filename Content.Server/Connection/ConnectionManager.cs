using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Content.Server.Database;
using Content.Server.GameTicking;
using Content.Server.Preferences.Managers;
using Content.Shared.CCVar;
using Content.Shared.GameTicking;
using Content.Shared.Players.PlayTimeTracking;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Network;
using Robust.Shared.Timing;


namespace Content.Server.Connection
{
    public interface IConnectionManager
    {
        void Initialize();

        Task<bool> HasPrivilegedJoin(NetUserId userId);

        /// <summary>
        /// Temporarily allow a user to bypass regular connection requirements.
        /// </summary>
        /// <remarks>
        /// The specified user will be allowed to bypass regular player cap,
        /// whitelist and panic bunker restrictions for <paramref name="duration"/>.
        /// Bans are not bypassed.
        /// </remarks>
        /// <param name="user">The user to give a temporary bypass.</param>
        /// <param name="duration">How long the bypass should last for.</param>
        void AddTemporaryConnectBypass(NetUserId user, TimeSpan duration);
    }

    /// <summary>
    ///     Handles various duties like guest username assignment, bans, connection logs, etc...
    /// </summary>
    public sealed class ConnectionManager : IConnectionManager
    {
        [Dependency] private readonly IServerDbManager _dbManager = default!;
        [Dependency] private readonly IPlayerManager _plyMgr = default!;
        [Dependency] private readonly IServerNetManager _netMgr = default!;
        [Dependency] private readonly IServerDbManager _db = default!;
        [Dependency] private readonly IConfigurationManager _cfg = default!;
        [Dependency] private readonly ILocalizationManager _loc = default!;
        [Dependency] private readonly ServerDbEntryManager _serverDbEntry = default!;
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        [Dependency] private readonly ILogManager _logManager = default!;

        private readonly Dictionary<NetUserId, TimeSpan> _temporaryBypasses = [];
        private ISawmill _sawmill = default!;

        private List<NetUserId> _connectedWhitelistedPlayers = new(); // DeltaV - Soft whitelist improvements

        public void Initialize()
        {
            _sawmill = _logManager.GetSawmill("connections");

            _netMgr.Connecting += NetMgrOnConnecting;
            _netMgr.Connected += OnConnected; // DeltaV - Soft whitelist improvements
            _netMgr.Disconnect += OnDisconnected; // DeltaV - Soft whitelist improvements
            _netMgr.AssignUserIdCallback = AssignUserIdCallback;
            // Approval-based IP bans disabled because they don't play well with Happy Eyeballs.
            // _netMgr.HandleApprovalCallback = HandleApproval;
        }

        public void AddTemporaryConnectBypass(NetUserId user, TimeSpan duration)
        {
            ref var time = ref CollectionsMarshal.GetValueRefOrAddDefault(_temporaryBypasses, user, out _);
            var newTime = _gameTiming.RealTime + duration;
            // Make sure we only update the time if we wouldn't shrink it.
            if (newTime > time)
                time = newTime;
        }

        /*
        private async Task<NetApproval> HandleApproval(NetApprovalEventArgs eventArgs)
        {
            var ban = await _db.GetServerBanByIpAsync(eventArgs.Connection.RemoteEndPoint.Address);
            if (ban != null)
            {
                var expires = Loc.GetString("ban-banned-permanent");
                if (ban.ExpirationTime is { } expireTime)
                {
                    var duration = expireTime - ban.BanTime;
                    var utc = expireTime.ToUniversalTime();
                    expires = Loc.GetString("ban-expires", ("duration", duration.TotalMinutes.ToString("N0")), ("time", utc.ToString("f")));
                }
                var reason = Loc.GetString("ban-banned-1") + "\n" + Loc.GetString("ban-banned-2", ("reason", this.Reason)) + "\n" + expires;;
                return NetApproval.Deny(reason);
            }

            return NetApproval.Allow();
        }
        */

        private async Task NetMgrOnConnecting(NetConnectingArgs e)
        {
            var deny = await ShouldDeny(e);

            var addr = e.IP.Address;
            var userId = e.UserId;

            var serverId = (await _serverDbEntry.ServerEntity).Id;

            if (deny != null)
            {
                var (reason, msg, banHits) = deny.Value;

                var id = await _db.AddConnectionLogAsync(userId, e.UserName, addr, e.UserData.HWId, reason, serverId);
                if (banHits is { Count: > 0 })
                    await _db.AddServerBanHitsAsync(id, banHits);

                e.Deny(msg);
            }
            else
            {
                await _db.AddConnectionLogAsync(userId, e.UserName, addr, e.UserData.HWId, null, serverId);

                if (!ServerPreferencesManager.ShouldStorePrefs(e.AuthType))
                    return;

                await _db.UpdatePlayerRecordAsync(userId, e.UserName, addr, e.UserData.HWId);
            }
        }

        private async Task<(ConnectionDenyReason, string, List<ServerBanDef>? bansHit)?> ShouldDeny(
            NetConnectingArgs e)
        {
            // Check if banned.
            var addr = e.IP.Address;
            var userId = e.UserId;
            ImmutableArray<byte>? hwId = e.UserData.HWId;
            if (hwId.Value.Length == 0 || !_cfg.GetCVar(CCVars.BanHardwareIds))
            {
                // HWId not available for user's platform, don't look it up.
                // Or hardware ID checks disabled.
                hwId = null;
            }

            var bans = await _db.GetServerBansAsync(addr, userId, hwId, includeUnbanned: false);
            if (bans.Count > 0)
            {
                var firstBan = bans[0];
                var message = firstBan.FormatBanMessage(_cfg, _loc);
                return (ConnectionDenyReason.Ban, message, bans);
            }

            if (HasTemporaryBypass(userId))
            {
                _sawmill.Verbose("User {UserId} has temporary bypass, skipping further connection checks", userId);
                return null;
            }

            var adminData = await _dbManager.GetAdminDataForAsync(e.UserId);

            if (_cfg.GetCVar(CCVars.PanicBunkerEnabled) && adminData == null)
            {
                var showReason = _cfg.GetCVar(CCVars.PanicBunkerShowReason);
                var customReason = _cfg.GetCVar(CCVars.PanicBunkerCustomReason);

                var minMinutesAge = _cfg.GetCVar(CCVars.PanicBunkerMinAccountAge);
                var record = await _dbManager.GetPlayerRecordByUserId(userId);
                var validAccountAge = record != null &&
                                      record.FirstSeenTime.CompareTo(DateTimeOffset.Now - TimeSpan.FromMinutes(minMinutesAge)) <= 0;
                var bypassAllowed = _cfg.GetCVar(CCVars.BypassBunkerWhitelist) && await _db.GetWhitelistStatusAsync(userId);

                // Use the custom reason if it exists & they don't have the minimum account age
                if (customReason != string.Empty && !validAccountAge && !bypassAllowed)
                {
                    return (ConnectionDenyReason.Panic, customReason, null);
                }

                if (showReason && !validAccountAge && !bypassAllowed)
                {
                    return (ConnectionDenyReason.Panic,
                        Loc.GetString("panic-bunker-account-denied-reason",
                            ("reason", Loc.GetString("panic-bunker-account-reason-account", ("minutes", minMinutesAge)))), null);
                }

                var minOverallHours = _cfg.GetCVar(CCVars.PanicBunkerMinOverallHours);
                var overallTime = ( await _db.GetPlayTimes(e.UserId)).Find(p => p.Tracker == PlayTimeTrackingShared.TrackerOverall);
                var haveMinOverallTime = overallTime != null && overallTime.TimeSpent.TotalHours > minOverallHours;

                // Use the custom reason if it exists & they don't have the minimum time
                if (customReason != string.Empty && !haveMinOverallTime && !bypassAllowed)
                {
                    return (ConnectionDenyReason.Panic, customReason, null);
                }

                if (showReason && !haveMinOverallTime && !bypassAllowed)
                {
                    return (ConnectionDenyReason.Panic,
                        Loc.GetString("panic-bunker-account-denied-reason",
                            ("reason", Loc.GetString("panic-bunker-account-reason-overall", ("hours", minOverallHours)))), null);
                }

                if (!validAccountAge || !haveMinOverallTime && !bypassAllowed)
                {
                    return (ConnectionDenyReason.Panic, Loc.GetString("panic-bunker-account-denied"), null);
                }
            }


            var isPrivileged = await HasPrivilegedJoin(userId);
            var isQueueEnabled = _cfg.GetCVar(CCVars.QueueEnabled);

            if (_plyMgr.PlayerCount >= _cfg.GetCVar(CCVars.SoftMaxPlayers) && !isPrivileged && !isQueueEnabled)
                return (ConnectionDenyReason.Full, Loc.GetString("soft-player-cap-full"), null);


            // DeltaV - Replace existing softwhitelist implementation
            if (false) //_cfg.GetCVar(CCVars.WhitelistEnabled))
            {
                var min = _cfg.GetCVar(CCVars.WhitelistMinPlayers);
                var max = _cfg.GetCVar(CCVars.WhitelistMaxPlayers);
                var playerCountValid = _plyMgr.PlayerCount >= min && _plyMgr.PlayerCount < max;

                if (playerCountValid && await _db.GetWhitelistStatusAsync(userId) == false
                                     && adminData is null)
                {
                    var msg = Loc.GetString(_cfg.GetCVar(CCVars.WhitelistReason));
                    // was the whitelist playercount changed?
                    if (min > 0 || max < int.MaxValue)
                        msg += "\n" + Loc.GetString("whitelist-playercount-invalid", ("min", min), ("max", max));
                    return (ConnectionDenyReason.Whitelist, msg, null);
                }
            }

            // DeltaV - Soft whitelist improvements
            if (_cfg.GetCVar(CCVars.WhitelistEnabled))
            {
                var connectedPlayers = _plyMgr.PlayerCount;
                var connectedWhitelist = _connectedWhitelistedPlayers.Count;

                var slots = _cfg.GetCVar(CCVars.WhitelistMinPlayers);

                var noSlotsOpen = slots > 0 && slots < connectedPlayers - connectedWhitelist;

                if (noSlotsOpen && await _db.GetWhitelistStatusAsync(userId) == false
                                     && adminData is null)
                {
                    var msg = Loc.GetString(_cfg.GetCVar(CCVars.WhitelistReason));

                    if (slots > 0)
                        msg += "\n" + Loc.GetString("whitelist-playercount-invalid", ("min", slots), ("max", _cfg.GetCVar(CCVars.SoftMaxPlayers)));

                    return (ConnectionDenyReason.Whitelist, msg, null);
                }
            }

            return null;
        }

        private bool HasTemporaryBypass(NetUserId user)
        {
            return _temporaryBypasses.TryGetValue(user, out var time) && time > _gameTiming.RealTime;
        }

        private async Task<NetUserId?> AssignUserIdCallback(string name)
        {
            if (!_cfg.GetCVar(CCVars.GamePersistGuests))
            {
                return null;
            }

            var userId = await _db.GetAssignedUserIdAsync(name);
            if (userId != null)
            {
                return userId;
            }

            var assigned = new NetUserId(Guid.NewGuid());
            await _db.AssignUserIdAsync(name, assigned);
            return assigned;
        }

        /// <summary>
        ///     DeltaV - Soft whitelist improvements
        ///     Handles a completed connection, and stores the player if they're whitelisted and the whitelist is enabled
        /// </summary>
        private async void OnConnected(object? sender, NetChannelArgs e)
        {
            var userId = e.Channel.UserId;

            if (_cfg.GetCVar(CCVars.WhitelistEnabled) && await _db.GetWhitelistStatusAsync(userId))
            {
                _connectedWhitelistedPlayers.Add(userId);
            }
        }

        /// <summary>
        ///     DeltaV - Soft whitelist improvements
        ///     Handles a disconnection, and removes a stored player from the count if the whitelist is enabled
        /// </summary>
        private async void OnDisconnected(object? sender, NetChannelArgs e)
        {
            if (_cfg.GetCVar(CCVars.WhitelistEnabled))
            {
                _connectedWhitelistedPlayers.Remove(e.Channel.UserId);
            }
        }

        public async Task<bool> HasPrivilegedJoin(NetUserId userId)
        {
            var isAdmin = await _dbManager.GetAdminDataForAsync(userId) != null;
            var wasInGame = EntitySystem.TryGet<GameTicker>(out var ticker) &&
                            ticker.PlayerGameStatuses.TryGetValue(userId, out var status) &&
                            status == PlayerGameStatus.JoinedGame;
            return isAdmin || wasInGame;
        }
    }
}
