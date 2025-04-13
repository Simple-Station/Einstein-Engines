using Content.Shared.Whitelist;

namespace Content.Shared.Psionics;

[RegisterComponent]
public sealed partial class PsionicInvisibleContactsComponent : Component
{
    [DataField(required: true)]
    public EntityWhitelist Whitelist = default!;

    /// <summary>
    /// This tracks how many valid entities are being contacted,
    /// so when you stop touching one, you don't immediately lose invisibility.
    /// </summary>
    [DataField]
    public int Stages = 0;
}
