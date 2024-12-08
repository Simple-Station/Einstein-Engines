using System.Linq;
using Content.Client.Guidebook;
using Content.Client.Humanoid;
using Content.Client.Inventory;
using Content.Client.Lobby.UI;
using Content.Client.Players.PlayTimeTracking;
using Content.Client.Station;
using Content.Shared.CCVar;
using Content.Shared.Clothing.Loadouts.Prototypes;
using Content.Shared.Clothing.Loadouts.Systems;
using Content.Shared.GameTicking;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Content.Shared.Traits;
using Robust.Client.Player;
using Robust.Client.ResourceManagement;
using Robust.Client.State;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;
using static Content.Shared.Humanoid.SharedHumanoidAppearanceSystem;
using CharacterSetupGui = Content.Client.Lobby.UI.CharacterSetupGui;
using HumanoidProfileEditor = Content.Client.Lobby.UI.HumanoidProfileEditor;

namespace Content.Client.Lobby;

public sealed class LobbyUIController : UIController, IOnStateEntered<LobbyState>, IOnStateExited<LobbyState>
{
    [Dependency] private readonly IClientPreferencesManager _preferencesManager = default!;
    [Dependency] private readonly IConfigurationManager _configurationManager = default!;
    [Dependency] private readonly IFileDialogManager _dialogManager = default!;
    [Dependency] private readonly ILogManager _logManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IResourceCache _resourceCache = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IStateManager _stateManager = default!;
    [Dependency] private readonly JobRequirementsManager _jobRequirements = default!;
    [Dependency] private readonly MarkingManager _markings = default!;
    [UISystemDependency] private readonly HumanoidAppearanceSystem _humanoid = default!;
    [UISystemDependency] private readonly ClientInventorySystem _inventory = default!;
    [UISystemDependency] private readonly GuidebookSystem _guide = default!;
    [UISystemDependency] private readonly SharedLoadoutSystem _loadouts = default!;
    [UISystemDependency] private readonly StationSpawningSystem _stationSpawning = default!;

    private CharacterSetupGui? _characterSetup;
    private HumanoidProfileEditor? _profileEditor;
    private CharacterSetupGuiSavePanel? _savePanel;

    /// <summary>
    /// This is the characher preview panel in the chat. This should only update if their character updates.
    /// </summary>
    private LobbyCharacterPreviewPanel? PreviewPanel => GetLobbyPreview();

    /// <summary>
    /// This is the modified profile currently being edited.
    /// </summary>
    private HumanoidCharacterProfile? EditedProfile => _profileEditor?.Profile;

    private int? EditedSlot => _profileEditor?.CharacterSlot;

    public override void Initialize()
    {
        base.Initialize();

        _prototypeManager.PrototypesReloaded += OnPrototypesReloaded;
        _preferencesManager.OnServerDataLoaded += PreferencesDataLoaded;
        _jobRequirements.Updated += OnRequirementsUpdated;

        _configurationManager.OnValueChanged(CCVars.FlavorText, _ => _profileEditor?.RefreshFlavorText());
        _configurationManager.OnValueChanged(CCVars.GameRoleTimers, _ => RefreshProfileEditor());
        _configurationManager.OnValueChanged(CCVars.GameRoleWhitelist, _ => RefreshProfileEditor());

        _preferencesManager.OnServerDataLoaded += PreferencesDataLoaded;
    }

    private LobbyCharacterPreviewPanel? GetLobbyPreview()
    {
        if (_stateManager.CurrentState is LobbyState lobby)
            return lobby.Lobby?.CharacterPreview;

        return null;
    }

    private void OnRequirementsUpdated()
    {
        if (_profileEditor == null)
            return;

        _profileEditor.RefreshAntags();
        _profileEditor.RefreshJobs();
    }


    private void OnPrototypesReloaded(PrototypesReloadedEventArgs obj)
    {
        if (_profileEditor == null)
            return;

        if (obj.WasModified<SpeciesPrototype>())
            _profileEditor.RefreshSpecies();

        if (obj.WasModified<AntagPrototype>())
            _profileEditor.RefreshAntags();

        if (obj.WasModified<JobPrototype>()
            || obj.WasModified<DepartmentPrototype>())
            _profileEditor.RefreshJobs();

        if (obj.WasModified<TraitPrototype>())
            _profileEditor.UpdateTraits(null, true);

        if (obj.WasModified<LoadoutPrototype>())
            _profileEditor.UpdateLoadouts(null, true);
    }

    private void PreferencesDataLoaded()
    {
        PreviewPanel?.SetLoaded(true);

        if (_stateManager.CurrentState is not LobbyState)
            return;

        ReloadCharacterSetup();
    }

    public void OnStateEntered(LobbyState state)
    {
        PreviewPanel?.SetLoaded(_preferencesManager.ServerDataLoaded);
        ReloadCharacterSetup();
    }

    public void OnStateExited(LobbyState state)
    {
        PreviewPanel?.SetLoaded(false);
        _profileEditor?.Dispose();
        _characterSetup?.Dispose();

        _characterSetup = null;
        _profileEditor = null;
    }

    /// <summary>
    /// Reloads every single character setup control.
    /// </summary>
    public void ReloadCharacterSetup()
    {
        RefreshLobbyPreview();
        var (characterGui, profileEditor) = EnsureGui();
        characterGui.ReloadCharacterPickers();
        profileEditor.SetProfile(
            (HumanoidCharacterProfile?) _preferencesManager.Preferences?.SelectedCharacter,
            _preferencesManager.Preferences?.SelectedCharacterIndex);
    }

    /// <summary>
    /// Refreshes the character preview in the lobby chat.
    /// </summary>
    private void RefreshLobbyPreview()
    {
        if (PreviewPanel == null)
            return;

        // Get selected character, load it, then set it
        var character = _preferencesManager.Preferences?.SelectedCharacter;

        if (character is not HumanoidCharacterProfile humanoid)
        {
            PreviewPanel.SetSprite(EntityUid.Invalid);
            PreviewPanel.SetSummaryText(string.Empty);
            return;
        }

        var dummy = LoadProfileEntity(humanoid, null, true, true);
        PreviewPanel.SetSprite(dummy);
        PreviewPanel.SetSummaryText(humanoid.Summary);
    }

    private void RefreshProfileEditor()
    {
        _profileEditor?.RefreshAntags();
        _profileEditor?.RefreshJobs();
    }

    private void SaveProfile()
    {
        DebugTools.Assert(EditedProfile != null);

        if (EditedProfile == null || EditedSlot == null)
            return;

        var selected = _preferencesManager.Preferences?.SelectedCharacterIndex;
        if (selected == null)
            return;

        _preferencesManager.UpdateCharacter(EditedProfile, EditedSlot.Value);
        ReloadCharacterSetup();
    }

    private void CloseProfileEditor()
    {
        if (_profileEditor == null)
            return;

        _profileEditor.SetProfile(null, null);
        _profileEditor.Visible = false;

        if (_stateManager.CurrentState is LobbyState lobbyGui)
            lobbyGui.SwitchState(LobbyGui.LobbyGuiState.Default);
    }

    private void OpenSavePanel()
    {
        if (_savePanel is { IsOpen: true })
            return;

        _savePanel = new CharacterSetupGuiSavePanel();

        _savePanel.SaveButton.OnPressed += _ =>
        {
            SaveProfile();

            _savePanel.Close();

            CloseProfileEditor();
        };

        _savePanel.NoSaveButton.OnPressed += _ =>
        {
            _savePanel.Close();

            CloseProfileEditor();
        };

        _savePanel.OpenCentered();
    }

    private (CharacterSetupGui, HumanoidProfileEditor) EnsureGui()
    {
        if (_characterSetup != null && _profileEditor != null)
        {
            _characterSetup.Visible = true;
            _profileEditor.Visible = true;
            return (_characterSetup, _profileEditor);
        }

        _profileEditor = new HumanoidProfileEditor(
            _preferencesManager,
            _configurationManager,
            EntityManager,
            _dialogManager,
            _logManager,
            _playerManager,
            _prototypeManager,
            _resourceCache,
            _jobRequirements,
            _markings,
            _random);

        _profileEditor.OnOpenGuidebook += _guide.OpenHelp;

        _characterSetup = new CharacterSetupGui(_profileEditor);

        _characterSetup.CloseButton.OnPressed += _ =>
        {
            // Open the save panel if we have unsaved changes.
            if (_profileEditor.Profile != null && _profileEditor.IsDirty)
            {
                OpenSavePanel();

                return;
            }

            // Reset sliders etc.
            CloseProfileEditor();
        };

        _profileEditor.Save += SaveProfile;

        _characterSetup.SelectCharacter += args =>
        {
            _preferencesManager.SelectCharacter(args);
            ReloadCharacterSetup();
        };

        _characterSetup.DeleteCharacter += args =>
        {
            _preferencesManager.DeleteCharacter(args);

            if (EditedSlot == args)
                // Reload everything
                ReloadCharacterSetup();
            else
                // Only need to reload character pickers
                _characterSetup?.ReloadCharacterPickers();
        };

        if (_stateManager.CurrentState is LobbyState lobby)
            lobby.Lobby?.CharacterSetupState.AddChild(_characterSetup);

        return (_characterSetup, _profileEditor);
    }

    #region Helpers

    /// <summary>
    /// Gets the highest priority job for the profile.
    /// </summary>
    public JobPrototype GetPreferredJob(HumanoidCharacterProfile profile)
    {
        var highPriorityJob = profile.JobPriorities.FirstOrDefault(p => p.Value == JobPriority.High).Key;
        // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract (what is resharper smoking?)
        return _prototypeManager.Index<JobPrototype>(highPriorityJob.Id ?? SharedGameTicker.FallbackOverflowJob);
    }

    /// <summary>
    /// Applies loadouts to the dummy.
    /// </summary>
    public void GiveDummyLoadout(EntityUid dummy, JobPrototype job, HumanoidCharacterProfile profile)
    {
        _loadouts.ApplyCharacterLoadout(dummy, job, profile, _jobRequirements.GetRawPlayTimeTrackers(), _jobRequirements.IsWhitelisted(), out _);
    }

    /// <summary>
    /// Applies the specified job's clothes to the dummy.
    /// </summary>
    public void GiveDummyJobClothes(EntityUid dummy, JobPrototype job, HumanoidCharacterProfile profile)
    {
        if (!_inventory.TryGetSlots(dummy, out var slots)
            || job.StartingGear == null)
            return;

        var gear = _prototypeManager.Index<StartingGearPrototype>(job.StartingGear);
        gear = _stationSpawning.ApplySubGear(gear, profile, job);

        foreach (var slot in slots)
        {
            var itemType = gear.GetGear(slot.Name);

            if (_inventory.TryUnequip(dummy, slot.Name, out var unequippedItem, silent: true, force: true, reparent: false))
                EntityManager.DeleteEntity(unequippedItem.Value);

            if (itemType == string.Empty)
                continue;

            var item = EntityManager.SpawnEntity(itemType, MapCoordinates.Nullspace);
            _inventory.TryEquip(dummy, item, slot.Name, true, true);
        }
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
    /// Loads the profile onto a dummy entity.
    /// </summary>
    public EntityUid LoadProfileEntity(HumanoidCharacterProfile? humanoid, JobPrototype? job, bool jobClothes, bool loadouts)
    {
        EntityUid dummyEnt;

        EntProtoId? previewEntity = null;
        if (humanoid != null && jobClothes)
        {
            job ??= GetPreferredJob(humanoid);

            previewEntity = job.JobPreviewEntity ?? (EntProtoId?)job?.JobEntity;
        }

        if (previewEntity != null)
        {
            // Special type like borg or AI, do not spawn a human just spawn the entity.
            dummyEnt = EntityManager.SpawnEntity(previewEntity, MapCoordinates.Nullspace);
            return dummyEnt;
        }
        else if (humanoid is not null)
        {
            var dummy = _prototypeManager.Index<SpeciesPrototype>(humanoid.Species).DollPrototype;
            dummyEnt = EntityManager.SpawnEntity(dummy, MapCoordinates.Nullspace);
        }
        else
        {
            var dummy = _prototypeManager.Index<SpeciesPrototype>(DefaultSpecies).DollPrototype;
            dummyEnt = EntityManager.SpawnEntity(dummy, MapCoordinates.Nullspace);
        }

        _humanoid.LoadProfile(dummyEnt, humanoid);

        if (humanoid != null && jobClothes)
        {
            DebugTools.Assert(job != null);

            GiveDummyJobClothes(dummyEnt, job, humanoid);

            if (jobClothes)
                GiveDummyJobClothes(dummyEnt, job, humanoid);
            if (loadouts)
                GiveDummyLoadout(dummyEnt, job, humanoid);
        }

        return dummyEnt;
    }

    #endregion
}

