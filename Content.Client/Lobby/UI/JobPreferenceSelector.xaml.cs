using Content.Client.Players.PlayTimeTracking;
using Robust.Client.Player;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using System.Runtime.CompilerServices;
using Content.Shared.Roles;
using Content.Shared.Preferences;
using Content.Client.UserInterface.Controls;
using Content.Shared.Guidebook;
using Content.Client.Stylesheets;
using Content.Shared.StatusIcon;
using Robust.Shared.Prototypes;
using Content.Shared.Customization.Systems;
using Robust.Shared.Configuration;
using Content.Shared.GameTicking;

namespace Content.Client.Lobby.UI
{
    public sealed partial class JobPreferenceSelector : BoxContainer
    {
        private readonly JobRequirementsManager _requirements;
        private readonly IPlayerManager _playerManager;
        private readonly IEntityManager _entityManager;
        private readonly LobbyUIController _controller;
        private readonly IPrototypeManager _prototypeManager;
        //private readonly Dictionary<string, BoxContainer> _jobCategories;
        private readonly CharacterRequirementsSystem _characterRequirementsSystem;
        private readonly IConfigurationManager _configManager;
        private readonly HumanoidProfileEditor _profileEditor;

        private List<(string, RequirementsSelector)> _jobPriorities = new();

        public event Action<List<ProtoId<GuideEntryPrototype>>>? OnOpenGuidebook;

        public JobPreferenceSelector(
            JobRequirementsManager jobRequirements,
            IPlayerManager playerManager,
            IEntityManager entityManager,
            IPrototypeManager protoManager,
            IConfigurationManager cfgManager,
            HumanoidProfileEditor profileEditor
            )
        {
            RobustXamlLoader.Load(this);
            _requirements = jobRequirements;
            _playerManager = playerManager;
            _entityManager = entityManager;
            _prototypeManager = protoManager;
            _configManager = cfgManager;
            _profileEditor = profileEditor;
            _controller = UserInterfaceManager.GetUIController<LobbyUIController>();
            _characterRequirementsSystem = _characterRequirementsSystem = _entityManager.System<CharacterRequirementsSystem>();


            //    Jobs.Orphan();
            //    PreferenceUnavailableButton.AddItem(
            //        Loc.GetString(
            //            "humanoid-profile-editor-preference-unavailable-stay-in-lobby-button"),
            //            (int) PreferenceUnavailableMode.StayInLobby);
            //    PreferenceUnavailableButton.AddItem(
            //        Loc.GetString(
            //            "humanoid-profile-editor-preference-unavailable-spawn-as-overflow-button",
            //            ("overflowJob", Loc.GetString(SharedGameTicker.FallbackOverflowJobName))),
            //            (int) PreferenceUnavailableMode.SpawnAsOverflow);

            //    PreferenceUnavailableButton.OnItemSelected += args =>
            //    {
            //        PreferenceUnavailableButton.SelectId(args.Id);

            //        Profile = Profile?.WithPreferenceUnavailable((PreferenceUnavailableMode) args.Id);
            //        IsDirty = true;
            //    };

            //    _jobCategories = new Dictionary<string, BoxContainer>();
            //}



            //public void RefreshJobs()
            //{
            //    JobList.DisposeAllChildren();
            //    _jobCategories.Clear();
            //    _jobPriorities.Clear();

            //    // Get all displayed departments
            //    var departments = new List<DepartmentPrototype>();
            //    foreach (var department in _prototypeManager.EnumeratePrototypes<DepartmentPrototype>())
            //    {
            //        if (department.EditorHidden)
            //            continue;

            //        departments.Add(department);
            //    }

            //    departments.Sort(DepartmentUIComparer.Instance);

            //    var items = new[]
            //    {
            //        ("humanoid-profile-editor-job-priority-never-button", (int) JobPriority.Never),
            //        ("humanoid-profile-editor-job-priority-low-button", (int) JobPriority.Low),
            //        ("humanoid-profile-editor-job-priority-medium-button", (int) JobPriority.Medium),
            //        ("humanoid-profile-editor-job-priority-high-button", (int) JobPriority.High),
            //    };

            //    var firstCategory = true;
            //    foreach (var department in departments)
            //    {
            //        var departmentName = Loc.GetString($"department-{department.ID}");

            //        if (!_jobCategories.TryGetValue(department.ID, out var category))
            //        {
            //            category = new AlternatingBGContainer
            //            {
            //                Orientation = LayoutOrientation.Vertical,
            //                Name = department.ID,
            //                ToolTip = Loc.GetString("humanoid-profile-editor-jobs-amount-in-department-tooltip",
            //                    ("departmentName", departmentName)),
            //                Margin = new(0, firstCategory ? 0 : 20, 0, 0),
            //                Children =
            //                {
            //                    new Label
            //                    {
            //                        Text = Loc.GetString("humanoid-profile-editor-department-jobs-label",
            //                            ("departmentName", departmentName)),
            //                        StyleClasses = { StyleBase.StyleClassLabelHeading, },
            //                        Margin = new(5f, 0, 0, 0),
            //                    },
            //                },
            //            };

            //            firstCategory = false;
            //            _jobCategories[department.ID] = category;
            //            JobList.AddChild(category);
            //        }

            //        var jobs = department.Roles.Select(jobId => _prototypeManager.Index<JobPrototype>(jobId))
            //            .Where(job => job.SetPreference)
            //            .ToArray();

            //        Array.Sort(jobs, JobUIComparer.Instance);

            //        foreach (var job in jobs)
            //        {
            //            var jobContainer = new BoxContainer { Orientation = LayoutOrientation.Horizontal, };
            //            var selector = new RequirementsSelector { Margin = new(3f, 3f, 3f, 0f) };
            //            selector.OnOpenGuidebook += OnOpenGuidebook;

            //            var icon = new TextureRect
            //            {
            //                TextureScale = new(2, 2),
            //                VerticalAlignment = VAlignment.Center
            //            };
            //            var jobIcon = _prototypeManager.Index<JobIconPrototype>(job.Icon);
            //            icon.Texture = jobIcon.Icon.Frame0();
            //            selector.Setup(items, job.LocalizedName, 200, job.LocalizedDescription, icon, job.Guides);

            //            if (!_requirements.CheckJobWhitelist(job, out var reason))
            //                selector.LockRequirements(reason);
            //            else if (!_characterRequirementsSystem.CheckRequirementsValid(
            //                 job.Requirements ?? new(),
            //                 job,
            //                 _profileEditor.Profile ?? HumanoidCharacterProfile.DefaultWithSpecies(),
            //                 _requirements.GetRawPlayTimeTrackers(),
            //                 _requirements.IsWhitelisted(),
            //                 job,
            //                 _entityManager,
            //                 _prototypeManager,
            //                 _configManager,
            //                 out var reasons))
            //                selector.LockRequirements(_characterRequirementsSystem.GetRequirementsText(reasons));
            //            else
            //                selector.UnlockRequirements();

            //            selector.OnSelected += selectedPrio =>
            //            {
            //                var selectedJobPrio = (JobPriority) selectedPrio;
            //                _profileEditor.Profile = _profileEditor.Profile?.WithJobPriority(job.ID, selectedJobPrio);

            //                foreach (var (jobId, other) in _jobPriorities)
            //                {
            //                    // Sync other selectors with the same job in case of multiple department jobs
            //                    if (jobId == job.ID)
            //                        other.Select(selectedPrio);
            //                    else if (selectedJobPrio == JobPriority.High &&
            //                             (JobPriority) other.Selected == JobPriority.High)
            //                    {
            //                        // Lower any other high priorities to medium.
            //                        other.Select((int) JobPriority.Medium);
            //                        _profileEditor.Profile = _profileEditor.Profile?.WithJobPriority(jobId, JobPriority.Medium);
            //                    }
            //                }

            //                // TODO: Only reload on high change (either to or from).
            //                UpdateJobPriorities();
            //            };

            //            _jobPriorities.Add((job.ID, selector));
            //            jobContainer.AddChild(selector);
            //            category.AddChild(jobContainer);
            //        }

            //    }

            //    UpdateJobPriorities();

            //}

            ///// Updates selected job priorities to the profile's
            //private void UpdateJobPriorities()
            //{
            //    foreach (var (jobId, prioritySelector) in _jobPriorities)
            //    {
            //        var priority = _profileEditor.Profile?.JobPriorities.GetValueOrDefault(jobId, JobPriority.Never) ?? JobPriority.Never;
            //        prioritySelector.Select((int) priority);
            //    }
        }
    }
}
