using Content.Server.Body.Components;
using Content.Server.DoAfter;
using Content.Server.Fluids.EntitySystems;
using Content.Server.Forensics.Components;
using Content.Server.Popups;
using Content.Shared.Chemistry.Components;
using Content.Shared.DoAfter;
using Content.Shared.Forensics;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Inventory;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Random;
using Content.Shared.Inventory.Events;

namespace Content.Server.Forensics
{
    public sealed class ForensicsSystem : EntitySystem
    {
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly InventorySystem _inventory = default!;
        [Dependency] private readonly DoAfterSystem _doAfterSystem = default!;
        [Dependency] private readonly PopupSystem _popupSystem = default!;
        public override void Initialize()
        {
            SubscribeLocalEvent<FingerprintComponent, ContactInteractionEvent>(OnInteract);
            SubscribeLocalEvent<ScentComponent, DidEquipEvent>(OnEquip);
            SubscribeLocalEvent<FiberComponent, MapInitEvent>(OnFiberInit);
            SubscribeLocalEvent<FingerprintComponent, MapInitEvent>(OnFingerprintInit);
            SubscribeLocalEvent<DnaComponent, MapInitEvent>(OnDNAInit);
            SubscribeLocalEvent<ScentComponent, MapInitEvent>(OnScentInit);

            SubscribeLocalEvent<DnaComponent, BeingGibbedEvent>(OnBeingGibbed);
            SubscribeLocalEvent<ForensicsComponent, MeleeHitEvent>(OnMeleeHit);
            SubscribeLocalEvent<ForensicsComponent, GotRehydratedEvent>(OnRehydrated);
            SubscribeLocalEvent<CleansForensicsComponent, AfterInteractEvent>(OnAfterInteract, after: new[] { typeof(AbsorbentSystem) });
            SubscribeLocalEvent<ForensicsComponent, CleanForensicsDoAfterEvent>(OnCleanForensicsDoAfter);
            SubscribeLocalEvent<DnaComponent, TransferDnaEvent>(OnTransferDnaEvent);
        }

        private void OnInteract(EntityUid uid, FingerprintComponent component, ContactInteractionEvent args)
        {
            ApplyEvidence(uid, args.Other);
        }

        private void OnEquip(EntityUid uid, ScentComponent component, DidEquipEvent args)
        {
            ApplyScent(uid, args.Equipment);
        }

        private void OnFiberInit(EntityUid uid, FiberComponent component, MapInitEvent args)
        {
            component.Fiberprint = GenerateFingerprint(length: 7);
        }

        private void OnFingerprintInit(EntityUid uid, FingerprintComponent component, MapInitEvent args)
        {
            component.Fingerprint = GenerateFingerprint();
        }

        private void OnDNAInit(EntityUid uid, DnaComponent component, MapInitEvent args)
        {
            component.DNA = GenerateDNA();

        }

        private void OnScentInit(EntityUid uid, ScentComponent component, MapInitEvent args)
        {
            component.Scent = GenerateFingerprint(length: 5);

            var updatecomp = EnsureComp<ForensicsComponent>(uid);
            updatecomp.Scent = component.Scent;

            Dirty(uid, updatecomp);
        }

        private void OnBeingGibbed(EntityUid uid, DnaComponent component, BeingGibbedEvent args)
        {
            foreach (EntityUid part in args.GibbedParts)
            {
                var partComp = EnsureComp<ForensicsComponent>(part);
                partComp.DNAs.Add(component.DNA);
                partComp.CanDnaBeCleaned = false;
                Dirty(part, partComp);
            }
        }

        private void OnMeleeHit(EntityUid uid, ForensicsComponent component, MeleeHitEvent args)
        {
            if ((args.BaseDamage.DamageDict.TryGetValue("Blunt", out var bluntDamage) && bluntDamage.Value > 0) ||
                (args.BaseDamage.DamageDict.TryGetValue("Slash", out var slashDamage) && slashDamage.Value > 0) ||
                (args.BaseDamage.DamageDict.TryGetValue("Piercing", out var pierceDamage) && pierceDamage.Value > 0))
            {
                foreach (EntityUid hitEntity in args.HitEntities)
                {
                    if (TryComp<DnaComponent>(hitEntity, out var hitEntityComp))
                        component.DNAs.Add(hitEntityComp.DNA);
                }
            }
            Dirty(uid, component);
        }

        private void OnRehydrated(Entity<ForensicsComponent> ent, ref GotRehydratedEvent args)
        {
            CopyForensicsFrom(ent.Comp, args.Target);
            Dirty(args.Target, ent.Comp);
        }

        /// <summary>
        /// Copy forensic information from a source entity to a destination.
        /// Existing forensic information on the target is still kept.
        /// </summary>
        public void CopyForensicsFrom(ForensicsComponent src, EntityUid target)
        {
            var dest = EnsureComp<ForensicsComponent>(target);
            foreach (var dna in src.DNAs)
            {
                dest.DNAs.Add(dna);
            }

            foreach (var fiber in src.Fibers)
            {
                dest.Fibers.Add(fiber);
            }

            foreach (var print in src.Fingerprints)
            {
                dest.Fingerprints.Add(print);
            }
        }

        private void OnAfterInteract(EntityUid uid, CleansForensicsComponent component, AfterInteractEvent args)
        {
            if (args.Handled || !args.CanReach || !TryComp<ForensicsComponent>(args.Target, out var forensicsComp))
                return;

            if ((forensicsComp.DNAs.Count <= 0 || !forensicsComp.CanDnaBeCleaned)
                && forensicsComp.Fingerprints.Count + forensicsComp.Fibers.Count <= 0
                && forensicsComp.Scent == string.Empty)
                return; // Nothing to do if there is no DNAs, fibers, and scent

            var cleanDelay = component.CleanDelay;
            if (HasComp<ScentComponent>(args.Target))
                cleanDelay += 30;

            var doAfterArgs = new DoAfterArgs(EntityManager, args.User, cleanDelay, new CleanForensicsDoAfterEvent(), uid, target: args.Target, used: args.Used)
            {
                BreakOnHandChange = true,
                NeedHand = true,
                BreakOnDamage = true,
                BreakOnMove = true,
                MovementThreshold = 0.01f,
                DistanceThreshold = forensicsComp.CleanDistance,
            };

            _doAfterSystem.TryStartDoAfter(doAfterArgs);
            _popupSystem.PopupEntity(Loc.GetString("forensics-cleaning", ("target", args.Target)), args.User, args.User);

            args.Handled = true;
        }

        private void OnCleanForensicsDoAfter(EntityUid uid, ForensicsComponent component, CleanForensicsDoAfterEvent args)
        {
            if (args.Handled || args.Cancelled || args.Args.Target == null)
                return;

            if (!TryComp<ForensicsComponent>(args.Target, out var targetComp))
                return;

            targetComp.Fibers = new();
            targetComp.Fingerprints = new();
            targetComp.Scent = String.Empty;

            if (targetComp.CanDnaBeCleaned)
                targetComp.DNAs = new();

            // leave behind evidence it was cleaned
            if (TryComp<FiberComponent>(args.Used, out var fiber))
                targetComp.Fibers.Add(string.IsNullOrEmpty(fiber.FiberColor) ? Loc.GetString("forensic-fibers", ("material", fiber.FiberMaterial)) : Loc.GetString("forensic-fibers-colored", ("color", fiber.FiberColor), ("material", fiber.FiberMaterial)));

            if (TryComp<ResidueComponent>(args.Used, out var residue))
                targetComp.Residues.Add(string.IsNullOrEmpty(residue.ResidueColor) ? Loc.GetString("forensic-residue", ("adjective", residue.ResidueAdjective)) : Loc.GetString("forensic-residue-colored", ("color", residue.ResidueColor), ("adjective", residue.ResidueAdjective)));

            // If the ent has a Scent Component, we completely generate a new one and apply the new scent to all currently worn items.
            // TODO this is never gonna work unless you like, wash yourself with the soap???
            if (TryComp<ScentComponent>(args.Target, out var scentComp))
            {
                var generatedscent = GenerateFingerprint(length: 5);
                scentComp.Scent = generatedscent;
                targetComp.Scent = generatedscent;

                if (args.Target is { Valid: true } target
                    && _inventory.TryGetSlots(target, out var slotDefinitions))
                    foreach (var slot in slotDefinitions)
                    {
                        if (!_inventory.TryGetSlotEntity(target, slot.Name, out var slotEnt))
                            continue;

                        EnsureComp<ForensicsComponent>(slotEnt.Value, out var recipientComp);
                        recipientComp.Scent = generatedscent;

                        Dirty(slotEnt.Value, recipientComp);
                    }
            }

            if (args.Target is { Valid: true } targetuid)
                Dirty(targetuid, targetComp);
        }

        public string GenerateFingerprint(int length = 16)
        {
            var fingerprint = new byte[Math.Clamp(length, 0, 255)];
            _random.NextBytes(fingerprint);
            return Convert.ToHexString(fingerprint);
        }

        public string GenerateDNA()
        {
            var letters = new[] { "A", "C", "G", "T" };
            var DNA = string.Empty;

            for (var i = 0; i < 16; i++)
            {
                DNA += letters[_random.Next(letters.Length)];
            }

            return DNA;
        }

        private void ApplyEvidence(EntityUid user, EntityUid target)
        {
            if (HasComp<IgnoresFingerprintsComponent>(target))
                return;

            var component = EnsureComp<ForensicsComponent>(target);
            if (_inventory.TryGetSlotEntity(user, "gloves", out var gloves))
            {
                if (TryComp<FiberComponent>(gloves, out var fiber) && !string.IsNullOrEmpty(fiber.FiberMaterial))
                {
                    var fiberLocale = string.IsNullOrEmpty(fiber.FiberColor)
                        ? Loc.GetString("forensic-fibers", ("material", fiber.FiberMaterial))
                        : Loc.GetString("forensic-fibers-colored", ("color", fiber.FiberColor), ("material", fiber.FiberMaterial));
                    component.Fibers.Add(fiberLocale + " ; " + fiber.Fiberprint);
                }

                if (HasComp<FingerprintMaskComponent>(gloves))
                {
                    Dirty(target, component);
                    return;
                }
            }
            if (TryComp<FingerprintComponent>(user, out var fingerprint))
            {
                component.Fingerprints.Add(fingerprint.Fingerprint ?? "");
                Dirty(target, component);
            }
        }

        private void ApplyScent(EntityUid user, EntityUid target)
        {
            if (HasComp<ScentComponent>(target))
                return;

            var component = EnsureComp<ForensicsComponent>(target);
            if (TryComp<ScentComponent>(user, out var scent))
                component.Scent = scent.Scent;

            Dirty(target, component);
        }

        private void OnTransferDnaEvent(EntityUid uid, DnaComponent component, ref TransferDnaEvent args)
        {
            var recipientComp = EnsureComp<ForensicsComponent>(args.Recipient);
            recipientComp.DNAs.Add(component.DNA);
            recipientComp.CanDnaBeCleaned = args.CanDnaBeCleaned;

            Dirty(args.Recipient, recipientComp);
        }

        #region Public API

        /// <summary>
        /// Transfer DNA from one entity onto the forensics of another
        /// </summary>
        /// <param name="recipient">The entity receiving the DNA</param>
        /// <param name="donor">The entity applying its DNA</param>
        /// <param name="canDnaBeCleaned">If this DNA be cleaned off of the recipient. e.g. cleaning a knife vs cleaning a puddle of blood</param>
        public void TransferDna(EntityUid recipient, EntityUid donor, bool canDnaBeCleaned = true)
        {
            if (TryComp<DnaComponent>(donor, out var donorComp))
            {
                EnsureComp<ForensicsComponent>(recipient, out var recipientComp);
                recipientComp.DNAs.Add(donorComp.DNA);
                recipientComp.CanDnaBeCleaned = canDnaBeCleaned;

                Dirty(recipient, recipientComp);
            }
        }

        #endregion
    }
}
