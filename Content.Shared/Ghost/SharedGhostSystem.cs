using Content.Shared.Emoting;
using Content.Shared.Hands;
using Content.Shared.Interaction.Events;
using Content.Shared.InteractionVerbs.Events;
using Content.Shared.Item;
using Content.Shared.Popups;
using Robust.Shared.Serialization;

namespace Content.Shared.Ghost
{
    /// <summary>
    /// System for the <see cref="GhostComponent"/>.
    /// Prevents ghosts from interacting when <see cref="GhostComponent.CanGhostInteract"/> is false.
    /// </summary>
    public abstract class SharedGhostSystem : EntitySystem
    {
        [Dependency] protected readonly SharedPopupSystem Popup = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<GhostComponent, UseAttemptEvent>(OnAttempt);
            SubscribeLocalEvent<GhostComponent, InteractionAttemptEvent>(OnAttemptInteract);
            SubscribeLocalEvent<GhostComponent, EmoteAttemptEvent>(OnAttempt);
            SubscribeLocalEvent<GhostComponent, DropAttemptEvent>(OnAttempt);
            SubscribeLocalEvent<GhostComponent, PickupAttemptEvent>(OnAttempt);
            SubscribeLocalEvent<GhostComponent, InteractionVerbAttemptEvent>(OnAttempt);
        }

        private void OnAttemptInteract(Entity<GhostComponent> ent, ref InteractionAttemptEvent args)
        {
            if (!ent.Comp.CanGhostInteract)
                args.Cancelled = true;
        }

        private void OnAttempt(EntityUid uid, GhostComponent component, CancellableEntityEventArgs args)
        {
            if (!component.CanGhostInteract)
                args.Cancel();
        }

        public void SetTimeOfDeath(EntityUid uid, TimeSpan value, GhostComponent? component)
        {
            if (!Resolve(uid, ref component))
                return;

            component.TimeOfDeath = value;
        }

        public void SetCanReturnToBody(EntityUid uid, bool value, GhostComponent? component = null)
        {
            if (!Resolve(uid, ref component))
                return;

            component.CanReturnToBody = value;
        }

        public void SetCanReturnToBody(GhostComponent component, bool value)
        {
            component.CanReturnToBody = value;
        }
    }

    /// <summary>
    /// A client to server request to get places a ghost can warp to.
    /// Response is sent via <see cref="GhostWarpsResponseEvent"/>
    /// </summary>
    [Serializable, NetSerializable]
    public sealed class GhostWarpsRequestEvent : EntityEventArgs
    {
    }

    /// <summary>
    /// Goobstation - A server to client request for them to spawn at the ghost bar
    /// </summary>
    [Serializable, NetSerializable]
    public sealed class GhostBarSpawnEvent : EntityEventArgs
    {
    }

     // WWDP-Start
     /// <summary>
     /// An player body a ghost can warp to.
     /// This is used as part of <see cref="GhostWarpsResponseEvent"/>
     /// </summary>
     [Serializable, NetSerializable]
     public struct GhostWarp
     {
         public GhostWarp(NetEntity entity, string displayName, string subGroup, string description, Color? color)
         {
             Entity = entity;
             DisplayName = displayName;
             SubGroup = subGroup;
             Color = color;
             Description = description;
         }

         public NetEntity Entity { get; }

         public string DisplayName { get; }
         public string SubGroup { get; }
         public string Description { get; }

         public Color? Color { get; }

         public WarpGroup Group { get; set; } = WarpGroup.Location;
     }

     [Serializable, NetSerializable, Flags]
     public enum WarpGroup
     {
        Location = 0,
        Ghost = 1 << 0,
        Alive = 1 << 1,
        Dead =  1 << 2,
        Left =  1 << 3,
        Antag = 1 << 4,
        Department = 1 << 5,
        Other = 1 << 6,

        AliveAntag = Alive | Antag,
        DeadAntag = Dead | Antag,

        AliveDepartment = Alive | Department,
        DeadDepartment = Dead | Department,
        LeftDepartment = Left | Department,

        AliveOther = Alive | Other

     }


    /// <summary>
    /// A server to client response for a <see cref="GhostWarpsRequestEvent"/>.
    /// Contains players, and locations a ghost can warp to
    /// </summary>
    [Serializable, NetSerializable]
    public sealed class GhostWarpsResponseEvent : EntityEventArgs
    {
        public GhostWarpsResponseEvent(List<GhostWarp> warps)
        {
            Warps = warps;
        }

        /// <summary>
        /// A list of warps to teleport.
        /// </summary>
        public List<GhostWarp> Warps { get; }
    }

    /// <summary>
    ///  A client to server request for their ghost to be warped to an entity
    /// </summary>
    [Serializable, NetSerializable]
    public sealed class GhostWarpToTargetRequestEvent : EntityEventArgs
    {
        public NetEntity Target { get; }

        public GhostWarpToTargetRequestEvent(NetEntity target)
        {
            Target = target;
        }
    }

    /// <summary>
    /// A client to server request for their ghost to be warped to the most followed entity.
    /// </summary>
    [Serializable, NetSerializable]
    public sealed class GhostnadoRequestEvent : EntityEventArgs
    {
    }

    /// <summary>
    /// A client to server request for their ghost to return to body
    /// </summary>
    [Serializable, NetSerializable]
    public sealed class GhostReturnToBodyRequest : EntityEventArgs
    {
    }

    /// <summary>
    /// A server to client update with the available ghost role count
    /// </summary>
    [Serializable, NetSerializable]
    public sealed class GhostUpdateGhostRoleCountEvent : EntityEventArgs
    {
        public int AvailableGhostRoles { get; }

        public GhostUpdateGhostRoleCountEvent(int availableGhostRoleCount)
        {
            AvailableGhostRoles = availableGhostRoleCount;
        }
    }

    [Serializable, NetSerializable]
    public sealed class GhostReturnToRoundRequest : EntityEventArgs
    {
    }
}
