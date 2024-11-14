using Robust.Shared.GameStates;
using Robust.Shared.Audio;

namespace Content.Shared.Paint;

///  Removes paint from an entity that was painted with spray paint
[RegisterComponent, NetworkedComponent]
[Access(typeof(PaintRemoverSystem))]
public sealed partial class PaintRemoverComponent : Component
{
    /// Sound played when target is cleaned
    [DataField]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/Effects/Fluids/watersplash.ogg");

    [DataField]
    public float CleanDelay = 2f;
}
