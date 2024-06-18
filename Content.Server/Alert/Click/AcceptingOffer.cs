using Content.Shared.OfferingItem;
using Content.Server.OfferingItem;
using Content.Shared.Alert;
using JetBrains.Annotations;

namespace Content.Server.Alert.Click;

/// <summary>
/// Acceptance of the offer
/// </summary>
[UsedImplicitly]
[DataDefinition]
public sealed partial class AcceptingOffer : IAlertClick
{
    public void AlertClicked(EntityUid player)
    {
        var entManager = IoCManager.Resolve<IEntityManager>();

        if (entManager.TryGetComponent(player, out OfferingItemComponent? offeringItem))
        {
            entManager.System<OfferingItemSystem>().Receiving(player, offeringItem);
        }
    }
}
