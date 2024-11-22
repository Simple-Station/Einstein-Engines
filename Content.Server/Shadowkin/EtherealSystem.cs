using Content.Shared.Eye;
using Content.Shared.Shadowkin;
using Robust.Server.GameObjects;
using Content.Server.Atmos.Components;
using Content.Server.Temperature.Components;
using Content.Shared.Movement.Components;
using Content.Shared.Stealth;
using Content.Shared.Stealth.Components;
using Content.Server.Body.Components;
using Content.Server.NPC.Components;
using Content.Server.NPC.Systems;
using System.Linq;
using Content.Shared.Abilities.Psionics;
using Robust.Shared.Random;
using Content.Server.Light.Components;

namespace Content.Server.Shadowkin;

public sealed class EtherealSystem : SharedEtherealSystem
{
    [Dependency] private readonly VisibilitySystem _visibilitySystem = default!;
    [Dependency] private readonly SharedStealthSystem _stealth = default!;
    [Dependency] private readonly EyeSystem _eye = default!;
    [Dependency] private readonly NpcFactionSystem _factions = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedPointLightSystem _light = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void OnStartup(EntityUid uid, EtherealComponent component, MapInitEvent args)
    {
        base.OnStartup(uid, component, args);

        var visibility = EnsureComp<VisibilityComponent>(uid);
        _visibilitySystem.RemoveLayer((uid, visibility), (int) VisibilityFlags.Normal, false);
        _visibilitySystem.AddLayer((uid, visibility), (int) VisibilityFlags.Ethereal, false);
        _visibilitySystem.RefreshVisibility(uid, visibility);

        if (TryComp<EyeComponent>(uid, out var eye))
            _eye.SetVisibilityMask(uid, eye.VisibilityMask | (int) (VisibilityFlags.Ethereal), eye);

        if (TryComp<TemperatureComponent>(uid, out var temp))
            temp.AtmosTemperatureTransferEfficiency = 0;

        var stealth = EnsureComp<StealthComponent>(uid);
        _stealth.SetVisibility(uid, 0.8f, stealth);

        SuppressFactions(uid, component, true);

        EnsureComp<PressureImmunityComponent>(uid);
        EnsureComp<RespiratorImmuneComponent>(uid);
        EnsureComp<MovementIgnoreGravityComponent>(uid);

        if (HasComp<MindbrokenComponent>(uid))
            RemComp(uid, component);
    }

    public override void OnShutdown(EntityUid uid, EtherealComponent component, ComponentShutdown args)
    {
        base.OnShutdown(uid, component, args);

        if (TryComp<VisibilityComponent>(uid, out var visibility))
        {
            _visibilitySystem.AddLayer((uid, visibility), (int) VisibilityFlags.Normal, false);
            _visibilitySystem.RemoveLayer((uid, visibility), (int) VisibilityFlags.Ethereal, false);
            _visibilitySystem.RefreshVisibility(uid, visibility);
        }

        if (TryComp<EyeComponent>(uid, out var eye))
            _eye.SetVisibilityMask(uid, (int) VisibilityFlags.Normal, eye);

        if (TryComp<TemperatureComponent>(uid, out var temp))
            temp.AtmosTemperatureTransferEfficiency = 0.1f;

        SuppressFactions(uid, component, false);

        RemComp<StealthComponent>(uid);
        RemComp<PressureImmunityComponent>(uid);
        RemComp<RespiratorImmuneComponent>(uid);
        RemComp<MovementIgnoreGravityComponent>(uid);

        SpawnAtPosition("ShadowkinShadow", Transform(uid).Coordinates);
        SpawnAtPosition("EffectFlashShadowkinDarkSwapOff", Transform(uid).Coordinates);

        foreach (var light in component.DarkenedLights.ToArray())
        {
            if (!TryComp<PointLightComponent>(light, out var pointLight)
                || !TryComp<EtherealLightComponent>(light, out var etherealLight))
                continue;

            ResetLight(light, pointLight, etherealLight);
        }
    }

    public void SuppressFactions(EntityUid uid, EtherealComponent component, bool set)
    {
        if (set)
        {
            if (!TryComp<NpcFactionMemberComponent>(uid, out var factions))
                return;

            component.SuppressedFactions = factions.Factions.ToList();

            foreach (var faction in factions.Factions)
                _factions.RemoveFaction(uid, faction);
        }
        else
        {
            foreach (var faction in component.SuppressedFactions)
                _factions.AddFaction(uid, faction);

            component.SuppressedFactions.Clear();
        }
    }

    public void ResetLight(EntityUid uid, PointLightComponent light, EtherealLightComponent etherealLight)
    {
        etherealLight.AttachedEntity = EntityUid.Invalid;

        if (etherealLight.OldRadiusEdited)
            _light.SetRadius(uid, etherealLight.OldRadius);
        etherealLight.OldRadiusEdited = false;

        if (etherealLight.OldEnergyEdited)
            _light.SetEnergy(uid, etherealLight.OldEnergy);
        etherealLight.OldEnergyEdited = false;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<EtherealComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (!component.Darken)
                continue;

            component.DarkenAccumulator += frameTime;

            if (component.DarkenAccumulator <= 1)
                continue;

            component.DarkenAccumulator -= component.DarkenRate;

            var darkened = new List<EntityUid>();
            var lightQuery = _lookup.GetEntitiesInRange(uid, component.DarkenRange, flags: LookupFlags.StaticSundries)
                .Where(x => HasComp<EtherealLightComponent>(x) && HasComp<PointLightComponent>(x));

            foreach (var entity in lightQuery)
                if (!darkened.Contains(entity))
                    darkened.Add(entity);

            _random.Shuffle(darkened);
            component.DarkenedLights = darkened;

            var playerPos = _transform.GetWorldPosition(uid);

            foreach (var light in component.DarkenedLights.ToArray())
            {
                var lightPos = _transform.GetWorldPosition(light);
                if (!TryComp<PointLightComponent>(light, out var pointLight)
                    || !TryComp<EtherealLightComponent>(light, out var etherealLight))
                    continue;

                if (TryComp<PoweredLightComponent>(light, out var powered) && !powered.On)
                {
                    ResetLight(light, pointLight, etherealLight);
                    continue;
                }

                if (etherealLight.AttachedEntity == EntityUid.Invalid)
                    etherealLight.AttachedEntity = uid;

                if (etherealLight.AttachedEntity != EntityUid.Invalid
                && etherealLight.AttachedEntity != uid)
                {
                    component.DarkenedLights.Remove(light);
                    continue;
                }

                if (etherealLight.AttachedEntity == uid
                    && _random.Prob(0.03f))
                        etherealLight.AttachedEntity = EntityUid.Invalid;

                if (!etherealLight.OldRadiusEdited)
                {
                    etherealLight.OldRadius = pointLight.Radius;
                    etherealLight.OldRadiusEdited = true;
                }
                if (!etherealLight.OldEnergyEdited)
                {
                    etherealLight.OldEnergy = pointLight.Energy;
                    etherealLight.OldEnergyEdited = true;
                }

                var distance = (lightPos - playerPos).Length();
                var radius = distance * 2f;
                var energy = distance * 0.8f;

                if (etherealLight.OldRadiusEdited && radius > etherealLight.OldRadius)
                    radius = etherealLight.OldRadius;
                if (etherealLight.OldRadiusEdited && radius < etherealLight.OldRadius * 0.20f)
                    radius = etherealLight.OldRadius * 0.20f;

                if (etherealLight.OldEnergyEdited && energy > etherealLight.OldEnergy)
                    energy = etherealLight.OldEnergy;
                if (etherealLight.OldEnergyEdited && energy < etherealLight.OldEnergy * 0.20f)
                    energy = etherealLight.OldEnergy * 0.20f;

                _light.SetRadius(light, radius);
                _light.SetEnergy(light, energy);
            }
        }
    }
}