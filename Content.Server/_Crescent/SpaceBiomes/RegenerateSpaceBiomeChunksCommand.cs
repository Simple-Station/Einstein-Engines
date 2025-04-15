using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server._Crescent.SpaceBiomes;

[AdminCommand(AdminFlags.Mapping)]
public sealed class RegenerateSpaceBiomeChunksCommand : IConsoleCommand
{
    public string Command => "sb_genchunks";
    public string Description => "Regenerates space biome chunks.";
    public string Help => "No extra arguments required.";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        IoCManager.Resolve<IEntityManager>().System<SpaceBiomeSystem>().RegenerateChunks();
    }
}
