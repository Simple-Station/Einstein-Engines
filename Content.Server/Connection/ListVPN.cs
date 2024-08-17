using Content.Shared.Administration;
using Content.Shared.CCVar;
using Robust.Shared.Configuration;
using Robust.Shared.Console;
using Robust.Server.Player;
using Content.Server.Administration;
using Content.Server.Database;
using Content.Shared.Players.PlayTimeTracking;
using System.Text;
using Content.Server.Players.PlayTimeTracking;

namespace Content.Server.Connection;

[AdminCommand(AdminFlags.Admin)]
public sealed class ListVPNCommand : LocalizedCommands
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    [Dependency] private readonly IPlayerManager _players = default!;
    [Dependency] private readonly IIPInformation _ipInformation = default!;
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly PlayTimeTrackingManager _playTimeTrackingManager = default!;

    public override string Command => "listvpn";
    public override async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        _playTimeTrackingManager.FlushAllTrackers(); // return up to date times

        var sb = new StringBuilder();

        var players = _players.Sessions;
        sb.AppendLine("Here is a report of all connected players.  Keep in mind IP Suspicion of below .99 is considered normal.");
        sb.AppendLine($"{"Player Name",20} {"Status",12} {"Total Time",16} {"Ping",9} {"IP Suspicion",20} {"Country", 10}");
        sb.AppendLine("------------------------------------------------------------------------------------------");

        foreach (var player in players)
        {
            IPData ipData;
            try
            {
                ipData = await _ipInformation.GetIPInformationAsync(player.Channel.RemoteEndPoint.Address); // should already be cached
            } catch (Exception e)
            {
                shell.WriteError("Couldn't get IP data for " + player.Name);
                continue;
            }

            if (ipData == null)
            {
                shell.WriteError("Couldn't get IP data for " + player.Name);
                continue;
            }

            TimeSpan? overallTime = null;
            if (_playTimeTrackingManager.TryGetTrackerTimes(player, out var playTimes))
            {
                playTimes.TryGetValue(PlayTimeTrackingShared.TrackerOverall, out var overallTimeTemp);
                overallTime = overallTimeTemp; // couldn't figure out a more elegant way of doing this
            }

            sb.AppendLine(string.Format("{4,20} {1,12} {2,16:dd\\:hh\\:mm\\:ss} {3,9} {0,20} {5,10}",
                ipData.suspiciousScore.ToString("#.####"),
                player.Status.ToString(),
                overallTime,
                player.Channel.Ping + "ms",
                player.Name,
                ipData.country));
        }

        shell.WriteLine(sb.ToString());
    }
}
