using Content.Shared.Popups;
using Content.Shared.Atmos.Rotting;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Inventory;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Verbs;
using Robust.Shared.Utility;
using Robust.Shared.Random;

namespace Content.Shared.Medical.CPR
{
    public sealed partial class CPRSystem : EntitySystem
    {
        [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
        [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
        [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
        [Dependency] private readonly DamageableSystem _damageable = default!;
        [Dependency] private readonly MobThresholdSystem _mobThreshold = default!;
        [Dependency] private readonly IRobustRandom _robustRandom = default!;
        [Dependency] private readonly SharedRottingSystem _rottingSystem = default!;
        [Dependency] private readonly InventorySystem _inventory = default!;
        public override void Initialize()
        {
            base.Initialize();
            InitializeCVars();
            SubscribeLocalEvent<CPRTrainingComponent, GetVerbsEvent<InnateVerb>>(AddCPRVerb);
            SubscribeLocalEvent<CPRTrainingComponent, CPRDoAfterEvent>(OnCPRDoAfter);
        }

        private void AddCPRVerb(EntityUid uid, CPRTrainingComponent component, GetVerbsEvent<InnateVerb> args)
        {
            if (!DoCPRSystem || !args.CanInteract || !args.CanAccess
                || !TryComp<MobStateComponent>(args.Target, out var targetState)
                || targetState.CurrentState == MobState.Alive)
                return;

            InnateVerb verb = new()
            {
                Act = () =>
                {
                    StartCPR(uid, args.Target, targetState, component);
                },
                Text = Loc.GetString("cpr-verb"),
                Icon = new SpriteSpecifier.Rsi(new("Interface/Alerts/human_alive.rsi"), "health4"),
                Priority = 2
            };
            args.Verbs.Add(verb);
        }

        private void StartCPR(EntityUid performer, EntityUid target, MobStateComponent targetState, CPRTrainingComponent cprComponent)
        {
            if (HasComp<RottingComponent>(target))
            {
                _popupSystem.PopupEntity(Loc.GetString("cpr-target-rotting", ("entity", target)), performer, performer);
                return;
            }

            if (_inventory.TryGetSlotEntity(target, "outerClothing", out var outer))
            {
                _popupSystem.PopupEntity(Loc.GetString("cpr-must-remove", ("clothing", outer)), performer, performer, PopupType.MediumCaution);
                return;
            }

            if (_inventory.TryGetSlotEntity(target, "mask", out var mask))
            {
                _popupSystem.PopupEntity(Loc.GetString("cpr-must-remove", ("clothing", mask)), performer, performer, PopupType.MediumCaution);
                return;
            }

            if (_inventory.TryGetSlotEntity(target, "mask", out var maskSelf))
            {
                _popupSystem.PopupEntity(Loc.GetString("cpr-must-remove-own-mask", ("clothing", maskSelf)), performer, performer, PopupType.MediumCaution);
                return;
            }

            _popupSystem.PopupEntity(Loc.GetString("cpr-start-second-person", ("target", target)), target, performer, PopupType.Medium);
            _popupSystem.PopupEntity(Loc.GetString("cpr-start-second-person-patient", ("user", performer)), target, target, PopupType.Medium);

            _doAfterSystem.TryStartDoAfter(new DoAfterArgs(EntityManager, performer, cprComponent.DoAfterDuration, new CPRDoAfterEvent(), target)
            {
                BreakOnTargetMove = true,
                BreakOnUserMove = true,
                NeedHand = true,
                BlockDuplicate = true
            });
        }

        private void OnCPRDoAfter(EntityUid performer, CPRTrainingComponent component, CPRDoAfterEvent args)
        {
            // There is PROBABLY a better way to do this, by all means let me know
            DamageSpecifier healing = new()
            {
                DamageDict = new()
                {
                    { "Airloss", -component.AirlossHeal * CPRAirlossReductionMultiplier}
                }
            };

            if (args.Target == null)
                return;

            if (CPRHealsAirloss)
                _damageable.TryChangeDamage(args.Target, healing, true, origin: performer);

            if (CPRReducesRot)
                _rottingSystem.ReduceAccumulator((EntityUid) args.Target, component.DoAfterDuration * CPRRotReductionMultiplier);

            if (CPRResuscitate && _robustRandom.Prob(0.01f)
                && _mobThreshold.TryGetThresholdForState((EntityUid) args.Target, MobState.Dead, out var threshold)
                && TryComp<DamageableComponent>(args.Target, out var damageableComponent)
                && TryComp<MobStateComponent>(args.Target, out var state)
                && damageableComponent.TotalDamage < threshold)
            {
                _mobStateSystem.ChangeMobState((EntityUid) args.Target, MobState.Critical, state, performer);
            }

        }
    }
}
