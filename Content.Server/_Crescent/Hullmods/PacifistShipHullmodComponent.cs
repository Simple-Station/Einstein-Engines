

namespace Content.Server._Crescent.Hullmods
{
    [RegisterComponent]
    public sealed partial class PacifistShipHullmodComponent : Component
    {
        /// <summary>
        ///     Disable the use of tools on the entity.
        /// </summary>
        [DataField, ViewVariables(VVAccess.ReadWrite)]
        public bool universal = false;


    }
}
