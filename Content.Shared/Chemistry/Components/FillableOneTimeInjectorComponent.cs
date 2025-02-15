using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.DoAfter;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Chemistry.Components;

/// <summary>
/// Implements draw/inject behavior for syringes that can be filled once and then injected once.
/// </summary>
/// <seealso cref="SharedInjectorSystem"/>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class FillableOneTimeInjectorComponent : Component
{
    [DataField]
    public string SolutionName = "injector";

    /// <summary>
    ///     The minimum amount of solution that can be transferred at once from this solution.
    /// </summary>
    [DataField("minTransferAmount")]
    public FixedPoint2 MinimumTransferAmount = FixedPoint2.New(1);

    /// <summary>
    ///     The maximum amount of solution that can be transferred at once from this solution.
    /// </summary>
    [DataField("maxTransferAmount")]
    public FixedPoint2 MaximumTransferAmount = FixedPoint2.New(15);

    /// <summary>
    /// Amount to inject or draw on each usage. If the injector is inject only, it will
    /// attempt to inject it's entire contents upon use.
    /// </summary>
    [DataField]
    [AutoNetworkedField]
    public FixedPoint2 TransferAmount = FixedPoint2.New(1);

    /// <summary>
    /// Injection delay (seconds) when the target is a mob.
    /// </summary>
    /// <remarks>
    /// The base delay has a minimum of 1 second, but this will still be modified if the target is incapacitated or
    /// in combat mode.
    /// </remarks>
    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(5);

    /// <summary>
    /// The state of the injector. Determines it's attack behavior. Containers must have the
    /// right SolutionCaps to support injection/drawing.
    /// </summary>
    [AutoNetworkedField]
    [DataField]
    public FillableOneTimeInjectorToggleMode ToggleState = FillableOneTimeInjectorToggleMode.Draw;

    [AutoNetworkedField]
    [DataField]
    public bool HasDrawn = false;

    [AutoNetworkedField]
    [DataField]
    public bool HasInjected = false;
}

/// <summary>
/// Possible modes for an <see cref="FillableOneTimeInjectorComponent"/>.
/// </summary>
public enum FillableOneTimeInjectorToggleMode : byte
{
    /// <summary>
    /// The injector will try to inject reagent into things.
    /// </summary>
    Inject,

    /// <summary>
    /// The injector will try to draw reagent from things.
    /// </summary>
    Draw,

    /// <summary>
    /// The injector can no longer be used.
    /// </summary>
    Spent,
}
