namespace Content.Server._Crescent;

/// <summary>
/// This handles...
/// </summary>
///
[RegisterComponent]
public sealed partial class ShipPriceMultiplierComponent : Component
{
    public float priceMultiplier = 0.1f;
}
public sealed class ShipPriceMultiplierSystem : EntitySystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {


    }
}
