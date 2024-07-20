using System.Diagnostics.CodeAnalysis;
using Content.Shared.Actions;
using Content.Shared.Administration.Logs;
using Content.Shared.Examine;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Psionics.Glimmer;
using Robust.Shared.Network;
using Robust.Shared.Random;
using Robust.Shared.Serialization;

namespace Content.Shared.Psionics.Abilities
{
    public sealed class SharedPsionicAbilitiesSystem : EntitySystem
    {
        [Dependency] private readonly SharedActionsSystem _actions = default!;
        [Dependency] private readonly EntityLookupSystem _lookup = default!;
        [Dependency] private readonly SharedPopupSystem _popups = default!;
        [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
        [Dependency] private readonly GlimmerSystem _glimmerSystem = default!;
        [Dependency] private readonly IRobustRandom _robustRandom = default!;
        [Dependency] private readonly INetManager _net = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<PsionicsDisabledComponent, ComponentInit>(OnInit);
            SubscribeLocalEvent<PsionicsDisabledComponent, ComponentShutdown>(OnShutdown);
            SubscribeLocalEvent<PsionicComponent, PsionicPowerUsedEvent>(OnPowerUsed);
            SubscribeLocalEvent<PsionicInsulationComponent, ExaminedEvent>(OnExamined);

            SubscribeLocalEvent<PsionicComponent, MobStateChangedEvent>(OnMobStateChanged);
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

        private void OnInit(EntityUid uid, PsionicsDisabledComponent component, ComponentInit args)
        {
            SetPsionicsThroughEligibility(uid);
        }

        private void OnShutdown(EntityUid uid, PsionicsDisabledComponent component, ComponentShutdown args)
        {
            SetPsionicsThroughEligibility(uid);
        }

        private void OnMobStateChanged(EntityUid uid, PsionicComponent component, MobStateChangedEvent args)
        {
            SetPsionicsThroughEligibility(uid);
        }

        /// <summary>
        /// Checks whether the entity is eligible to use its psionic ability. This should be run after anything that could effect psionic eligibility.
        /// </summary>
        public void SetPsionicsThroughEligibility(EntityUid uid)
        {
            PsionicComponent? component = null;
            if (!Resolve(uid, ref component, false))
                return;

            if (component.PsionicAbility == null)
                return;

            _actions.TryGetActionData( component.PsionicAbility, out var actionData );

            if (actionData == null)
                return;

            _actions.SetEnabled(uid, IsEligibleForPsionics(uid));
        }

        private void OnExamined(EntityUid uid, PsionicInsulationComponent component, ExaminedEvent args)
        {
            if (!component.MindBroken || !args.IsInDetailsRange)
                return;

            args.PushMarkup($"[color=mediumpurple]{Loc.GetString("examine-mindbroken-message", ("entity", uid))}[/color]");
        }

        private bool IsEligibleForPsionics(EntityUid uid)
        {
            return !HasComp<PsionicInsulationComponent>(uid)
                && (!TryComp<MobStateComponent>(uid, out var mobstate) || mobstate.CurrentState == MobState.Alive);
        }

        public bool CheckCanSelfCast(EntityUid uid, [NotNullWhen(true)] out PsionicComponent? psiComp)
        {
            if (!HasComp<PsionicInsulationComponent>(uid))
                return TryComp(uid, out psiComp);
            psiComp = null;
            return false;
        }

        public bool CheckCanTargetCast(EntityUid performer, EntityUid target, [NotNullWhen(true)] out PsionicComponent? psiComp)
        {
            if (!HasComp<PsionicInsulationComponent>(performer)
                && !HasComp<PsionicInsulationComponent>(target))
                return TryComp(performer, out psiComp);
            psiComp = null;
            return false;
        }

        public void LogPowerUsed(EntityUid uid, string power, PsionicComponent? psionic = null, int minGlimmer = 8, int maxGlimmer = 12, bool overrideGlimmer = false)
        {
            _adminLogger.Add(Database.LogType.Psionics, Database.LogImpact.Medium, $"{ToPrettyString(uid):player} used {power}");
            var ev = new PsionicPowerUsedEvent(uid, power);
            RaiseLocalEvent(uid, ev, false);

            //Redundant check for the GlimmerEnabled CVar because I want to skip this math too if its turned off.
            if (_glimmerSystem.GetGlimmerEnabled() && !overrideGlimmer)
            {
                if (psionic == null)
                    _glimmerSystem.DeltaGlimmerInput(_robustRandom.NextFloat(minGlimmer, maxGlimmer));
                else _glimmerSystem.DeltaGlimmerInput(_robustRandom.NextFloat(
                    minGlimmer * psionic.Amplification - psionic.Dampening,
                    maxGlimmer * psionic.Amplification - psionic.Dampening));
            }
        }
    }

    public sealed class PsionicPowerUsedEvent : HandledEntityEventArgs
    {
        public EntityUid User { get; }
        public string Power = string.Empty;

        public PsionicPowerUsedEvent(EntityUid user, string power)
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
