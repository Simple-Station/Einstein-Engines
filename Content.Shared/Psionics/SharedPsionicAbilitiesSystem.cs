using Content.Shared.Administration.Logs;
using Content.Shared.Contests;
using Content.Shared.Popups;
using Content.Shared.Psionics;
using Content.Shared.Psionics.Glimmer;
using Robust.Shared.Random;
using Robust.Shared.Serialization;
using Content.Shared.Mobs.Systems;
using Content.Shared.FixedPoint;
using Content.Shared.Rejuvenate;

namespace Content.Shared.Abilities.Psionics
{
    public sealed class SharedPsionicAbilitiesSystem : EntitySystem
    {
        [Dependency] private readonly EntityLookupSystem _lookup = default!;
        [Dependency] private readonly SharedPopupSystem _popups = default!;
        [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
        [Dependency] private readonly GlimmerSystem _glimmerSystem = default!;
        [Dependency] private readonly IRobustRandom _robustRandom = default!;
        [Dependency] private readonly ContestsSystem _contests = default!;
        [Dependency] private readonly MobStateSystem _mobState = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<PsionicComponent, PsionicPowerUsedEvent>(OnPowerUsed);
        }

        public bool OnAttemptPowerUse(EntityUid uid, string power, bool checkInsulation = true)
        {
            if (!TryComp<PsionicComponent>(uid, out var component)
                || HasComp<MindbrokenComponent>(uid)
                || checkInsulation
                && HasComp<PsionicInsulationComponent>(uid))
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
            _adminLogger.Add(Database.LogType.Psionics, Database.LogImpact.Medium, $"{ToPrettyString(uid):player} used {power}");
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

    public sealed class PsionicPowerUsedEvent : HandledEntityEventArgs
    {
        public EntityUid User { get; }
        public string Power;

        public PsionicPowerUsedEvent(EntityUid user, string power)
        {
            User = user;
            Power = power;
        }
    }

    public sealed class OnAttemptPowerUseEvent : CancellableEntityEventArgs
    {
        public EntityUid User { get; }
        public string Power = string.Empty;

        public OnAttemptPowerUseEvent(EntityUid user, string power)
        {
            User = user;
            Power = power;
        }
    }

    [Serializable]
    [NetSerializable]
    public sealed class PsionicsChangedEvent : EntityEventArgs
    {
        public readonly NetEntity Euid;
        public PsionicsChangedEvent(NetEntity euid)
        {
            Euid = euid;
        }
    }
}
