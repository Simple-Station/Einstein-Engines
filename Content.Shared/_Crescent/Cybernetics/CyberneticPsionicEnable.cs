using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Cybernetics
{
    [RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
    public sealed partial class CyberneticPsionicEnableComponent : Component
    {
        // We don't want to accidentally remove psionics from someone who had them before this was implanted. Keep records!
        [DataField, AutoNetworkedField]
        public bool HadPsionics = false;
    }
}
