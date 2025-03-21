using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Changeling;

[RegisterComponent, NetworkedComponent]
public sealed partial class ChangelingActionComponent : Component
{
    [DataField] public bool RequireBiomass = true;

    [DataField] public float ChemicalCost;

    [DataField] public float BiomassCost;

    [DataField] public bool UseInLastResort;

    [DataField] public bool UseInLesserForm;

    [DataField] public float RequireAbsorbed;
}

#region Events - Basic

public sealed partial class OpenEvolutionMenuEvent : InstantActionEvent { }
public sealed partial class AbsorbDNAEvent : EntityTargetActionEvent { }

public sealed partial class ChangelingInfectTargetEvent : EntityTargetActionEvent { }
public sealed partial class AbsorbBiomatterEvent : EntityTargetActionEvent { }
public sealed partial class StingExtractDNAEvent : EntityTargetActionEvent { }
public sealed partial class ChangelingTransformCycleEvent : InstantActionEvent { }
public sealed partial class ChangelingTransformEvent : InstantActionEvent { }
public sealed partial class EnterStasisEvent : InstantActionEvent { }
public sealed partial class ExitStasisEvent : InstantActionEvent { }

#endregion

#region Events - Combat

public sealed partial class ToggleArmbladeEvent : InstantActionEvent { }
public sealed partial class ToggleArmHammerEvent : InstantActionEvent { }
public sealed partial class ToggleArmClawEvent : InstantActionEvent { }
public sealed partial class ToggleDartGunEvent : InstantActionEvent { }
public sealed partial class CreateBoneShardEvent : InstantActionEvent { }
public sealed partial class ToggleChitinousArmorEvent : InstantActionEvent { }
public sealed partial class ToggleOrganicShieldEvent : InstantActionEvent { }
public sealed partial class ShriekDissonantEvent : InstantActionEvent { }
public sealed partial class ShriekResonantEvent : InstantActionEvent { }
public sealed partial class ToggleStrainedMusclesEvent : InstantActionEvent { }

#endregion

#region Events - Sting

public sealed partial class StingReagentEvent : EntityTargetActionEvent { }
public sealed partial class StingFakeArmbladeEvent : EntityTargetActionEvent { }
public sealed partial class StingTransformEvent : EntityTargetActionEvent { }
public sealed partial class StingLayEggsEvent : EntityTargetActionEvent { }

#endregion

#region Events - Utility

public sealed partial class ActionAnatomicPanaceaEvent : InstantActionEvent { }
public sealed partial class ActionBiodegradeEvent : InstantActionEvent { }
public sealed partial class ActionChameleonSkinEvent : InstantActionEvent { }
public sealed partial class ActionEphedrineOverdoseEvent : InstantActionEvent { }
public sealed partial class ActionFleshmendEvent : InstantActionEvent { }
public sealed partial class ActionLastResortEvent : InstantActionEvent { }
public sealed partial class ActionLesserFormEvent : InstantActionEvent { }
public sealed partial class ActionSpacesuitEvent : InstantActionEvent { }
public sealed partial class ActionHivemindAccessEvent : InstantActionEvent { }
public sealed partial class ActionContortBodyEvent : InstantActionEvent { }

#endregion

#region Events - Misc

[DataDefinition, NetSerializable, Serializable] // why did I have to readd netserializable to this? why did it not just inherit it?
public sealed partial class AugmentedEyesightPurchasedEvent : EntityEventArgs;

#endregion
