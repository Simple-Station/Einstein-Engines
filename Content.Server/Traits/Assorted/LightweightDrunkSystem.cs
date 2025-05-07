using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Traits.Assorted.Components;

namespace Content.Shared.Traits.Assorted.Systems;
public sealed class LightweightDrunkSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<LightweightDrunkComponent, TryMetabolizeReagent>(OnTryMetabolizeReagent);
    }

    private void OnTryMetabolizeReagent(EntityUid uid, LightweightDrunkComponent comp, ref TryMetabolizeReagent args)
    {
        //Log.Debug(args.Prototype.ID);
        if (args.Prototype.ID != "Ethanol")
            return;

        args.Scale *= comp.BoozeStrengthMultiplier;
        args.QuantityMultiplier *= comp.BoozeStrengthMultiplier;
    }
}