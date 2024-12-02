#region

using Content.Client.UserInterface.Fragments;
using Robust.Client.UserInterface;

#endregion


namespace Content.Client.DeltaV.CartridgeLoader.Cartridges;


public sealed partial class CrimeAssistUi : UIFragment
{
    private CrimeAssistUiFragment? _fragment;

    public override Control GetUIFragmentRoot() => _fragment!;

    public override void Setup(BoundUserInterface userInterface, EntityUid? fragmentOwner) => _fragment = new();

    public override void UpdateState(BoundUserInterfaceState state) { }
}
