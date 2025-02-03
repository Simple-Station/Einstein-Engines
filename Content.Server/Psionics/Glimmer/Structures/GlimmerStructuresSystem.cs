using Content.Server.Anomaly.Components;
using Content.Server.Power.EntitySystems;
using Content.Server.Research.Components;
using Content.Shared.Anomaly.Components;
using Content.Shared.Mobs;
using Content.Shared.Psionics.Glimmer;
using Content.Shared.Power;

namespace Content.Server.Psionics.Glimmer;

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
        SubscribeLocalEvent<GlimmerSourceComponent, ComponentStartup>(OnInit);
    }

    private void OnInit(EntityUid uid, GlimmerSourceComponent component, ComponentStartup args)
    {
        if (component.ResearchPointGeneration != null)
        {
            EnsureComp<ResearchPointSourceComponent>(uid, out var points);
            points.PointsPerSecond = component.ResearchPointGeneration.Value;
            points.Active = true;
        }
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
            _glimmerSystem.DeltaGlimmerOutput(5f * anomaly.Severity);
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
        var glimmerSources = Count<GlimmerSourceComponent>();
        foreach (var source in EntityQuery<GlimmerSourceComponent>())
        {
            if (!_powerReceiverSystem.IsPowered(source.Owner)
                && source.RequiresPower)
            {
                glimmerSources--;
                continue;
            }

            if (!source.Active)
            {
                glimmerSources--;
                continue;
            }

            source.Accumulator += frameTime;

            if (source.Accumulator > source.SecondsPerGlimmer)
            {
                source.Accumulator -= source.SecondsPerGlimmer;

                // https://www.desmos.com/calculator/zjzefpue03
                // In Short: 1 prober makes 20 research points. 4 probers makes twice as many points as 1 prober. 9 probers makes 69 points in total between all 9.
                // This is then modified by afterwards by GlimmerEquilibrium, to help smooth out the curves. But also, now if you have more drainers than probers, the probers won't generate research!
                // Also, this counts things like Anomalies & Glimmer Mites! Which means scientists should be more encouraged to actively hunt mites.
                // As a fun novelty, this means that a highly psionic Epistemics department can essentially "Study" their powers for actual research points!
                if (source.ResearchPointGeneration != null
                && TryComp<ResearchPointSourceComponent>(source.Owner, out var research))
                    research.PointsPerSecond = (int) MathF.Round(
                        source.ResearchPointGeneration.Value
                        / (MathF.Log(glimmerSources, 4) + 1)
                        * _glimmerSystem.GetGlimmerEquilibriumRatio());

                // Shorthand explanation:
                // This makes glimmer far more "Swingy", by making both positive and negative glimmer sources scale quite dramatically with glimmer
                if (!_glimmerSystem.GetGlimmerEnabled())
                    return;

                var glimmerEquilibrium = GlimmerSystem.GlimmerEquilibrium;

                if (source.AddToGlimmer)
                {
                    _glimmerSystem.DeltaGlimmerInput((_glimmerSystem.GlimmerOutput > glimmerEquilibrium
                    ? MathF.Pow(_glimmerSystem.GetGlimmerOutputInteger() - source.GlimmerExponentOffset + glimmerSources, 2) : 1f)
                    * (_glimmerSystem.GlimmerOutput < glimmerEquilibrium ? _glimmerSystem.GetGlimmerEquilibriumRatio() : 1f));
                }
                else
                {
                    _glimmerSystem.DeltaGlimmerInput(-(_glimmerSystem.GlimmerOutput > glimmerEquilibrium
                    ? MathF.Pow(_glimmerSystem.GetGlimmerOutputInteger() - source.GlimmerExponentOffset + glimmerSources, 2) : 1f)
                    * (_glimmerSystem.GlimmerOutput > glimmerEquilibrium ? _glimmerSystem.GetGlimmerEquilibriumRatio() : 1f));
                }
            }
        }
    }
}
