using Content.Client.Message;
using Content.Client.Stylesheets;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.FixedPoint;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Timing;

namespace Content.Client.Chemistry.UI;

public sealed class FillableOneTimeInjectorStatusControl : Control
{
    private readonly Entity<FillableOneTimeInjectorComponent> _parent;
    private readonly SharedSolutionContainerSystem _solutionContainers;
    private readonly RichTextLabel _label;

    private FixedPoint2 PrevVolume;
    private FixedPoint2 PrevMaxVolume;
    private FixedPoint2 PrevTransferAmount;
    private FillableOneTimeInjectorToggleMode PrevToggleStateIndex;

    public FillableOneTimeInjectorStatusControl(Entity<FillableOneTimeInjectorComponent> parent, SharedSolutionContainerSystem solutionContainers)
    {
        _parent = parent;
        _solutionContainers = solutionContainers;
        _label = new RichTextLabel { StyleClasses = { StyleNano.StyleClassItemStatus } };
        AddChild(_label);
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        if (!_solutionContainers.TryGetSolution(_parent.Owner, _parent.Comp.SolutionName, out _, out var solution))
            return;

        // only updates the UI if any of the details are different than they previously were
        if (PrevVolume == solution.Volume
            && PrevMaxVolume == solution.MaxVolume
            && PrevTransferAmount == _parent.Comp.TransferAmount)
            return;

        PrevVolume = solution.Volume;
        PrevMaxVolume = solution.MaxVolume;
        PrevTransferAmount = _parent.Comp.TransferAmount;
        var modeStringLocalized = "";

        // only updates the UI if any of the details are different than they previously were
        if(PrevToggleStateIndex == _parent.Comp.ToggleState)
            return;

        PrevToggleStateIndex = _parent.Comp.ToggleState;

        // Update current volume and injector state
        modeStringLocalized = Loc.GetString(
            _parent.Comp.ToggleState switch
            {
                FillableOneTimeInjectorToggleMode.Draw => "injector-draw-text",
                FillableOneTimeInjectorToggleMode.Inject => "injector-inject-text",
                FillableOneTimeInjectorToggleMode.Spent => "injector-spent-text",
                _ => "injector-invalid-injector-toggle-mode"
            });

        if (_parent.Comp.ToggleState != FillableOneTimeInjectorToggleMode.Draw)
        {
            _label.SetMarkup(
                Loc.GetString(
                    "onetime-injector-simple-volume-label",
                    ("currentVolume", solution.Volume),
                    ("modeString", modeStringLocalized)));
        }
        else
        {
            _label.SetMarkup(
                Loc.GetString(
                    "injector-volume-label",
                    ("currentVolume", solution.Volume),
                    ("totalVolume", solution.MaxVolume),
                    ("modeString", modeStringLocalized),
                    ("transferVolume", _parent.Comp.TransferAmount)));
        }
    }
}
