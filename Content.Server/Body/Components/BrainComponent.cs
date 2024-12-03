using Content.Server.Body.Systems;

namespace Content.Server.Body.Components
{
    [RegisterComponent, Access(typeof(BrainSystem))]
    public sealed partial class BrainComponent : Component
    {
        /// <summary>
        ///     Is this brain currently controlling the entity?
        /// </summary>
        [DataField]
        public bool Active = true;
    }
}
