#region

using Content.Client.Chemistry.UI;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;

#endregion


namespace Content.Client.Chemistry.EntitySystems;


public sealed class InjectorSystem : SharedInjectorSystem
{
    public override void Initialize()
    {
        base.Initialize();
        Subs.ItemStatus<InjectorComponent>(ent => new InjectorStatusControl(ent, SolutionContainers));
    }
}
