/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using Robust.Server.GameStates;

namespace Content.Server._CE.PVS;

public sealed partial class CEPvsOverrideSystem : EntitySystem
{
    [Dependency] private readonly PvsOverrideSystem _pvs = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<CEPvsOverrideComponent, ComponentStartup>(OnPvsStartup);
        SubscribeLocalEvent<CEPvsOverrideComponent, ComponentShutdown>(OnPvsShutdown);
    }

    private void OnPvsShutdown(Entity<CEPvsOverrideComponent> ent, ref ComponentShutdown args)
    {
        _pvs.RemoveGlobalOverride(ent);
    }

    private void OnPvsStartup(Entity<CEPvsOverrideComponent> ent, ref ComponentStartup args)
    {
        _pvs.AddGlobalOverride(ent);
    }
}
