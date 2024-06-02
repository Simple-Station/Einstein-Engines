using Robust.Shared.GameStates;

namespace Content.Shared.DetailExaminable
{
    [RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
    public sealed partial class DetailExaminableComponent : Component
    {
        [DataField("content", required: true)]
        [ViewVariables(VVAccess.ReadWrite)]
        [AutoNetworkedField]
        public string Content = "";
    }
}
