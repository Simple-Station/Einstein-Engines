using Content.Client.Chemistry.UI;
using Content.Client.Items;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Robust.Shared.GameStates;

namespace Content.Client.Chemistry.EntitySystems;

public sealed class FillableOneTimeInjectorSystem : SharedFillableOneTimeInjectorSystem
{
    public override void Initialize()
    {
        base.Initialize();
        Subs.ItemStatus<FillableOneTimeInjectorComponent>(ent => new FillableOneTimeInjectorStatusControl(ent, SolutionContainers));
    }
}
