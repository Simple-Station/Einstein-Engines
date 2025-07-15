using Content.Shared.Maps;


namespace Content.Shared._Crescent;


/// <summary>
/// This handles...
/// </summary>
public sealed class DirectionalTilingSystem : EntitySystem
{
    [Dependency] private readonly TileSystem _tileSystem = default!;
    public override void Initialize()
    {

    }
}
