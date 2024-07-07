using Content.Shared.Alert;
using Content.Shared.Buckle;
using Content.Shared.Buckle.Components;
using JetBrains.Annotations;
using Robust.Shared.IoC;

namespace Content.Server.Alert.Click
{
    /// <summary>
    /// Unbuckles if player is currently buckled.
    /// </summary>
    [UsedImplicitly]
    [DataDefinition]
    public sealed partial class Unbuckle : IAlertClick
    {
        public void AlertClicked(EntityUid player)
        {
            var entityManager = IoCManager.Resolve<IEntityManager>();
            var buckleSystem = entityManager.System<SharedBuckleSystem>();
            var buckleComp = entityManager.GetComponent<BuckleComponent>(player);
            var strapComp = buckleComp.BuckledTo != null ? entityManager.GetComponent<StrapComponent>(buckleComp.BuckledTo.Value) : null;
            buckleSystem.TryUnbuckle(player, player, false, buckleComp, strapComp);
        }
    }
}
