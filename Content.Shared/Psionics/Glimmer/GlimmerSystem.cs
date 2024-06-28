using Robust.Shared.Serialization;
using Robust.Shared.Configuration;
using Content.Shared.CCVar;
using Content.Shared.GameTicking;
using Content.Shared.Mobs;

namespace Content.Shared.Psionics.Glimmer
{
    /// <summary>
    /// This handles setting / reading the value of glimmer.
    /// </summary>
    public sealed class GlimmerSystem : EntitySystem
    {
        [Dependency] private readonly IConfigurationManager _cfg = default!;
        private float _glimmerInput = 0;
        /// <summary>
        ///     GlimmerInput represents the system-facing value of the station's glimmer, and is given by f(y) for this graph: https://www.desmos.com/calculator/posutiq38e
        ///     Where x = GlimmerOutput and y = GlimmerInput
        /// </summary>
        /// <remarks>
        ///     This is private set for a good reason, if you're looking to change it, do so via DeltaGlimmerInput or SetGlimmerInput
        /// </remarks>
        public float GlimmerInput
        {
            get { return _glimmerInput; }
            private set { _glimmerInput = _enabled ? Math.Max(value, 0) : 0; }
        }
        private float _glimmerOutput = 0;

        /// <summary>
        ///     Glimmer Output represents the player-facing value of the station's glimmer, and is given by f(x) for this graph: https://www.desmos.com/calculator/posutiq38e
        ///     Where x = GlimmerInput and y = GlimmerOutput
        /// </summary>
        /// <remarks>
        ///     This is private set for a good reason, if you're looking to change it, do so via DeltaGlimmerOutput or SetGlimmerOutput
        /// </remarks>
        public float GlimmerOutput
        {
            get { return _glimmerOutput; }
            private set { _glimmerOutput = _enabled ? Math.Clamp(value, 0, 999.999f) : 0; }
        }
        private bool _enabled;
        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<RoundRestartCleanupEvent>(Reset);
            _enabled = _cfg.GetCVar(CCVars.GlimmerEnabled);
            _cfg.OnValueChanged(CCVars.GlimmerEnabled, value => _enabled = value, true);
        }

        private void Reset(RoundRestartCleanupEvent args)
        {
            GlimmerInput = 0;
            GlimmerOutput = 0;
        }

        /// <summary>
        /// Return an abstracted range of a glimmer count.
        /// </summary>
        /// <param name="glimmer">What glimmer count to check. Uses the current glimmer by default.</param>
        public GlimmerTier GetGlimmerTier(float? glimmer = null)
        {
            if (glimmer == null)
                glimmer = GlimmerOutput;

            return glimmer switch
            {
                <= 49 => GlimmerTier.Minimal,
                >= 50 and <= 99 => GlimmerTier.Low,
                >= 100 and <= 299 => GlimmerTier.Moderate,
                >= 300 and <= 499 => GlimmerTier.High,
                >= 500 and <= 899 => GlimmerTier.Dangerous,
                _ => GlimmerTier.Critical,
            };
        }

        public void DeltaGlimmerInput(float delta)
        {
            if (_enabled && delta != 0)
            {
                GlimmerInput += delta;
                GlimmerOutput = 2000 / (1 + MathF.Pow(MathF.E, -.0022f * GlimmerInput)) - 1000;
            }
        }

        public void DeltaGlimmerOutput(float delta)
        {
            if (_enabled && delta != 0)
            {
                GlimmerOutput += delta;
                GlimmerInput = 2000 / (1 + MathF.Pow(MathF.E, -.0022f * GlimmerOutput)) - 1000;
            }
        }

        public void SetGlimmerOutput(float set)
        {
            if (_enabled && set != 0)
            {
                GlimmerOutput = set;
                GlimmerInput = 2000 / (1 + MathF.Pow(MathF.E, -.0022f * GlimmerOutput)) - 1000;
            }
        }

        public void SetGlimmerInput(float set)
        {
            if (_enabled && set != 0)
            {
                GlimmerInput = set;
                GlimmerOutput = 2000 / (1 + MathF.Pow(MathF.E, -.0022f * GlimmerOutput)) - 1000;
            }
        }
    }

    [Serializable, NetSerializable]
    public enum GlimmerTier : byte
    {
        Minimal,
        Low,
        Moderate,
        High,
        Dangerous,
        Critical,
    }
}
