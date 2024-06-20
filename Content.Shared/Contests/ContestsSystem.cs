using Robust.Shared.Physics.Components;

namespace Content.Shared.Contests
{
    public sealed partial class ContestsSystem : EntitySystem
    {
        // Defaulted to the average mass of an adult human
        /// <summary>
        ///     The presumed average mass of a player entity
        /// </summary>
        private const float AverageMass = 71f;
        public override void Initialize()
        {
            base.Initialize();

            InitializeCVars();

        }
        // REGION
        // Mass Contests
        /// <summary>
        ///     Outputs the ratio of mass between a performer and the average human mass
        /// </summary>
        /// <param name="performerUid">Uid of Performer</param>
        /// <returns></returns>
        public float MassContest(EntityUid performerUid, float otherMass = AverageMass)
        {
            if (DoMassContests
                && TryComp<PhysicsComponent>(performerUid, out var performerPhysics)
                && performerPhysics.Mass != 0)
                return Math.Clamp(performerPhysics.Mass / otherMass, 1 - MassContestsMaxPercentage, 1 + MassContestsMaxPercentage);

            return 1f;
        }

        public float MassContest(EntityUid? performerUid, float otherMass = AverageMass)
        {
            if (DoMassContests)
            {
                var ratio = performerUid is { } uid ? MassContest(uid, otherMass) : 1f;
                return ratio;
            }

            return 1f;
        }

        /// <summary>
        ///     Outputs the ratio of mass between a performer and the average human mass
        ///     If a function already has the performer's physics component, this is faster
        /// </summary>
        /// <param name="performerPhysics"></param>
        /// <returns></returns>
        public float MassContest(PhysicsComponent performerPhysics, float otherMass = AverageMass)
        {
            if (DoMassContests
                && performerPhysics.Mass != 0)
                return Math.Clamp(performerPhysics.Mass / otherMass, 1 - MassContestsMaxPercentage, 1 + MassContestsMaxPercentage);

            return 1f;
        }

        /// <summary>
        ///     Outputs the ratio of mass between a performer and a target, accepts either EntityUids or PhysicsComponents in any combination
        ///     If you have physics components already in your function, use those instead
        /// </summary>
        /// <param name="performerUid"></param>
        /// <param name="targetUid"></param>
        /// <returns></returns>
        public float MassContest(EntityUid performerUid, EntityUid targetUid)
        {
            if (DoMassContests
                && TryComp<PhysicsComponent>(performerUid, out var performerPhysics)
                && TryComp<PhysicsComponent>(targetUid, out var targetPhysics)
                && performerPhysics.Mass != 0
                && targetPhysics.InvMass != 0)
                return Math.Clamp(performerPhysics.Mass * targetPhysics.InvMass, 1 - MassContestsMaxPercentage, 1 + MassContestsMaxPercentage);

            return 1f;
        }

        /// <inheritdoc cref="MassContest(EntityUid, EntityUid)"/>
        public float MassContest(EntityUid performerUid, PhysicsComponent targetPhysics)
        {
            if (DoMassContests
                && TryComp<PhysicsComponent>(performerUid, out var performerPhysics)
                && performerPhysics.Mass != 0
                && targetPhysics.InvMass != 0)
                return Math.Clamp(performerPhysics.Mass * targetPhysics.InvMass, 1 - MassContestsMaxPercentage, 1 + MassContestsMaxPercentage);

            return 1f;
        }

        /// <inheritdoc cref="MassContest(EntityUid, EntityUid)"/>
        public float MassContest(PhysicsComponent performerPhysics, EntityUid targetUid)
        {
            if (DoMassContests
                && TryComp<PhysicsComponent>(targetUid, out var targetPhysics)
                && performerPhysics.Mass != 0
                && targetPhysics.InvMass != 0)
                return Math.Clamp(performerPhysics.Mass * targetPhysics.InvMass, 1 - MassContestsMaxPercentage, 1 + MassContestsMaxPercentage);

            return 1f;
        }

        /// <inheritdoc cref="MassContest(EntityUid, EntityUid)"/>
        public float MassContest(PhysicsComponent performerPhysics, PhysicsComponent targetPhysics)
        {
            if (DoMassContests
                && performerPhysics.Mass != 0
                && targetPhysics.InvMass != 0)
                return Math.Clamp(performerPhysics.Mass * targetPhysics.InvMass, 1 - MassContestsMaxPercentage, 1 + MassContestsMaxPercentage);

            return 1f;
        }
    }
}
