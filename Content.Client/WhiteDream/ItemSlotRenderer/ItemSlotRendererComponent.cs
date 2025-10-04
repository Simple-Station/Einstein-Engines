using Robust.Client.Graphics;

namespace Content.Client.WhiteDream.ItemSlotRenderer;

[RegisterComponent]
public sealed partial class ItemSlotRendererComponent : Component
{
    // [slotId] = layer mapping (in string form)
    [DataField("mapping")]
    public Dictionary<string, string> PrototypeLayerMappings = new();

    // [mapkey] = slotId
    [ViewVariables(VVAccess.ReadWrite)]
    public List<(object, string)> LayerMappings = new();

    // [slotId] = entity uid
    [ViewVariables(VVAccess.ReadOnly)]
    public Dictionary<string, EntityUid?> CachedEntities = new();

    // [slotId] = IRenderTexture
    [ViewVariables(VVAccess.ReadOnly)]
    public Dictionary<string, IRenderTexture> CachedRT = new();

    [DataField]
    public bool ErrorOnMissing = true;

    [DataField]
    public Vector2i RenderTargetSize = new Vector2i(32, 32);
}
