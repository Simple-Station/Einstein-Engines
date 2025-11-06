using Content.Shared._Crescent.Blocking;
using Content.Shared.Blocking;
using Robust.Shared.GameStates;

namespace Content.Client._Crescent.Blocking.Components;

/// <summary>
/// This component gets dynamically added to an Entity via the <see cref="BlockingSystem"/> if the IsClothing is true
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(SharedBlockingSystem))]
[AutoGenerateComponentState]
public sealed partial class BlockingVisualsComponent : Component
{
    /// <summary>
    /// Doesnt really do anything lol
    /// </summary>
    [DataField("enabled")]
    [AutoNetworkedField]
    public bool Enabled = true;
}
