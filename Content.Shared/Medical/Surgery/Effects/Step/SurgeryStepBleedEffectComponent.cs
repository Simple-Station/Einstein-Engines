using Robust.Shared.GameStates;

namespace Content.Shared.Medical.Surgery.Effects.Step;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SurgeryStepBleedEffectComponent : Component
{
    [DataField, AutoNetworkedField]
    public int Damage;
}