using Content.Server.Anomaly.Components;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared.Anomaly.Components;
using Content.Shared.Mobs;
using Content.Shared.Psionics.Glimmer;

namespace Content.Server.Psionics.Glimmer
{
    /// <summary>
    /// Handles structures which add/subtract glimmer.
    /// </summary>
    public sealed class GlimmerStructuresSystem : EntitySystem
    {
        [Dependency] private readonly PowerReceiverSystem _powerReceiverSystem = default!;
        [Dependency] private readonly GlimmerSystem _glimmerSystem = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<AnomalyVesselComponent, PowerChangedEvent>(OnAnomalyVesselPowerChanged);

            SubscribeLocalEvent<GlimmerSourceComponent, AnomalyPulseEvent>(OnAnomalyPulse);
            SubscribeLocalEvent<GlimmerSourceComponent, AnomalySupercriticalEvent>(OnAnomalySupercritical);
            SubscribeLocalEvent<GlimmerSourceComponent, MobStateChangedEvent>(OnMobStateChanged);
        }

        private void OnAnomalyVesselPowerChanged(EntityUid uid, AnomalyVesselComponent component, ref PowerChangedEvent args)
        {
            if (TryComp<GlimmerSourceComponent>(component.Anomaly, out var glimmerSource))
                glimmerSource.Active = args.Powered;
        }

        private void OnAnomalyPulse(EntityUid uid, GlimmerSourceComponent component, ref AnomalyPulseEvent args)
        {
            // Anomalies are meant to have GlimmerSource on them with the
            // active flag set to false, as they will be set to actively
            // generate glimmer when scanned to an anomaly vessel for
            // harvesting research points.
            //
            // It is not a bug that glimmer increases on pulse or
            // supercritical with an inactive glimmer source.
            //
            // However, this will need to be reworked if a distinction
            // needs to be made in the future. I suggest a GlimmerAnomaly
            // component.

            if (TryComp<AnomalyComponent>(uid, out var anomaly))
                _glimmerSystem.DeltaGlimmerInput(5f * anomaly.Severity);
        }

        private void OnAnomalySupercritical(EntityUid uid, GlimmerSourceComponent component, ref AnomalySupercriticalEvent args)
        {
            _glimmerSystem.DeltaGlimmerOutput(100);
        }

        private void OnMobStateChanged(EntityUid uid, GlimmerSourceComponent component, ref MobStateChangedEvent args)
        {
            if (args.NewMobState != MobState.Alive)
                component.Active = false;
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);
            foreach (var source in EntityQuery<GlimmerSourceComponent>())
            {
                if (!_powerReceiverSystem.IsPowered(source.Owner))
                    continue;

                if (!source.Active)
                    continue;

                source.Accumulator += frameTime;

                if (source.Accumulator > source.SecondsPerGlimmer)
                {
                    var glimmerEquilibrium = GlimmerSystem.GlimmerEquilibrium;
                    source.Accumulator -= source.SecondsPerGlimmer;

                    // Shorthand explanation:
                    // This makes glimmer far more "Swingy", by making both positive and negative glimmer sources scale quite dramatically with glimmer
                    if (source.AddToGlimmer)
                    {
                        _glimmerSystem.DeltaGlimmerInput((_glimmerSystem.GlimmerOutput > glimmerEquilibrium ? _glimmerSystem.GetGlimmerOutputInteger() : 1f)
                        * (_glimmerSystem.GlimmerOutput < glimmerEquilibrium ? _glimmerSystem.GetGlimmerEquilibriumRatio() : 1f));
                    }
                    else
                    {
                        _glimmerSystem.DeltaGlimmerInput(-(_glimmerSystem.GlimmerOutput > glimmerEquilibrium ? _glimmerSystem.GetGlimmerOutputInteger() : 1f)
                        * (_glimmerSystem.GlimmerOutput > glimmerEquilibrium ? _glimmerSystem.GetGlimmerEquilibriumRatio() : 1f));
                    }
                }
            }
        }
    }
}
