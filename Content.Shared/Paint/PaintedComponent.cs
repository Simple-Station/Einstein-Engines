using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Paint;

/// Component applied to target entity when painted
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PaintedComponent : Component
{
    [DataField, AutoNetworkedField]
    public Color Color = Color.FromHex("#2cdbd5");

    /// Used to remove the color when the component is removed
    [DataField, AutoNetworkedField]
    public Color BeforeColor;

    [DataField, AutoNetworkedField]
    public bool Enabled;

    // Not using ProtoId because ShaderPrototype is in Robust.Client
    [DataField, AutoNetworkedField]
    public string ShaderName = "Greyscale";
}

[Serializable, NetSerializable]
public enum PaintVisuals : byte
{
    Painted,
}
