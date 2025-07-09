using Content.Server.Body.Components;
using Content.Server.Limbus.Traits.Components;
using Content.Server.Vampiric;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;

namespace Content.Server.Limbus.Traits;

public sealed class VampirismSystem : EntitySystem
{
    [Dependency] private readonly SharedBodySystem _body = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<VampirismComponent, MapInitEvent>(OnInitVampire);
    }

    private void OnInitVampire(Entity<VampirismComponent> ent, ref MapInitEvent args)
    {
        EnsureBloodSucker(ent);

        if (!TryComp<BodyComponent>(ent, out var body)
		    || !_body.TryGetBodyOrganEntityComps<MetabolizerComponent>((ent, body), out var comps))
            return;

        foreach (var entity in comps)
        {
            if (!TryComp<StomachComponent>(entity.Owner, out var stomach))
                continue;

            entity.Comp1.MetabolizerTypes = ent.Comp.MetabolizerPrototypes;

            if (ent.Comp.SpecialDigestible is {} whitelist)
                stomach.SpecialDigestible = whitelist;
        }
    }

    private void EnsureBloodSucker(Entity<VampirismComponent> uid)
    {
        if (HasComp<BloodSuckerComponent>(uid))
            return;

        AddComp(uid, new BloodSuckerComponent
        {
            Delay = uid.Comp.SuccDelay,
            InjectWhenSucc = false, // The code for it is deprecated, might wanna make it inject something when (if?) it gets reworked
            UnitsToSucc = uid.Comp.UnitsToSucc,
            WebRequired = false
        });
    }
}
