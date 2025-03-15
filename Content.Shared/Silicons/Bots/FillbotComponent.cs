namespace Content.Shared.Silicons.Bots;

[RegisterComponent]
[Access(typeof(FillbotSystem))]
public sealed partial class FillbotComponent : Component
{
    [ViewVariables]
    public EntityUid? LinkedSinkEntity { get; set; }
}
