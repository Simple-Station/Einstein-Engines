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
        //   Chance to get EyeDamage on flash
        // </summary>
        [DataField]
        public float EyeDamageChance;

        // <summary>
        //   How many EyeDamage when flashed? (If EyeDamageChance check passed)
        // </summary>
        [DataField]
        public int EyeDamage;
        
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
        public float EyeDamageChance { get; }
        public int EyeDamage { get; }
        public float DurationMultiplier { get; }

        public FlashableComponentState(float duration, TimeSpan time, float eyeDamageChance, int eyeDamage, float durationMultiplier)
        {
            Duration = duration;
            Time = time;
            EyeDamageChance = eyeDamageChance;
            EyeDamage = eyeDamage;
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
