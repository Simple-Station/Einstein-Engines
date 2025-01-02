using Content.Shared.NPC.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;


namespace Content.Shared.Nyanotrasen.NPC.Components.Faction
{
    [RegisterComponent]
    /// <summary>
    /// Allows clothing to add a faction to you when you wear it.
    /// </summary>
    public sealed partial class ClothingAddFactionComponent : Component
    {
        public bool IsActive = false;

        /// <summary>
        /// Faction added
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite),
         DataField("faction", required: true, customTypeSerializer:typeof(PrototypeIdSerializer<NpcFactionPrototype>))]
        public string Faction = "";
    }
}
