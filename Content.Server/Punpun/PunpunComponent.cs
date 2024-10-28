namespace Content.Server.Punpun;

[RegisterComponent]
public sealed partial class PunpunComponent : Component
{
    /// How many rounds Punpun will be around for before disappearing with a note
    [DataField]
    public int Lifetime = 14;
}
