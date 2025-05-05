namespace Content.Server._EE.Shadowling.Objectives;

[RegisterComponent, Access(typeof(ShadowlingAscendConditionSystem))]
public sealed partial class ShadowlingAscendConditionComponent : Component
{
    [DataField]
    public bool Ascended = false;

    [DataField]
    public EntityUid MindId;
}

