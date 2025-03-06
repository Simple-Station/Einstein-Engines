using Content.Shared.Containers.ItemSlots;
using Content.Shared.Inventory;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Backmen.ModSuits.Components;

/// <summary>
///     This component gives an item an action that will equip or un-equip some clothing e.g. hardsuits and hardsuit helmets.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ModSuitComponent : Component
{
    public const string DefaultClothingContainerId = "modsuit-part";
    public const string DefaultModuleContainerId = "modsuit-mod";

    /// <summary>
    ///     Action used to toggle the clothing on or off.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntProtoId Action = "ActionToggleMODPiece";

    [DataField, AutoNetworkedField]
    public EntityUid? ActionEntity;

    /// <summary>
    ///     The said ever-consuming container of modules
    /// </summary>
    public List<ItemSlot> ModuleSlots = new();

    /// <summary>
    ///     How much complexity can this mod suit fit?
    /// </summary>
    [DataField, AutoNetworkedField]
    public int MaxComplexity = 15;

    /// <summary>
    ///     Current complexity this mod suit is holding.
    /// </summary>
    [ViewVariables]
    [AutoNetworkedField]
    public int CurrentComplexity = 0;

    /// <summary>
    ///     The inventory slot that the clothing is equipped to.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField, AutoNetworkedField]
    public string Slot = string.Empty;

    /// <summary>
    ///     The inventory slot, that will always be deployed first.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField, AutoNetworkedField]
    public string FirstSlotToDeploy = "outerClothing";

    /// <summary>
    ///     Dictionary of inventory slots and entity prototypes to spawn into the clothing container.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Dictionary<string, EntProtoId> ClothingPrototypes = new();

    [DataField, AutoNetworkedField]
    public List<EntProtoId> InnateModules = new();

    /// <summary>
    ///     Dictionary of clothing uids and slots
    /// </summary>
    [DataField, AutoNetworkedField]
    public Dictionary<EntityUid, string> ClothingUids = new();

    /// <summary>
    ///     Entities queued for deployment
    /// </summary>
    [ViewVariables]
    public Dictionary<string, EntityUid> EntitiesToDeploy = new();

    /// <summary>
    ///     Is the modsuit actively deploying itself?
    /// </summary>
    [ViewVariables]
    public bool BeingDeployed = false;

    /// <summary>
    ///     The container that the clothing is stored in when not equipped.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string ContainerId = DefaultClothingContainerId;

    /// <summary>
    ///     The container that the modules are stored in.
    /// </summary>
    [DataField, AutoNetworkedField, ]
    public string ModuleContainerId = DefaultModuleContainerId;

    [ViewVariables]
    public Container? Container;

    /// <summary>
    ///     Time it takes for this clothing to toggle one part of a mod suit
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan ModPartToggleDelay = TimeSpan.FromSeconds(1.7f);

    /// <summary>
    ///     Text shown in the toggle-clothing verb. Defaults to using the name of the <see cref="ActionEntity"/> action.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string? VerbText;

    /// <summary>
    ///     If true it will block unequip of this entity until all attached clothing are removed
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool BlockUnequipWhenAttached = true;

    /// <summary>
    ///     If true all attached will replace already equipped clothing on equip attempt
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool ReplaceCurrentClothing = true;

    [DataField("requiredSlot"), AutoNetworkedField]
    public SlotFlags RequiredFlags = SlotFlags.BACK;
}
