namespace Content.Server.Abilities.Chitinid;


[RegisterComponent]
public sealed partial class BlockInjectionComponent : Component
{
    [DataField]
    public string BlockReason { get; set; } = string.Empty;
}
