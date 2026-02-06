using System.Linq;
using System.Numerics;
using System.Text;
using Content.Server.Chat.Systems;
using Content.Server.Singularity.Components;
using Content.Server.RoundEnd;
using Content.Shared._EE.CCVars;
using Content.Shared._EE.Supermatter.Components;
using Content.Shared.Atmos;
using Content.Shared.Audio;
using Content.Shared.Chat;
using Content.Shared.DeviceLinking;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Physics;
using Content.Shared.Radiation.Components;
using Content.Shared.Silicons.Laws.Components;
using Content.Shared.Speech;
using Content.Shared.Storage.Components;
using Content.Shared.GameTicking;
using Content.Shared.GameTicking.Components;
using Content.Shared.Traits.Assorted;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Spawners;
using Content.Shared.Traits.Assorted.Components;

namespace Content.Server._EE.Supermatter.Systems;

public sealed partial class SupermatterSystem
{
    /// <summary>
    /// Logging the first launch of supermatter
    /// <summary>
    private bool CheckFirstPower(EntityUid uid, SupermatterComponent sm, GasMixture mix)
    {
        if (sm.Power > 0 && !sm.HasBeenPowered)
        {
            LogFirstPower(uid, sm, mix);
        }

        return sm.HasBeenPowered;
    }


    /// <summary>
    /// The logic of supermatter working with gases.
    /// </summary>
    private void ProcessAtmos(EntityUid uid, SupermatterComponent sm, float frameTime)
    {
        var mix = _atmosphere.GetContainingMixture(uid, true, true);
        if (mix is not { })
            return;

        if (!sm.HasBeenPowered)
            return;

        sm.GasStorage = mix.Remove(sm.GasEfficiency * mix.TotalMoles);

        var moles = sm.GasStorage.TotalMoles;
        if (!(moles > 0f))
            return;

        var gasComposition = sm.GasStorage.Clone();

        foreach (Gas gasId in Enum.GetValues(typeof(Gas)))
        {
            var proportion = sm.GasStorage.GetMoles(gasId) / moles;
            gasComposition.SetMoles(gasId, Math.Clamp(proportion, 0, 1));
        }

        var powerRatio = SupermatterGasData.GetPowerMixRatios(gasComposition);
        sm.GasHeatModifier = SupermatterGasData.GetHeatPenalties(gasComposition);
        var transmissionBonus = SupermatterGasData.GetTransmitModifiers(gasComposition);

        var h2OBonus = 1 - gasComposition.GetMoles(Gas.WaterVapor) * 0.25f;

        powerRatio = Math.Clamp(powerRatio, 0, 1);
        sm.HeatModifier = Math.Max(sm.GasHeatModifier, 0.5f);
        transmissionBonus *= h2OBonus;

        var ammoniaProportion = gasComposition.GetMoles(Gas.Ammonia);

        if (ammoniaProportion > 0)
        {
            var ammoniaPartialPressure = mix.Pressure * ammoniaProportion;
            var consumedMiasma = Math.Clamp((ammoniaPartialPressure - _config.GetCVar(CCVars.SupermatterAmmoniaConsumptionPressure)) /
                (ammoniaPartialPressure + _config.GetCVar(CCVars.SupermatterAmmoniaPressureScaling)) *
                (1 + powerRatio * _config.GetCVar(CCVars.SupermatterAmmoniaGasMixScaling)),
                0f, 1f);

            consumedMiasma *= ammoniaProportion * moles;

            if (consumedMiasma > 0)
            {
                sm.GasStorage.AdjustMoles(Gas.Ammonia, -consumedMiasma);
                sm.MatterPower += consumedMiasma * _config.GetCVar(CCVars.SupermatterAmmoniaPowerGain);
            }
        }

        var heatResistance = SupermatterGasData.GetHeatResistances(gasComposition);
        sm.DynamicHeatResistance = Math.Max(heatResistance, 1);
        sm.MoleHeatPenaltyThreshold = (float)Math.Max(moles / _config.GetCVar(CCVars.SupermatterMoleHeatPenalty), 0.25);

        if (moles > _config.GetCVar(CCVars.SupermatterPowerlossInhibitionMoleThreshold) &&
            gasComposition.GetMoles(Gas.CarbonDioxide) > _config.GetCVar(CCVars.SupermatterPowerlossInhibitionGasThreshold))
        {
            var co2powerloss = Math.Clamp(gasComposition.GetMoles(Gas.CarbonDioxide) - sm.PowerlossDynamicScaling, -0.02f, 0.02f);
            sm.PowerlossDynamicScaling = Math.Clamp(sm.PowerlossDynamicScaling + co2powerloss, 0f, 1f);
        }
        else
            sm.PowerlossDynamicScaling = Math.Clamp(sm.PowerlossDynamicScaling - 0.05f, 0f, 1f);

        sm.PowerlossInhibitor = Math.Clamp(
            1 - sm.PowerlossDynamicScaling * Math.Clamp(moles / _config.GetCVar(CCVars.SupermatterPowerlossInhibitionMoleBoostThreshold), 1f, 1.5f),
            0f, 1f);

        if (sm.MatterPower != 0)
        {
            var removedMatter = Math.Max(sm.MatterPower / _config.GetCVar(CCVars.SupermatterMatterPowerConversion), 40);
            sm.Power = Math.Max(sm.Power + removedMatter, 0);
            sm.MatterPower = Math.Max(sm.MatterPower - removedMatter, 0);
        }

        var tempFactor = powerRatio > 0.8 ? 50f : 30f;
        sm.Power = Math.Max(sm.GasStorage.Temperature * tempFactor / Atmospherics.T0C * powerRatio + sm.Power, 0);

        var integrity = GetIntegrity(sm);
        var integrityRatio = Math.Clamp(integrity / 100f, 0f, 1f);
        var integrityRadModificator = MathF.Pow(1f - integrityRatio, 2f) * 5f;

        if (TryComp<RadiationSourceComponent>(uid, out var rad))
        {
            rad.Intensity =
                _config.GetCVar(CCVars.SupermatterRadsBase) +
                (sm.Power * Math.Max(0, 1f + transmissionBonus / 10f) * 0.003f + integrityRadModificator)
                * _config.GetCVar(CCVars.SupermatterRadsModifier);

            rad.Slope = Math.Clamp(rad.Intensity / 15, 0.2f, 1f);
        }

        var energy = sm.Power * _config.GetCVar(CCVars.SupermatterReactionPowerModifier) * (1f - sm.PsyCoefficient * 0.2f) * 2 * frameTime;
        var gasReleased = sm.GasStorage.Clone();

        gasReleased.Temperature += energy * sm.HeatModifier / _config.GetCVar(CCVars.SupermatterThermalReleaseModifier);
        gasReleased.Temperature = Math.Max(0,
            Math.Min(gasReleased.Temperature, 2500f * sm.HeatModifier));

        gasReleased.AdjustMoles(
            Gas.Plasma,
            Math.Max(energy * sm.HeatModifier / _config.GetCVar(CCVars.SupermatterPlasmaReleaseModifier), 0f));
        gasReleased.AdjustMoles(
            Gas.Oxygen,
            Math.Max((energy + gasReleased.Temperature * sm.HeatModifier - Atmospherics.T0C) / _config.GetCVar(CCVars.SupermatterOxygenReleaseModifier), 0f));

//        mix.Temperature = gasReleased.Temperature;
        _atmosphere.Merge(mix, gasReleased);

        var powerReduction = (float)Math.Pow(sm.Power / 500, 3);
        sm.PowerLoss = Math.Min(powerReduction * sm.PowerlossInhibitor, sm.Power * 0.60f * sm.PowerlossInhibitor);
        sm.Power = Math.Max(sm.Power - sm.PowerLoss, 0f);

        if (TryComp<GravityWellComponent>(uid, out var gravityWell))
            gravityWell.MaxRange = Math.Clamp(sm.Power / 850f, 0.5f, 3f);

        var SupermatterResonantFrequency = SupermatterGasData.GetResonantFrequency(gasComposition);
        sm.ResonantFrequency = SupermatterResonantFrequency;
    }

    /// <summary>
    /// Supermatter damage logic. SM takes damage when: 1) there are a lot of moles of gas. 2) there is a lot of energy. 3) there is too much temperature.
    /// </summary>
    private void HandleDamage(EntityUid uid, SupermatterComponent sm)
    {
        // While Supermatter not working, we don't have any damage.
        if (!sm.HasBeenPowered)
            return;

        var xform = Transform(uid);
        var gridId = xform.GridUid;

        sm.DamageArchived = sm.Damage;

        var mix = _atmosphere.GetContainingMixture(uid, true, true);

        // Vacuum damage.
        if (!xform.GridUid.HasValue || mix is not { } || mix.TotalMoles == 0f)
        {
            sm.Damage += Math.Max(sm.Power / 1000 * sm.DamageIncreaseMultiplier, 10f);
            return;
        }

        // AntiNob damage.
        if (_config.GetCVar(CCVars.SupermatterDoCascadeDelam) && sm.ResonantFrequency >= 1)
        {
            var integrity = GetIntegrity(sm);
            float NobliumDamage = Math.Max(sm.Power / 1000 * sm.DamageIncreaseMultiplier, integrity < 35 ? 15f : 25f);
            sm.Damage += NobliumDamage;
        }

        var GasEfficiency = sm.GasEfficiency;
        var absorbedGas = mix.Remove(GasEfficiency * mix.TotalMoles);
        var moles = absorbedGas.TotalMoles;

        var totalDamage = 0f;
        var tempThreshold = Atmospherics.T0C + _config.GetCVar(CCVars.SupermatterHeatPenaltyThreshold);

        var tempDamage = Math.Max(Math.Clamp(moles / 50f, .1f, .5f) * absorbedGas.Temperature - tempThreshold * sm.DynamicHeatResistance, 0f) *
            sm.MoleHeatPenaltyThreshold / 150f * sm.DamageIncreaseMultiplier;
        totalDamage += tempDamage;

        var powerDamage = Math.Max(sm.Power - _config.GetCVar(CCVars.SupermatterPowerPenaltyThreshold), 0f) / 500f * sm.DamageIncreaseMultiplier;
        totalDamage += powerDamage;

        var moleDamage = Math.Max(moles - _config.GetCVar(CCVars.SupermatterMolePenaltyThreshold), 0f) / 80 * sm.DamageIncreaseMultiplier;
        totalDamage += moleDamage;

        if (moles < _config.GetCVar(CCVars.SupermatterMolePenaltyThreshold))
        {
            sm.HeatHealing = Math.Min(absorbedGas.Temperature - (tempThreshold + 45f * sm.PsyCoefficient), 0f) / 150f;
            totalDamage += sm.HeatHealing;
        }
        else
            sm.HeatHealing = 0f;

        if (TryComp<MapGridComponent>(gridId, out var grid))
        {
            var localpos = xform.Coordinates.Position;
            var tilerefs = _map.GetLocalTilesIntersecting(
                gridId.Value,
                grid,
                new Box2(localpos + new Vector2(-1, -1), localpos + new Vector2(1, 1)),
                true);

            if (tilerefs.Count() < 9)
            {
                var factor = GetIntegrity(sm) switch
                {
                    < 10 => 0.0005f,
                    < 25 => 0.0009f,
                    < 45 => 0.005f,
                    < 75 => 0.002f,
                    _ => 0f
                };

                totalDamage += Math.Clamp(sm.Power * factor * sm.DamageIncreaseMultiplier, 0, sm.MaxSpaceExposureDamage);
            }
        }

        var damage = Math.Min(sm.DamageArchived + sm.DamageHardcap * sm.DamageDelaminationPoint, sm.Damage + totalDamage);
        sm.Damage = Math.Clamp(damage, 0, float.PositiveInfinity);

        if (TryComp<AppearanceComponent>(uid, out var appearance))
        {
            var visual = SupermatterCrystalState.Normal;
            if (totalDamage > 0)
            {
                visual = sm.Status switch
                {
                    SupermatterStatusType.Delaminating => SupermatterCrystalState.GlowDelam,
                    >= SupermatterStatusType.Emergency => SupermatterCrystalState.GlowEmergency,
                    _ => SupermatterCrystalState.Glow
                };
            }

            _appearance.SetData(uid, SupermatterVisuals.Crystal, visual, appearance);
        }

         // return's absorbed gases
        _atmosphere.Merge(mix, absorbedGas);
    }
    public DelamType ChooseDelamType(EntityUid uid, SupermatterComponent sm)
    {
        var station = _station.GetOwningStation(uid);
        var xform = Transform(uid);;

        if (station == null)
            return DelamType.Explosion;

        EntityUid stationId = (EntityUid)station;

        // Cascade Delam
        if (_config.GetCVar(CCVars.SupermatterDoCascadeDelam) && sm.ResonantFrequency >= 1)
        {
            _alert.SetLevel(stationId, sm.AlertCodeCascadeId, true, true, true, true);
            sm.Cascade = true;
            return DelamType.Cascade;
        }

        // Singularity Delam
        var mix = _atmosphere.GetContainingMixture(uid, true, true);

        if (mix is { })
        {
            var absorbedGas = mix.Remove(sm.GasEfficiency * mix.TotalMoles);
            var moles = absorbedGas.TotalMoles;

            if (moles >= _config.GetCVar(CCVars.SupermatterMolePenaltyThreshold))
            {
                _alert.SetLevel(stationId, sm.AlertCodeDeltaId, true, true, true, true);
                return DelamType.Singularity;
            }
        }

        // Tesla Delam
        if (sm.Power >= _config.GetCVar(CCVars.SupermatterSeverePowerPenaltyThreshold))
        {
            _alert.SetLevel(stationId, sm.AlertCodeDeltaId, true, true, true, true);
            return DelamType.Tesla;
        }

        // Base explosion
        _alert.SetLevel(stationId, sm.AlertCodeDeltaId, true, true, true, true);
        return DelamType.Explosion;
    }

    private void HandleDelamination(EntityUid uid, SupermatterComponent sm)
    {
        var xform = Transform(uid);

        sm.PreferredDelamType = ChooseDelamType(uid, sm);

        if (!sm.Delamming)
        {
            sm.Delamming = true;
            sm.DelamEndTime = _timing.CurTime + TimeSpan.FromSeconds(sm.DelamTimer);
            AnnounceCoreDamage(uid, sm);
        }

        if (sm.Damage < sm.DamageDelaminationPoint && sm.Delamming)
        {
            sm.Delamming = false;
            AnnounceCoreDamage(uid, sm);
        }

        if (_timing.CurTime < sm.DelamEndTime)
            return;

        var mapId = Transform(uid).MapID;
        var mapFilter = Filter.BroadcastMap(mapId);
        var message = Loc.GetString("supermatter-delam-player");
        var wrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", message));

        _chatManager.ChatMessageToManyFiltered(mapFilter,
            ChatChannel.Server,
            message,
            wrappedMessage,
            uid,
            false,
            true,
            Color.Red);

        _audio.PlayGlobal(sm.DistortSound, mapFilter, true);

        var mobLookup = new HashSet<Entity<MobStateComponent>>();
        _entityLookup.GetEntitiesOnMap<MobStateComponent>(mapId, mobLookup);
        mobLookup.RemoveWhere(x => HasComp<InsideEntityStorageComponent>(x));

        var paracusiaSounds = new SoundCollectionSpecifier("Paracusia");
        var paracusiaMinTime = 0.1f;
        var paracusiaMaxTime = 300f;
        var paracusiaDistance = 7f;

        foreach (var mob in mobLookup)
        {
            if (HasComp<SiliconLawBoundComponent>(uid))
                continue;

            if (!EnsureComp<ParacusiaComponent>(mob, out var paracusia))
            {
                _paracusia.SetSounds(mob, paracusiaSounds, paracusia);
                _paracusia.SetTime(mob, paracusiaMinTime, paracusiaMaxTime, paracusia);
                _paracusia.SetDistance(mob, paracusiaDistance, paracusia);
            }
        }

        switch (sm.PreferredDelamType)
        {
            case DelamType.Cascade:
                QueueDel(uid);
                Spawn(sm.SupermatterCascadePrototype, xform.Coordinates);
                Spawn(sm.KudzuPrototype, xform.Coordinates);
                _roundEnd.EndRound(sm.RestartDelay);
                break;

            case DelamType.Singularity:
                Spawn(sm.SingularityPrototype, xform.Coordinates);
                break;

            case DelamType.Tesla:
                Spawn(sm.TeslaPrototype, xform.Coordinates);
                break;

            default:
                _explosion.TriggerExplosive(uid);
                Spawn(sm.AfterExplosionRadiationPrototype, xform.Coordinates);

                var station = _station.GetOwningStation(uid);
                if (station.HasValue)
                {
                    var stationId = station.Value;
                    _alert.SetLevel(stationId, sm.AlertCodeYellowId, true, true, true, true);
                }
                break;
        }
    }

    /// <summary>
    /// Console stuff
    /// </summary>
    private void HandleStatus(EntityUid uid, SupermatterComponent sm)
    {
        var currentStatus = GetStatus(uid, sm);

        if (sm.Status != currentStatus && HasComp<DeviceLinkSourceComponent>(uid))
        {
            var port = currentStatus switch
            {
                SupermatterStatusType.Normal => sm.PortNormal,
                SupermatterStatusType.Caution => sm.PortCaution,
                SupermatterStatusType.Warning => sm.PortWarning,
                SupermatterStatusType.Danger => sm.PortDanger,
                SupermatterStatusType.Emergency => sm.PortEmergency,
                SupermatterStatusType.Delaminating => sm.PortDelaminating,
                _ => sm.PortInactive
            };

            _link.InvokePort(uid, port);
        }

        sm.Status = currentStatus;

        if (!TryComp<SpeechComponent>(uid, out var speech))
            return;

        if (sm.Damage < sm.DamageArchived && currentStatus != SupermatterStatusType.Delaminating)
        {
            sm.StatusCurrentSound = sm.StatusSilentSound;
            speech.SpeechSounds = sm.StatusSilentSound;
            return;
        }

        sm.StatusCurrentSound = currentStatus switch
        {
            SupermatterStatusType.Warning => sm.StatusWarningSound,
            SupermatterStatusType.Danger => sm.StatusDangerSound,
            SupermatterStatusType.Emergency => sm.StatusEmergencySound,
            SupermatterStatusType.Delaminating => sm.StatusDelamSound,
            _ => sm.StatusSilentSound
        };

        if (currentStatus == SupermatterStatusType.Warning)
            speech.AudioParams = AudioParams.Default.AddVolume(7.5f);
        else
            speech.AudioParams = AudioParams.Default.AddVolume(10f);

        speech.SpeechSounds = sm.StatusCurrentSound;
    }
}
