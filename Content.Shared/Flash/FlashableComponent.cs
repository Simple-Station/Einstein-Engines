using Content.Shared.Physics;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Flash
{
    [RegisterComponent, NetworkedComponent]
    public sealed partial class FlashableComponent : Component
    {
        public float Duration;
        public TimeSpan LastFlash;

        // <summary>
        //   How much to modify the duration of flashes against this entity.
        // </summary>
        [DataField]
        public float DurationMultiplier = 1f;

        [DataField]
        public CollisionGroup CollisionGroup = CollisionGroup.Opaque;

        public override bool SendOnlyToOwner => true;
    }

    [Serializable, NetSerializable]
    public sealed class FlashableComponentState : ComponentState
    {
        public float Duration { get; }
        public TimeSpan Time { get; }

        // <summary>
        //   How much to modify the duration of flashes against this entity.
        // </summary>
        public float DurationMultiplier { get; }

        public FlashableComponentState(float duration, TimeSpan time, float durationMultiplier)
        {
            Duration = duration;
            Time = time;
            DurationMultiplier = durationMultiplier;
        }
    }

    [Serializable, NetSerializable]
    public enum FlashVisuals : byte
    {
        BaseLayer,
        LightLayer,
        Burnt,
        Flashing,
    }
}
