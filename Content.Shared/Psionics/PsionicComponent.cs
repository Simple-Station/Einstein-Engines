using Content.Shared.Psionics;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Abilities.Psionics
{
    [RegisterComponent, NetworkedComponent]
    public sealed partial class PsionicComponent : Component
    {
        public EntityUid? PsionicAbility = null;

        /// <summary>
        ///     Ifrits, revenants, etc are explicitly magical beings that shouldn't get mindbreakered.
        /// </summary>
        [DataField]
        public bool Removable = true;

        /// <summary>
        ///     The list of all powers currently active on a Psionic, by power Prototype.
        ///     TODO: Not in this PR due to scope, but this needs to go to Server and not Shared.
        /// </summary>
        [DataField]
        public HashSet<PsionicPowerPrototype> ActivePowers = new();

        /// <summary>
        ///     The list of each Psionic Power by action with entityUid.
        /// </summary>
        [DataField]
        public List<(EntProtoId Id, EntityUid? Entity)> Actions = new();

        /// <summary>
        ///     What sources of Amplification does this Psion have?
        /// </summary>
        [ViewVariables(VVAccess.ReadOnly)]
        public readonly Dictionary<string, float> AmplificationSources = new();

        /// <summary>
        ///     A measure of how "Powerful" a Psion is.
        ///     TODO: Implement this in a separate PR.
        /// </summary>
        [ViewVariables(VVAccess.ReadOnly)]
        public float CurrentAmplification;

        /// <summary>
        ///     What sources of Dampening does this Psion have?
        /// </summary>
        [ViewVariables(VVAccess.ReadOnly)]
        public readonly Dictionary<string, float> DampeningSources = new();

        /// <summary>
        ///     A measure of how "Controlled" a Psion is.
        ///     TODO: Implement this in a separate PR.
        /// </summary>
        [ViewVariables(VVAccess.ReadOnly)]
        public float CurrentDampening;

        /// <summary>
        ///     How close a Psion is to awakening a new power.
        ///     TODO: Implement this in a separate PR.
        /// </summary>
        [DataField]
        public float Potentia = 0;

        /// <summary>
        ///     The baseline chance of obtaining a psionic power when rolling for one.
        /// </summary>
        [DataField]
        public float Chance = 0.04f;

        /// <summary>
        ///     Whether or not a Psion has an available "Reroll" to spend on attempting to gain powers.
        /// </summary>
        [DataField]
        public bool Rerolled;
    }
}
