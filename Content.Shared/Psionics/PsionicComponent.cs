using Content.Shared.Actions;
using Robust.Shared.GameStates;

namespace Content.Shared.Abilities.Psionics
{
    [RegisterComponent, NetworkedComponent]
    public sealed partial class PsionicComponent : Component
    {
        public EntityUid? PsionicAbility = null;

        /// <summary>
        ///     Ifrits, revenants, etc are explicitly magical beings that shouldn't get mindbreakered.
        /// </summary>
        [DataField("removable")]
        public bool Removable = true;

        [DataField("activePowers")]
        public HashSet<Component> ActivePowers = new();
    }
}
