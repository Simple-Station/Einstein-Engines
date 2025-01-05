using System.Numerics;
using Content.Shared.Chemistry.Components;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.FootPrint;

[RegisterComponent]
public sealed partial class FootPrintsComponent : Component
{
    [DataField]
    public ResPath RsiPath = new("/Textures/Effects/footprints.rsi");

    [DataField]
    public string
        LeftBarePrint = "footprint-left-bare-human",
        RightBarePrint = "footprint-right-bare-human",
        ShoesPrint = "footprint-shoes",
        SuitPrint = "footprint-suit";

    [DataField]
    public string[] DraggingPrint =
    [
        "dragging-1",
        "dragging-2",
        "dragging-3",
        "dragging-4",
        "dragging-5",
    ];

    [DataField]
    public EntProtoId<FootPrintComponent> StepProtoId = "Footstep";

    /// <summary>
    ///     The size scaling factor for footprint steps. Must be positive.
    /// </summary>
    [DataField]
    public float StepSize = 0.7f;

    /// <summary>
    ///     The size scaling factor for drag marks. Must be positive.
    /// </summary>
    [DataField]
    public float DragSize = 0.5f;

    /// <summary>
    ///     Horizontal offset of the created footprints relative to the center.
    /// </summary>
    [DataField]
    public Vector2 OffsetPrint = new(0.1f, 0f);

    /// <summary>
    ///     Tracks which foot should make the next print. True for right foot, false for left.
    /// </summary>
    public bool RightStep = true;

    /// <summary>
    ///     The position of the last footprint in world coordinates.
    /// </summary>
    public Vector2 LastStepPos = Vector2.Zero;

    [DataField]
    public HashSet<string> DNAs = new();

    /// <summary>
    ///     Reagent volume used for footprints.
    /// </summary>
    [DataField]
    public Solution ContainedSolution = new(3) { CanReact = true, MaxVolume = 5f, };

    /// <summary>
    ///     Amount of reagents used per footprint.
    /// </summary>
    [DataField]
    public FixedPoint2 FootprintVolume = 1f;
}
