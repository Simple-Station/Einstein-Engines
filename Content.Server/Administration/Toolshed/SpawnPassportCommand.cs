using Content.Server.GameTicking;
using Content.Shared._EE.Contractors.Systems;
using Content.Shared.Administration;
using Robust.Shared.Player;
using Robust.Shared.Toolshed;
using Robust.Shared.Toolshed.Errors;
using Content.Shared.Roles.Jobs;
using Content.Server.Mind;

namespace Content.Server.Administration.Toolshed;

[ToolshedCommand, AdminCommand(AdminFlags.Spawn)]
public sealed class SpawnPassportCommand : ToolshedCommand
{
    private SharedPassportSystem? _passportSystem;
    private GameTicker? _ticker;
    private MindSystem? _mindSystem;
    private SharedJobSystem? _jobSystem;

    [CommandImplementation]
    public IEnumerable<EntityUid> SpawnPassport([PipedArgument] IEnumerable<EntityUid> input)
    {
        _passportSystem ??= GetSys<SharedPassportSystem>();
        _ticker ??= GetSys<GameTicker>();
        _mindSystem ??= GetSys<MindSystem>();
        _jobSystem ??= GetSys<SharedJobSystem>();

        foreach (var i in input)
        {
            if (!TryComp(i, out ActorComponent? targetActor) || !_mindSystem.TryGetMind(i, out var mindId, out _) || !_jobSystem.MindTryGetJob(mindId, out var job))
                continue;

            var profile = _ticker.GetPlayerProfile(targetActor.PlayerSession);
            _passportSystem.SpawnPassportForPlayer(i, profile, job.ID);
            yield return i;
        }
    }

    [CommandImplementation]
    public void SpawnPassport(IInvocationContext ctx)
    {
        _passportSystem ??= GetSys<SharedPassportSystem>();
        _ticker ??= GetSys<GameTicker>();
        _mindSystem ??= GetSys<MindSystem>();
        _jobSystem ??= GetSys<SharedJobSystem>();

        if (ExecutingEntity(ctx) is not { } ent)
        {
            if (ctx.Session is {} session)
                ctx.ReportError(new SessionHasNoEntityError(session));
            else
                ctx.ReportError(new NotForServerConsoleError());
        }
        else
        {
            if (!TryComp(ent, out ActorComponent? targetActor)|| !_mindSystem.TryGetMind(ent, out var mindId, out _) || !_jobSystem.MindTryGetJob(mindId, out var job))
                return;

            var profile = _ticker.GetPlayerProfile(targetActor.PlayerSession);
            _passportSystem.SpawnPassportForPlayer(ent, profile, job.ID);
        }
    }
}
