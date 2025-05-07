using Content.Shared.Preferences;

namespace Content.Shared.Humanoid;

/// Holds all of the data for importing / exporting character profiles.
[DataDefinition]
public sealed partial class HumanoidProfileExport
{
    [DataField]
    public string ForkId;

    [DataField]
    public int Version = 1;

    [DataField(required: true)]
    public HumanoidCharacterProfile Profile = default!;
}
