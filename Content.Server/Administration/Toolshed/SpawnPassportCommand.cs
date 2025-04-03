using Content.Server.GameTicking;
using Content.Shared._EE.Contractors.Systems;
using Content.Shared.Administration;
using Robust.Shared.Player;
using Robust.Shared.Toolshed;
using Robust.Shared.Toolshed.Errors;

namespace Content.Server.Administration.Toolshed;

[ToolshedCommand, AdminCommand(AdminFlags.Debug)]
public sealed class SpawnPassportCommand : ToolshedCommand
{
    private SharedPassportSystem? _passportSystem;
    private GameTicker? _ticker;

    [CommandImplementation]
    public IEnumerable<EntityUid> Rejuvenate([PipedArgument] IEnumerable<EntityUid> input)
    {
        _passportSystem ??= GetSys<SharedPassportSystem>();
        _ticker ??= GetSys<GameTicker>();

        foreach (var i in input)
        {
            if (!TryComp(i, out ActorComponent? targetActor))
                continue;

            var profile = _ticker.GetPlayerProfile(targetActor.PlayerSession);
            _passportSystem.SpawnPassportForPlayer(i, profile);
            yield return i;
        }
    }

    [CommandImplementation]
    public void SpawnPassport(IInvocationContext ctx)
    {
        _passportSystem ??= GetSys<SharedPassportSystem>();
        _ticker ??= GetSys<GameTicker>();

        if (ExecutingEntity(ctx) is not { } ent)
        {
            if (ctx.Session is {} session)
                ctx.ReportError(new SessionHasNoEntityError(session));
            else
                ctx.ReportError(new NotForServerConsoleError());
        }
        else
        {
            if (!TryComp(ent, out ActorComponent? targetActor))
                return;

            var profile = _ticker.GetPlayerProfile(targetActor.PlayerSession);
            _passportSystem.SpawnPassportForPlayer(ent, profile);
        }
    }
}
