using Content.Shared.Actions;
using Content.Shared.Antag;
using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared._Goobstation.Blob.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class BlobCarrierComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField("transformationDelay")]
    public float TransformationDelay = 600;

    [ViewVariables(VVAccess.ReadWrite), DataField("alertInterval")]
    public float AlertInterval = 30f;

    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan NextAlert = TimeSpan.FromSeconds(0);

    [ViewVariables(VVAccess.ReadWrite)]
    public bool HasMind = false;

    [ViewVariables(VVAccess.ReadWrite)]
    public float TransformationTimer = 0;

    [ViewVariables(VVAccess.ReadWrite),
     DataField("corePrototype", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string CoreBlobPrototype = "CoreBlobTile";

    public EntityUid? TransformToBlob = null;
}

public sealed partial class TransformToBlobActionEvent : InstantActionEvent
{

}

