#region

using Content.Shared.Cargo;
using Robust.Client.GameObjects;

#endregion


namespace Content.Client.Cargo.Systems;


public sealed partial class CargoSystem : SharedCargoSystem
{
    [Dependency] private readonly AnimationPlayerSystem _player = default!;

    public override void Initialize()
    {
        base.Initialize();
        InitializeCargoTelepad();
    }
}
