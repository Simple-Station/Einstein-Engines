using System.Collections.Generic;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Content.Shared.Psionics;

namespace Content.Shared._Crescent.Magic
{
    [RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
    public sealed partial class PsionicBrainComponent : Component
    {
        // networking is not fucking me over this time
        [DataField, AutoNetworkedField]
        public List<string> PowerPrototypes = new();

        // Do you need to be a Psion to take the power from this brain?
        [DataField, AutoNetworkedField]
        public bool RequiresLatent = true;

        // What's the per-power chance of getting a power from this brain?
        [DataField, AutoNetworkedField]
        public float ChancePerPower = 1.0f;
    }
}
