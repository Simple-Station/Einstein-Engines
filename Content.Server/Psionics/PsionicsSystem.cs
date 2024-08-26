using Content.Shared.Abilities.Psionics;
using Content.Shared.StatusEffect;
using Content.Shared.Psionics.Glimmer;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Damage.Events;
using Content.Shared.CCVar;
using Content.Server.Abilities.Psionics;
using Content.Server.Electrocution;
using Content.Server.NPC.Components;
using Content.Server.NPC.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Random;

namespace Content.Server.Psionics
{
    public sealed class PsionicsSystem : EntitySystem
    {
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly PsionicAbilitiesSystem _psionicAbilitiesSystem = default!;
        [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
        [Dependency] private readonly ElectrocutionSystem _electrocutionSystem = default!;
        [Dependency] private readonly MindSwapPowerSystem _mindSwapPowerSystem = default!;
        [Dependency] private readonly GlimmerSystem _glimmerSystem = default!;
        [Dependency] private readonly NpcFactionSystem _npcFactonSystem = default!;
        [Dependency] private readonly IConfigurationManager _cfg = default!;
        [Dependency] private readonly SharedAudioSystem _audio = default!;

        private const string BaselineAmplification = "Baseline Amplification";
        private const string BaselineDampening = "Baseline Dampening";

        /// <summary>
        /// Unfortunately, since spawning as a normal role and anything else is so different,
        /// this is the only way to unify them, for now at least.
        /// </summary>
        Queue<(PsionicComponent component, EntityUid uid)> _rollers = new();
        public override void Update(float frameTime)
        {
            base.Update(frameTime);
            foreach (var roller in _rollers)
                RollPsionics(roller.uid, roller.component, false);
            _rollers.Clear();
        }
        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<PsionicComponent, MapInitEvent>(OnStartup);
            SubscribeLocalEvent<AntiPsionicWeaponComponent, MeleeHitEvent>(OnMeleeHit);
            SubscribeLocalEvent<AntiPsionicWeaponComponent, TakeStaminaDamageEvent>(OnStamHit);

            SubscribeLocalEvent<PsionicComponent, ComponentStartup>(OnInit);
            SubscribeLocalEvent<PsionicComponent, ComponentRemove>(OnRemove);
        }

        private void OnStartup(EntityUid uid, PsionicComponent component, MapInitEvent args)
        {
            _rollers.Enqueue((component, uid));
        }

        private void OnMeleeHit(EntityUid uid, AntiPsionicWeaponComponent component, MeleeHitEvent args)
        {
            foreach (var entity in args.HitEntities)
            {
                if (HasComp<PsionicComponent>(entity))
                {
                    _audio.PlayPvs("/Audio/Effects/lightburn.ogg", entity);
                    args.ModifiersList.Add(component.Modifiers);
                    if (_random.Prob(component.DisableChance))
                        _statusEffects.TryAddStatusEffect(entity, component.DisableStatus, TimeSpan.FromSeconds(component.DisableDuration), true, component.DisableStatus);
                }

                if (TryComp<MindSwappedComponent>(entity, out var swapped))
                {
                    _mindSwapPowerSystem.Swap(entity, swapped.OriginalEntity, true);
                    return;
                }

                if (component.Punish && !HasComp<PsionicComponent>(entity) && _random.Prob(component.PunishChances))
                    _electrocutionSystem.TryDoElectrocution(args.User, null, component.PunishSelfDamage, TimeSpan.FromSeconds(component.PunishStunDuration), false);
            }
        }

        private void OnInit(EntityUid uid, PsionicComponent component, ComponentStartup args)
        {
            component.AmplificationSources.Add(BaselineAmplification, _random.NextFloat(component.BaselineAmplification.Item1, component.BaselineAmplification.Item2));
            component.DampeningSources.Add(BaselineDampening, _random.NextFloat(component.BaselineDampening.Item1, component.BaselineDampening.Item2));

            if (!component.Removable
                || !TryComp<NpcFactionMemberComponent>(uid, out var factions)
                || _npcFactonSystem.ContainsFaction(uid, "GlimmerMonster", factions))
                return;

            _npcFactonSystem.AddFaction(uid, "PsionicInterloper");
        }

        private void OnRemove(EntityUid uid, PsionicComponent component, ComponentRemove args)
        {
            if (!HasComp<NpcFactionMemberComponent>(uid))
                return;

            _npcFactonSystem.RemoveFaction(uid, "PsionicInterloper");
        }

        private void OnStamHit(EntityUid uid, AntiPsionicWeaponComponent component, TakeStaminaDamageEvent args)
        {
            if (HasComp<PsionicComponent>(args.Target))
                args.FlatModifier += component.PsychicStaminaDamage;
        }

        public void RollPsionics(EntityUid uid, PsionicComponent component, bool applyGlimmer = true, float rollEventMultiplier = 1f)
        {
            if (!_cfg.GetCVar(CCVars.PsionicRollsEnabled)
                || !component.Removable)
                return;

            // Calculate the initial odds based on the innate potential
            var baselineChance = component.Chance
                * component.PowerRollMultiplier
                + component.PowerRollFlatBonus;

            // Increase the initial odds based on Glimmer.
            // TODO: Change this equation when I do my Glimmer Refactor
            baselineChance += applyGlimmer
                ? (float) _glimmerSystem.Glimmer / 1000 //Convert from Glimmer to %chance
                : 0;

            // Certain sources of power rolls provide their own multiplier.
            baselineChance *= rollEventMultiplier;

            // Ask if the Roller has any other effects to contribute, such as Traits.
            var ev = new OnRollPsionicsEvent(uid, baselineChance);
            RaiseLocalEvent(uid, ref ev);

            if (_random.Prob(Math.Clamp(ev.BaselineChance, 0, 1)))
                _psionicAbilitiesSystem.AddPsionics(uid);
        }

        public void RerollPsionics(EntityUid uid, PsionicComponent? psionic = null, float bonusMuliplier = 1f)
        {
            if (!Resolve(uid, ref psionic, false)
                || !psionic.Removable
                || psionic.CanReroll)
                return;

            RollPsionics(uid, psionic, true, bonusMuliplier);
            psionic.CanReroll = true;
        }
    }
}
