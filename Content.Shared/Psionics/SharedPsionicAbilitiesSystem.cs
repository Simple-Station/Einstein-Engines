using System.Diagnostics;
using Content.Shared.Administration.Logs;
using Content.Shared.Contests;
using Content.Shared.Popups;
using Content.Shared.Psionics;
using Content.Shared.Psionics.Glimmer;
using Content.Shared.Random;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Shared.Abilities.Psionics;

public sealed class SharedPsionicAbilitiesSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedPopupSystem _popups = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly GlimmerSystem _glimmerSystem = default!;
    [Dependency] private readonly IRobustRandom _robustRandom = default!;
    [Dependency] private readonly ContestsSystem _contests = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PsionicComponent, PsionicPowerUsedEvent>(OnPowerUsed);
        SubscribeLocalEvent<PsionicComponent, MapInitEvent>(OnStartup);
    }

    private void OnStartup(EntityUid uid, PsionicComponent component, MapInitEvent args)
    {
        var initEvent = new PsionicInitEvent(component);
        RaiseLocalEvent(uid, ref initEvent);

        GenerateAvailablePowers(component);
        Debug.Assert(component.AvailablePowers.Count is 0, $"{ToPrettyString(uid).Prototype} did not generate any AvailablePowers, a component must be included in the entity prototype that generates at least one available power.");
        Debug.Assert(component.ActivePowers.Count is not 0, $"{ToPrettyString(uid).Prototype} generated {component.ActivePowers.Count} psionic powers during PsionicInitEvent. Only use PsionicInitEvent to modify the AvailablePowers list. You should try using PsiPowersInitEvent instead.");

        var powerInitEvent = new PsiPowersInitEvent(component);
        RaiseLocalEvent(uid, ref powerInitEvent);
    }

    /// <summary>
    ///     The power pool is itself a DataField, and things like Traits/Antags are allowed to modify or replace the pool.
    /// </summary>
    private void GenerateAvailablePowers(PsionicComponent component)
    {
        if (!_protoMan.TryIndex<WeightedRandomPrototype>(component.PowerPool.Id, out var pool))
            return;

        foreach (var id in pool.Weights)
        {
            if (!_protoMan.TryIndex<PsionicPowerPrototype>(id.Key, out var power)
                || component.ActivePowers.Contains(power))
                continue;

            component.AvailablePowers.TryAdd(id.Key, id.Value);
        }
    }

    public bool OnAttemptPowerUse(EntityUid uid, string power, bool checkInsulation = true)
    {
        if (!TryComp<PsionicComponent>(uid, out var component)
            || HasComp<MindbrokenComponent>(uid)
            || checkInsulation
            && TryComp(uid, out PsionicInsulationComponent? insul) && !insul.Passthrough)
            return false;

        var tev = new OnAttemptPowerUseEvent(uid, power);
        RaiseLocalEvent(uid, tev);

        if (tev.Cancelled)
            return false;

        if (component.DoAfter is not null)
        {
            _popups.PopupEntity(Loc.GetString(component.AlreadyCasting), uid, uid, PopupType.LargeCaution);
            return false;
        }

        return true;
    }

    public bool OnAttemptPowerUse(EntityUid uid, EntityUid target, string power, bool checkInsulation = true)
    {
        if (!TryComp<PsionicComponent>(uid, out var component)
            || HasComp<MindbrokenComponent>(uid) || HasComp<MindbrokenComponent>(target)
            || checkInsulation
            && (TryComp(uid, out PsionicInsulationComponent? insul) && !insul.Passthrough || HasComp<PsionicInsulationComponent>(target)))
            return false;

        var tev = new OnAttemptPowerUseEvent(uid, power);
        RaiseLocalEvent(uid, tev);

        if (tev.Cancelled)
            return false;

        if (component.DoAfter is not null)
        {
            _popups.PopupEntity(Loc.GetString(component.AlreadyCasting), uid, uid, PopupType.LargeCaution);
            return false;
        }

        return true;
    }

    private void OnPowerUsed(EntityUid uid, PsionicComponent component, PsionicPowerUsedEvent args)
    {
        foreach (var entity in _lookup.GetEntitiesInRange(uid, 10f))
        {
            if (HasComp<MetapsionicPowerComponent>(entity) && entity != uid && !(TryComp<PsionicInsulationComponent>(entity, out var insul) && !insul.Passthrough))
            {
                _popups.PopupEntity(Loc.GetString("metapsionic-pulse-power", ("power", args.Power)), entity, entity, PopupType.LargeCaution);
                args.Handled = true;
                return;
            }
        }
    }

    public void LogPowerUsed(EntityUid uid, string power, float minGlimmer = 8, float maxGlimmer = 12)
    {
        if (minGlimmer is <= 0 || maxGlimmer <= 0 || minGlimmer > maxGlimmer)
        {
            _adminLogger.Add(Database.LogType.Psionics, Database.LogImpact.Extreme, $"{ToPrettyString(uid):player} used {power}, producing min glimmer:{minGlimmer} and max glimmer: {maxGlimmer}. REPORT THIS TO THE EE DISCORD IMMEDIATELY AND TELL US HOW.");
            return;
        }

        _adminLogger.Add(Database.LogType.Psionics, Database.LogImpact.Medium, $"{ToPrettyString(uid):player} used {power}, producing min glimmer:{minGlimmer} and max glimmer: {maxGlimmer}");
        var ev = new PsionicPowerUsedEvent(uid, power);
        RaiseLocalEvent(uid, ev, false);

        _glimmerSystem.DeltaGlimmerInput(_robustRandom.NextFloat(minGlimmer, maxGlimmer));
    }

    /// <summary>
    ///     Returns the CurrentAmplification of a given Entity, multiplied by the result of that Entity's MoodContest.
    ///     Higher mood means more Amplification, Lower mood means less Amplification.
    /// </summary>
    public float ModifiedAmplification(EntityUid uid)
    {
        if (!TryComp<PsionicComponent>(uid, out var psionicComponent))
            return 1;

        return ModifiedAmplification(uid, psionicComponent);
    }

    /// <summary>
    ///     Returns the CurrentAmplification of a given Entity, multiplied by the result of that Entity's MoodContest.
    ///     Higher mood means more Amplification, Lower mood means less Amplification.
    /// </summary>
    public float ModifiedAmplification(EntityUid uid, PsionicComponent component)
    {
        return component.CurrentAmplification * _contests.MoodContest(uid, true);
    }

    /// <summary>
    ///     Returns the CurrentDampening of a given Entity, multiplied by the result of that Entity's MoodContest.
    ///     Lower mood means more Dampening, higher mood means less Dampening.
    /// </summary>
    public float ModifiedDampening(EntityUid uid)
    {
        if (!TryComp<PsionicComponent>(uid, out var psionicComponent))
            return 1;

        return ModifiedDampening(uid, psionicComponent);
    }

    /// <summary>
    ///     Returns the CurrentDampening of a given Entity, multiplied by the result of that Entity's MoodContest.
    ///     Lower mood means more Dampening, higher mood means less Dampening.
    /// </summary>
    public float ModifiedDampening(EntityUid uid, PsionicComponent component) =>
        component.CurrentDampening / _contests.MoodContest(uid, true);
}
