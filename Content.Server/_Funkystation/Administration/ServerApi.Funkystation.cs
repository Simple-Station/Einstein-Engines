using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Content.Server.Database;
using Robust.Server.ServerStatus;

namespace Content.Server.Administration;

public sealed partial class ServerApi
{
    [Dependency] private readonly IServerDbManager _dbManager = default!;
    [Dependency] private readonly IPlayerLocator _playerLocator = default!;

    public void InitializeFunky()
    {
        RegisterHandler(HttpMethod.Post, "/admin/actions/whitelist", ActionWhitelist);
    }

    /// <summary>
    ///     Whitelists a player.
    /// </summary>
    private async Task ActionWhitelist(IStatusHandlerContext context)
    {
        var body = await ReadJson<WhitelistActionBody>(context);

        if (body == null)
            return;

        var data = await _playerLocator.LookupIdByNameOrIdAsync(body.Username);

        if (data == null)
        {
            await RespondError(
                context,
                ErrorCode.PlayerNotFound,
                HttpStatusCode.UnprocessableContent,
                "Player not found");
            return;
        }

        var isWhitelisted = await _dbManager.GetWhitelistStatusAsync(data.UserId);

        if (isWhitelisted)
        {
            await RespondError(
                context,
                ErrorCode.BadRequest,
                HttpStatusCode.Conflict,
                "Already whitelisted");
            return;
        }

        await _dbManager.AddToWhitelistAsync(data.UserId);
        await RespondOk(context);
    }

    private sealed class WhitelistActionBody
    {
        public required string Username { get; init; }
    }
}
