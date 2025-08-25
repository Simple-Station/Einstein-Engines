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
    }
}
