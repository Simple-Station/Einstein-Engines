using System.Linq;
using Content.Client.Humanoid;
using Content.Client.Inventory;
using Content.Client.Lobby.UI;
using Content.Client.Preferences;
using Content.Client.Preferences.UI;
using Content.Shared.Clothing.Loadouts.Systems;
using Content.Shared.GameTicking;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Robust.Client.State;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Client.Lobby;

public sealed class LobbyUIController : UIController, IOnStateEntered<LobbyState>, IOnStateExited<LobbyState>
{
    [Dependency] private readonly IClientPreferencesManager _preferencesManager = default!;
    [Dependency] private readonly IStateManager _stateManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [UISystemDependency] private readonly HumanoidAppearanceSystem _humanoid = default!;
    [UISystemDependency] private readonly ClientInventorySystem _inventory = default!;
    [UISystemDependency] private readonly LoadoutSystem _loadouts = default!;

    private LobbyCharacterPanel? _previewPanel;
    private HumanoidProfileEditor? _profileEditor;

    /*
     * Each character profile has its own dummy. There is also a dummy for the lobby screen + character editor
     * that is shared too.
     */

    /// <summary>
    ///     Preview dummy for role gear.
    /// </summary>
    private EntityUid? _previewDummy;

    [Access(typeof(HumanoidProfileEditor))]
    public bool UpdateClothes = true;
    [Access(typeof(HumanoidProfileEditor))]
    public bool ShowClothes = true;
    [Access(typeof(HumanoidProfileEditor))]
    public bool ShowLoadouts = true;

    // TODO: Load the species directly and don't update entity ever.
    public event Action<EntityUid>? PreviewDummyUpdated;

    public override void Initialize()
    {
        base.Initialize();

        _preferencesManager.OnServerDataLoaded += PreferencesDataLoaded;
    }

    private void PreferencesDataLoaded()
    {
        if (_previewDummy != null)
            EntityManager.DeleteEntity(_previewDummy);

        UpdateCharacterUI();
    }

    public void OnStateEntered(LobbyState state)
    {
    }

    public void OnStateExited(LobbyState state)
    {
        EntityManager.DeleteEntity(_previewDummy);
        _previewDummy = null;
    }

    public void SetPreviewPanel(LobbyCharacterPanel? panel)
    {
        _previewPanel = panel;
        UpdateCharacterUI();
    }

    public void SetProfileEditor(HumanoidProfileEditor? editor)
    {
        _profileEditor = editor;
        UpdateCharacterUI();
    }

    public void UpdateCharacterUI()
    {
        // Test moment
        if (_stateManager.CurrentState is not LobbyState)
            return;

        if (!_preferencesManager.ServerDataLoaded)
        {
            _previewPanel?.SetLoaded(false);
            return;
        }

        if (_previewDummy == null)
        {
            _previewDummy =
                EntityManager.SpawnEntity(
                    _prototypeManager.Index<SpeciesPrototype>(HumanoidCharacterProfile.DefaultWithSpecies().Species)
                        .DollPrototype, MapCoordinates.Nullspace);
            _previewPanel?.SetSprite(_previewDummy.Value);
        }

        _previewPanel?.SetLoaded(true);

        if (_preferencesManager.Preferences?.SelectedCharacter is not HumanoidCharacterProfile selectedCharacter)
            _previewPanel?.SetSummaryText(string.Empty);
        else if (_previewDummy != null)
        {
            var maybeProfile = _profileEditor?.Profile ?? selectedCharacter;
            _previewPanel?.SetSummaryText(maybeProfile.Summary);
            _humanoid.LoadProfile(_previewDummy.Value, maybeProfile);


            if (UpdateClothes)
            {
                RemoveDummyClothes(_previewDummy.Value);
                if (ShowClothes)
                    GiveDummyJobClothes(_previewDummy.Value, GetPreferredJob(maybeProfile), maybeProfile);
                if (ShowLoadouts)
                    GiveDummyLoadouts(_previewDummy.Value, GetPreferredJob(maybeProfile), maybeProfile);
                UpdateClothes = false;
            }

            PreviewDummyUpdated?.Invoke(_previewDummy.Value);
        }
    }


    /// <summary>
    ///     Gets the highest priority job for the profile.
    /// </summary>
    public JobPrototype GetPreferredJob(HumanoidCharacterProfile profile)
    {
        var highPriorityJob = profile.JobPriorities.FirstOrDefault(p => p.Value == JobPriority.High).Key;
        // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract (what is ReSharper smoking?)
        return _prototypeManager.Index<JobPrototype>(highPriorityJob ?? SharedGameTicker.FallbackOverflowJob);
    }

    public void RemoveDummyClothes(EntityUid dummy)
    {
        if (!_inventory.TryGetSlots(dummy, out var slots))
            return;

        foreach (var slot in slots)
            if (_inventory.TryUnequip(dummy, slot.Name, out var unequippedItem, silent: true, force: true, reparent: false))
                EntityManager.DeleteEntity(unequippedItem.Value);
    }

    /// <summary>
    ///     Applies the highest priority job's clothes and loadouts to the dummy.
    /// </summary>
    public void GiveDummyJobClothesLoadout(EntityUid dummy, HumanoidCharacterProfile profile)
    {
        var job = GetPreferredJob(profile);
        GiveDummyJobClothes(dummy, job, profile);
        GiveDummyLoadouts(dummy, job, profile);
    }

    public void GiveDummyLoadouts(EntityUid dummy, JobPrototype job, HumanoidCharacterProfile profile)
    {
        _loadouts.ApplyCharacterLoadout(dummy, job, profile);
    }

    /// <summary>
    ///     Applies the specified job's clothes to the dummy.
    /// </summary>
    public void GiveDummyJobClothes(EntityUid dummy, JobPrototype job, HumanoidCharacterProfile profile)
    {
        if (!_inventory.TryGetSlots(dummy, out var slots)
            || job.StartingGear == null)
            return;

        var gear = _prototypeManager.Index<StartingGearPrototype>(job.StartingGear);

        foreach (var slot in slots)
        {
            var itemType = gear.GetGear(slot.Name, profile);

            if (_inventory.TryUnequip(dummy, slot.Name, out var unequippedItem, silent: true, force: true, reparent: false))
                EntityManager.DeleteEntity(unequippedItem.Value);

            if (itemType == string.Empty)
                continue;

            var item = EntityManager.SpawnEntity(itemType, MapCoordinates.Nullspace);
            _inventory.TryEquip(dummy, item, slot.Name, true, true);
        }
    }

    public EntityUid? GetPreviewDummy()
    {
        return _previewDummy;
    }
}
