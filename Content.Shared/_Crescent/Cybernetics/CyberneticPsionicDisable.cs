using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Cybernetics
{
    [RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
    public sealed partial class CyberneticPsionicDisableComponent : Component
    {
        // If they had psionic capabilities before having them suddenly revoked, at least give them the ability to get them back on removal.
        [DataField, AutoNetworkedField]
        public bool HadPsionics = false;

        // On the other hand, if they were pre-emptively psionically disabled (see: X-Waveform Misalignment), don't touch that state when it's removed (do nothing).
        [DataField, AutoNetworkedField]
        public bool WasPsionicallyDisabled = false;
    }
}
