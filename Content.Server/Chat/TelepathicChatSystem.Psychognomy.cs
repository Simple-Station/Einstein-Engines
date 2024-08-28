using Content.Shared.Humanoid;
using Content.Shared.Damage;
using Content.Shared.Mobs.Components;
using Content.Shared.Physics;
using Content.Shared.Psionics;
using Content.Server.Bible.Components;
using Content.Server.Research.Oracle;
using Robust.Shared.GameObjects.Components.Localization;
using Robust.Shared.Enums;
using Content.Server.Nyanotrasen.Research.SophicScribe;
using Content.Server.Nyanotrasen.Cloning;
using Content.Server.Psionics.Glimmer;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Content.Shared.Nutrition.Components;
using Content.Server.Vampiric;
using Content.Shared.Revenant.Components;
using Content.Shared.Abilities.Psionics;
using Content.Server.Abilities.Psionics;
using System.ComponentModel;

namespace Content.Server.Chat
{
    public sealed partial class TelepathicChatSystem
    {
        private void InitializePsychognomy()
        {
            SubscribeLocalEvent<HumanoidAppearanceComponent, GetPsychognomicDescriptorEvent>(DescribeHumanoid);
            SubscribeLocalEvent<GrammarComponent, GetPsychognomicDescriptorEvent>(DescribeGrammar);
            SubscribeLocalEvent<DamageableComponent, GetPsychognomicDescriptorEvent>(DescribeDamage);
            SubscribeLocalEvent<MobStateComponent, GetPsychognomicDescriptorEvent>(DescribeMobState);
            SubscribeLocalEvent<HungerComponent, GetPsychognomicDescriptorEvent>(DescribeHunger);
            SubscribeLocalEvent<PhysicsComponent, GetPsychognomicDescriptorEvent>(DescribePhysics);
            SubscribeLocalEvent<FixturesComponent, GetPsychognomicDescriptorEvent>(DescribeFixtures);
            SubscribeLocalEvent<MetempsychosisKarmaComponent, GetPsychognomicDescriptorEvent>(DescribeKarma);
            SubscribeLocalEvent<GlimmerSourceComponent, GetPsychognomicDescriptorEvent>(DescribeGlimmerSource);
            SubscribeLocalEvent<PsionicComponent, GetPsychognomicDescriptorEvent>(DescribePsion);
            SubscribeLocalEvent<InnatePsionicPowersComponent, GetPsychognomicDescriptorEvent>(DescribeInnatePsionics);
            SubscribeLocalEvent<BloodSuckerComponent, GetPsychognomicDescriptorEvent>(DescribeBloodsucker);
            SubscribeLocalEvent<RevenantComponent, GetPsychognomicDescriptorEvent>(DescribeRevenant);
            // SubscribeLocalEvent<GlimmerWispComponent, GetPsychognomicDescriptorEvent>(DescribeGlimmerWisp);
            SubscribeLocalEvent<FamiliarComponent, GetPsychognomicDescriptorEvent>(DescribeFamiliar);
            // SubscribeLocalEvent<GolemComponent, GetPsychognomicDescriptorEvent>(DescribeGolem);
            SubscribeLocalEvent<OracleComponent, GetPsychognomicDescriptorEvent>(DescribeOracle);
            SubscribeLocalEvent<SophicScribeComponent, GetPsychognomicDescriptorEvent>(DescribeSophia);
        }

        private void DescribeHumanoid(EntityUid uid, HumanoidAppearanceComponent component, GetPsychognomicDescriptorEvent ev)
        {
            if (component.Sex != Sex.Unsexed)
                ev.Descriptors.Add(component.Sex == Sex.Male ? Loc.GetString("p-descriptor-male") : Loc.GetString("p-descriptor-female"));

            // Deliberately obfuscating a bit; psionic signatures use different standards
            ev.Descriptors.Add(component.Age >= 100 ? Loc.GetString("p-descriptor-old") : Loc.GetString("p-descriptor-young"));
        }

        private void DescribeGrammar(EntityUid uid, GrammarComponent component, GetPsychognomicDescriptorEvent ev)
        {
            if (component.Gender == Gender.Male || component.Gender == Gender.Female)
                ev.Descriptors.Add(component.Gender == Gender.Male ? Loc.GetString("p-descriptor-masculine") : Loc.GetString("p-descriptor-feminine"));
        }

        private void DescribeDamage(EntityUid uid, DamageableComponent component, GetPsychognomicDescriptorEvent ev)
        {
            if (component.DamageContainerID == "CorporealSpirit")
            {
                ev.Descriptors.Add(Loc.GetString("p-descriptor-liminal"));
                if (!HasComp<HumanoidAppearanceComponent>(uid))
                    ev.Descriptors.Add(Loc.GetString("p-descriptor-old"));
                return;
            }

            ev.Descriptors.Add(Loc.GetString("p-descriptor-hylic"));
        }

        private void DescribeMobState(EntityUid uid, MobStateComponent component, GetPsychognomicDescriptorEvent ev)
        {
            if (component.CurrentState == Shared.Mobs.MobState.Critical)
                ev.Descriptors.Add(Loc.GetString("p-descriptor-liminal"));
        }

        private void DescribeHunger(EntityUid uid, HungerComponent component, GetPsychognomicDescriptorEvent ev)
        {
            if (component.CurrentThreshold <= HungerThreshold.Peckish)
                ev.Descriptors.Add(Loc.GetString("p-descriptor-hungry"));
        }

        private void DescribeFixtures(EntityUid uid, FixturesComponent component, GetPsychognomicDescriptorEvent ev)
        {
            foreach (var fixture in component.Fixtures.Values)
            {
                if (fixture.CollisionMask == (int) CollisionGroup.GhostImpassable)
                {
                    ev.Descriptors.Add(Loc.GetString("p-descriptor-pneumatic"));
                    return;
                }
            }
        }

        private void DescribePhysics(EntityUid uid, PhysicsComponent component, GetPsychognomicDescriptorEvent ev)
        {
            if (component.FixturesMass < 45)
                ev.Descriptors.Add(Loc.GetString("p-descriptor-light"));
            else if (component.FixturesMass > 70)
                ev.Descriptors.Add(Loc.GetString("p-descriptor-heavy"));
        }

        private void DescribeKarma(EntityUid uid, MetempsychosisKarmaComponent component, GetPsychognomicDescriptorEvent ev)
        {
            if (component.Score > 0)
            {
                ev.Descriptors.Add(Loc.GetString("cyclic"));
            }
        }

        private void DescribeGlimmerSource(EntityUid uid, GlimmerSourceComponent component, GetPsychognomicDescriptorEvent ev)
        {
            if (component.AddToGlimmer)
                ev.Descriptors.Add(Loc.GetString("p-descriptor-emanative"));
            else
            {
                ev.Descriptors.Add(Loc.GetString("p-descriptor-vampiric"));
                ev.Descriptors.Add(Loc.GetString("p-descriptor-hungry"));
            }
        }

        private void DescribePsion(EntityUid uid, PsionicComponent component, GetPsychognomicDescriptorEvent ev)
        {
            foreach (var power in component.ActivePowers)
            {
                // TODO: Mime counts too and isn't added back to psions yet
                if (power.ID == "PyrokinesisPower" || power.ID == "NoosphericZapPower")
                    ev.Descriptors.Add(Loc.GetString("p-descriptor-kinetic"));

                return;
            }
        }

        private void DescribeInnatePsionics(EntityUid uid, InnatePsionicPowersComponent component, GetPsychognomicDescriptorEvent ev)
        {
            ev.Descriptors.Add(Loc.GetString("p-descriptor-gnostic"));
        }
        private void DescribeBloodsucker(EntityUid uid, BloodSuckerComponent component, GetPsychognomicDescriptorEvent ev)
        {
            ev.Descriptors.Add(Loc.GetString("p-descriptor-vampiric"));
        }

        private void DescribeRevenant(EntityUid uid, RevenantComponent component, GetPsychognomicDescriptorEvent ev)
        {
            ev.Descriptors.Add(Loc.GetString("p-descriptor-vampiric"));
        }

        private void DescribeFamiliar(EntityUid uid, FamiliarComponent component, GetPsychognomicDescriptorEvent ev)
        {
            ev.Descriptors.Add(Loc.GetString("p-descriptor-bound"));
            ev.Descriptors.Add(Loc.GetString("p-descriptor-cyclic"));
        }

        private void DescribeOracle(EntityUid uid, OracleComponent component, GetPsychognomicDescriptorEvent ev)
        {
            ev.Descriptors.Add(Loc.GetString("p-descriptor-old"));
            ev.Descriptors.Add(Loc.GetString("p-descriptor-demiurgic"));
            ev.Descriptors.Add(Loc.GetString("p-descriptor-mysterious"));
        }

        private void DescribeSophia(EntityUid uid, SophicScribeComponent component, GetPsychognomicDescriptorEvent ev)
        {
            ev.Descriptors.Add(Loc.GetString("p-descriptor-old"));
            ev.Descriptors.Add(Loc.GetString("p-descriptor-demiurgic"));
            ev.Descriptors.Add(Loc.GetString("p-descriptor-mysterious"));
        }
    }
    public sealed class GetPsychognomicDescriptorEvent : EntityEventArgs
    {
        public List<String> Descriptors = new List<String>();
    }
}