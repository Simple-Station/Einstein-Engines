using Content.Shared._White.Bark;
using Content.Shared._White.Bark.Systems;
using Robust.Server.GameObjects;
using Robust.Shared.Console;
using Robust.Shared.Player;
using BarkComponent = Content.Shared._White.Bark.Components.BarkComponent;

namespace Content.Server._White.Bark;

public sealed class BarkSystem : SharedBarkSystem
{
    [Dependency] private readonly TransformSystem _transformSystem = default!;

    public override void Bark(Entity<BarkComponent> entity, List<BarkData> barks)
    {
        var mapPos = _transformSystem.GetMapCoordinates(entity.Owner);
        var filter = Filter.Empty().AddInRange(mapPos, 16f);
        RaiseNetworkEvent(new EntityBarkEvent(GetNetEntity(entity), barks), filter);
    }
}


public sealed class AddBarkCommand : IConsoleCommand
{
    public string Command => "addbark";
    public string Description => "add bark to self";
    public string Help => Command + " uid prototype";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var entMan = IoCManager.Resolve<IEntityManager>();

        if (args.Length < 2)
        {
            shell.WriteError("Small args count");
            return;
        }

        if (!entMan.TryParseNetEntity(args[0], out var attachedEnt))
        {
            shell.WriteError($"Could not find attached entity " + args[0]);
            return;
        }

        entMan.System<BarkSystem>().ApplyBark(attachedEnt.Value, args[1]);
    }

    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if(args.Length == 1)
            return
 CompletionResult.FromHint("Uid");
        if (args.Length == 2)
            return CompletionResult.FromHintOptions(CompletionHelper.PrototypeIDs<BarkVoicePrototype>(), "bark prototype");

        return CompletionResult.Empty;
    }
}
