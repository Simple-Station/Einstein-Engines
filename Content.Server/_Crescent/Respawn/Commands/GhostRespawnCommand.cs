using Content.Server.GameTicking;
using Content.Server.Mind;
using Content.Shared._Crescent.CCvars;
using Content.Shared.Administration;
using Content.Shared.Ghost;
using Robust.Shared.Configuration;
using Robust.Shared.Console;

namespace Content.Server._Crescent.Respawn.Commands;

[AnyCommand]
public sealed class GhostRespawnCommand : IConsoleCommand
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IConfigurationManager _configurationManager = default!;

    public string Command => "ghostrespawn";
    public string Description => "Allows the player to return to the lobby if they've been dead long enough, allowing re-entering the round AS ANOTHER CHARACTER.";
    public string Help => $"{Command}";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (!_configurationManager.GetCVar(CrescentCVars.RespawnEnabled))
        {
            shell.WriteLine("Respawning is disabled, ask an admin to respawn you.");
            return;
        }

        if (shell.Player is null)
        {
            shell.WriteLine("You cannot run this from the console!");
            return;
        }

        if (shell.Player.AttachedEntity is null)
        {
            shell.WriteLine("You cannot run this in the lobby, or without an entity.");
            return;
        }

        if (!_entityManager.HasComponent<GhostComponent>(shell.Player.AttachedEntity))
        {
            shell.WriteLine("You are not a ghost.");
            return;
        }

        var mindSystem = _entityManager.System<MindSystem>();
        if (!mindSystem.TryGetMind(shell.Player, out _, out _))
        {
            shell.WriteLine("You have no mind.");
            return;
        }

        if (!_entityManager.System<RespawnTrackerSystem>().CheckRespawn(shell.Player.UserId))
        {
            shell.WriteLine($"Trying to respawn before timer is up.");
            return;
        }

        var gameTicker = _entityManager.System<GameTicker>();
        gameTicker.Respawn(shell.Player);
    }
}
