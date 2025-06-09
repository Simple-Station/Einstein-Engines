
using Content.Shared.Sound.Components;

namespace Content.Shared.Sound.Components
{
    /// <summary>
    /// Whenever a <see cref="InhaleLocationEvent"/> and internals are on, play a sound in PVS range.
    /// </summary>
    [RegisterComponent]
    public sealed partial class EmitSoundOnInternalsActiveComponent : BaseEmitSoundComponent
    {
    }
}
