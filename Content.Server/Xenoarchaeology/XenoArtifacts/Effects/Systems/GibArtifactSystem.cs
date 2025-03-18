using Content.Server.Xenoarchaeology.XenoArtifacts.Effects.Components;
using Content.Server.Xenoarchaeology.XenoArtifacts.Events;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Damage;
using Content.Shared.FixedPoint;

namespace Content.Server.Xenoarchaeology.XenoArtifacts.Effects.Systems;

public sealed class GibArtifactSystem : EntitySystem
{
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<GibArtifactComponent, ArtifactActivatedEvent>(OnActivate);
    }

    private void OnActivate(EntityUid uid, GibArtifactComponent component, ArtifactActivatedEvent args)
    {
        if (args.Activator is null)
            return;

        if (TryComp<BodyComponent>(args.Activator, out var _)
        && TryComp<DamageableComponent>(args.Activator, out var damageable))
        {
            // it's gibbing time
            _damageableSystem.SetAllDamage((EntityUid) args.Activator, damageable, (FixedPoint2) 10000f);
            // the one bellow crashes the server when the activator is a skeleton
            //_body.GibBody((EntityUid) args.Activator, gibOrgans: true, body, launchGibs: true);
        }
    }
}
