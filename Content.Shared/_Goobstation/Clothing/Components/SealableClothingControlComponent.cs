using Content.Shared._Goobstation.Clothing.Systems;
using Content.Shared.Inventory;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Clothing.Components;

/// Component used to designate contol of sealable clothing. It'll contain action to seal clothing
[RegisterComponent]
[NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedSealableClothingSystem))]
public sealed partial class SealableClothingControlComponent : Component
{
    /// Action that used to start sealing
    [DataField, AutoNetworkedField]
    public EntProtoId SealAction = "ActionClothingSeal";

    [DataField, AutoNetworkedField]
    public EntityUid? SealActionEntity;

    /// Slot required for control to show action
    [DataField("requiredSlot"), AutoNetworkedField]
    public SlotFlags RequiredControlSlot = SlotFlags.BACK;

    /// True if clothing in sealing/unsealing process, false if not
    [DataField, AutoNetworkedField]
    public bool IsInProcess = false;

    /// True if clothing is currently sealed and need to start unsealing process. False if opposite
    [DataField, AutoNetworkedField]
    public bool IsCurrentlySealed = false;

    /// Queue of attached parts that should be sealed/unsealed
    [DataField, AutoNetworkedField]
    public Queue<NetEntity> ProcessQueue = new();

    /// Uid of entity that currently wear seal control
    [DataField, AutoNetworkedField]
    public EntityUid? WearerEntity;

    /// Doafter time for other players to start sealing via stripping menu
    [DataField, AutoNetworkedField]
    public TimeSpan NonWearerSealingTime = TimeSpan.FromSeconds(4);

    #region Popups & Sounds

    [DataField]
    public LocId ToggleFailedPopup = "sealable-clothing-equipment-not-toggled";

    [DataField]
    public LocId SealFailedPopup = "sealable-clothing-equipment-seal-failed";

    [DataField]
    public LocId SealedInProcessToggleFailPopup = "sealable-clothing-sealed-process-toggle-fail";

    [DataField]
    public LocId UnsealedInProcessToggleFailPopup = "sealable-clothing-unsealed-process-toggle-fail";

    [DataField]
    public LocId CurrentlySealedToggleFailPopup = "sealable-clothing-sealed-toggle-fail";

    [DataField]
    public LocId VerbText = "sealable-clothing-seal-verb";

    [DataField]
    public SoundSpecifier FailSound = new SoundPathSpecifier("/Audio/Machines/scanbuzz.ogg");

    [DataField]
    public SoundSpecifier SealCompleteSound = new SoundPathSpecifier("/Audio/_Goobstation/Mecha/nominal.ogg");

    [DataField]
    public SoundSpecifier UnsealCompleteSound = new SoundPathSpecifier("/Audio/_Goobstation/Machines/computer_end.ogg");
    #endregion
}
