using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using JetBrains.Annotations;

namespace Content.Server.Destructible.Thresholds.Behaviors
{
    [UsedImplicitly]
    [DataDefinition]
    public sealed partial class GibPartBehavior : IThresholdBehavior
    {
        public void Execute(EntityUid owner, DestructibleSystem system, EntityUid? cause = null)
        {
            if (system.EntityManager.TryGetComponent(owner, out BodyPartComponent? part))
            {
                system.BodySystem.GibPart(owner, part);
            }
        }
    }
}
