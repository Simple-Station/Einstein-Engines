#region

using Content.Client.UserInterface.Fragments;
using Content.Shared.Mech;
using Robust.Client.UserInterface;

#endregion


namespace Content.Client.Mech.Ui.Equipment;


public sealed partial class MechSoundboardUi : UIFragment
{
    private MechSoundboardUiFragment? _fragment;

    public override Control GetUIFragmentRoot() => _fragment!;

    public override void Setup(BoundUserInterface userInterface, EntityUid? fragmentOwner)
    {
        if (fragmentOwner == null)
            return;

        _fragment = new();
        _fragment.OnPlayAction += sound =>
        {
            // TODO: IDK dog
            userInterface.SendMessage(
                new MechSoundboardPlayMessage(
                    IoCManager.Resolve<IEntityManager>().GetNetEntity(fragmentOwner.Value),
                    sound));
        };
    }

    public override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is not MechSoundboardUiState soundboardState)
            return;

        _fragment?.UpdateContents(soundboardState);
    }
}
