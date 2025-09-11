using Robust.Shared.GameStates;


namespace Content.Shared.Crescent.Radar;


/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class EntityIFFDesignationComponent : Component
{
    [DataField, AutoNetworkedField]
    public string Name = "";
}
