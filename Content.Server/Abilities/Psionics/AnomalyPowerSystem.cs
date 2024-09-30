using Content.Shared.Abilities.Psionics;
using Content.Shared.Actions.Events;
using Content.Shared.Psionics.Glimmer;
using Content.Shared.Random.Helpers;
using Robust.Shared.Random;
using Content.Shared.Anomaly.Effects.Components;
using Robust.Shared.Map.Components;
using Content.Shared.Anomaly;
using Robust.Shared.Audio.Systems;

namespace Content.Server.Abilities.Psionics;

public sealed class AnomalyPowerSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly GlimmerSystem _glimmerSystem = default!;
    [Dependency] private readonly SharedAnomalySystem _anomalySystem = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPsionicAbilitiesSystem _psionics = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PsionicComponent, AnomalyPowerActionEvent>(OnPowerUsed);
    }

    private void OnPowerUsed(EntityUid uid, PsionicComponent component, AnomalyPowerActionEvent args)
    {
        if (HasComp<PsionicInsulationComponent>(uid)
            || HasComp<MindbrokenComponent>(uid))
            return;

        var overcharged = _glimmerSystem.Glimmer > args.SupercriticalThreshold * component.CurrentAmplification / component.CurrentDampening;

        // I already hate this, so much.
        //DoBluespaceAnomalyEffects(uid, component, args, overcharged);
        //DoElectricityAnomalyEffects(uid, component, args, overcharged);
        DoEntityAnomalyEffects(uid, component, args, overcharged);
        //DoExplosionAnomalyEffects(uid, component, args, overcharged);
        //DoGasProducerAnomalyEffects(uid, component, args, overcharged);
        //DoGravityAnomalyEffects(uid, component, args, overcharged);
        //DoInjectionAnomalyEffects(uid, component, args, overcharged);
        //DoPuddleCreateAnomalyEffects(uid, component, args, overcharged);
        //DoPyroclasticAnomalyEffects(uid, component, args, overcharged);
        //DoTemperatureAnomalyEffects(uid, component, args, overcharged);

        DoAnomalySounds(uid, component, args, overcharged);
        DoGlimmerEffects(uid, component, args, overcharged);

        args.Handled = true;
    }

    public void DoEntityAnomalyEffects(EntityUid uid, PsionicComponent component, AnomalyPowerActionEvent args, bool overcharged)
    {
        if (args.EntitySpawnEntries is null)
            return;

        if (overcharged)
            foreach (var entry in args.EntitySpawnEntries)
            {
                if (!entry.Settings.SpawnOnSuperCritical)
                    continue;

                SpawnEntities(uid, component, entry);
            }
        else foreach (var entry in args.EntitySpawnEntries)
            {
                if (!entry.Settings.SpawnOnPulse)
                    continue;

                SpawnEntities(uid, component, entry);
            }
    }

    public void SpawnEntities(EntityUid uid, PsionicComponent component, EntitySpawnSettingsEntry entry)
    {
        if (!TryComp<MapGridComponent>(Transform(uid).GridUid, out var grid))
            return;

        var tiles = _anomalySystem.GetSpawningPoints(uid,
                        component.CurrentDampening,
                        component.CurrentAmplification,
                        entry.Settings,
                        _glimmerSystem.Glimmer / 1000,
                        component.CurrentAmplification,
                        component.CurrentAmplification);

        if (tiles is null)
            return;

        foreach (var tileref in tiles)
        {
            Spawn(_random.Pick(entry.Spawns), _mapSystem.ToCenterCoordinates(tileref, grid));
        }
    }

    public void DoAnomalySounds(EntityUid uid, PsionicComponent component, AnomalyPowerActionEvent args, bool overcharged = false)
    {
        if (overcharged && args.SupercriticalSound is not null)
        {
            _audio.PlayPvs(args.SupercriticalSound, uid);
            return;
        }

        if (args.PulseSound is null
            || _glimmerSystem.Glimmer < args.GlimmerSoundThreshold * component.CurrentDampening)
            return;

        _audio.PlayEntity(args.PulseSound, uid, uid);
    }

    public void DoGlimmerEffects(EntityUid uid, PsionicComponent component, AnomalyPowerActionEvent args, bool overcharged = false)
    {
        var minGlimmer = (int) Math.Round(MathF.MinMagnitude(args.MinGlimmer, args.MaxGlimmer)
            * (overcharged ? args.SupercriticalGlimmerMultiplier : 1)
            * component.CurrentAmplification - component.CurrentDampening);
        var maxGlimmer = (int) Math.Round(MathF.MaxMagnitude(args.MinGlimmer, args.MaxGlimmer)
            * (overcharged ? args.SupercriticalGlimmerMultiplier : 1)
            * component.CurrentAmplification - component.CurrentDampening);

        _psionics.LogPowerUsed(uid, args.PowerName, minGlimmer, maxGlimmer);
    }
}
