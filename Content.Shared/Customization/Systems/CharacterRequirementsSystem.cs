using System.Linq;
using System.Text;
using Content.Shared.Inventory;
using Content.Shared.Mind;
using Content.Shared.Players.PlayTimeTracking;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Content.Shared.Roles.Jobs;
using Content.Shared.Station;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.Customization.Systems;


public sealed class CharacterRequirementsSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedJobSystem _jobSystem = default!;
    [Dependency] private readonly SharedMindSystem _mindSystem = default!;
    [Dependency] private readonly SharedStationSpawningSystem _stationSpawningSystem = default!;

    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IPrototypeManager _protomanager = default!;
    [Dependency] private readonly IConfigurationManager _configurationManager = default!;
    [Dependency] private readonly ISharedPlaytimeManager _playtimeManager = default!;

    public bool CheckRequirementValid(CharacterRequirement requirement, JobPrototype job,
        HumanoidCharacterProfile profile, Dictionary<string, TimeSpan> playTimes, bool whitelisted, IPrototype prototype,
        IEntityManager entityManager, IPrototypeManager prototypeManager, IConfigurationManager configManager,
        out string? reason, int depth = 0)
    {
        // Return false if the requirement is invalid and not inverted
        // If it's inverted return false when it's valid
        return
            !requirement.IsValid(job, profile, playTimes, whitelisted, prototype,
                entityManager, prototypeManager, configManager,
                out reason, depth)
                ? requirement.Inverted
                : !requirement.Inverted;
    }

    /// <summary>
    ///     Checks if a character entity meets the specified requirements.
    /// </summary>
    /// <param name="requirements">The list of requirements to validate.</param>
    /// <param name="characterUid">The entity ID of the character to check.</param>
    /// <param name="prototype">The prototype associated with the requirements.</param>
    /// <param name="reasons">Output list of reasons why requirements weren't met.</param>
    /// <param name="depth">Current recursion depth for nested requirements.</param>
    /// <param name="whitelisted">Whether the character is whitelisted.</param>
    /// <returns>True if all requirements are met, false otherwise.</returns>
    public bool CheckRequirementsValid(List<CharacterRequirement> requirements, EntityUid characterUid, IPrototype prototype, out List<string> reasons, int depth = 0, bool whitelisted = false)
    {
        reasons = new List<string>();

        if (!_mindSystem.TryGetMind(characterUid, out var mindId, out var mind)
            || mind.Session == null
            || !_jobSystem.MindTryGetJob(mindId, out var jobPrototype)
            || !_stationSpawningSystem.GetProfile(characterUid, out var stationSpawningProfile)
            || !_playtimeManager.TryGetTrackerTimes(mind.Session, out var trackerTimes))
            return false;

        return CheckRequirementsValid(requirements, jobPrototype, stationSpawningProfile, trackerTimes, whitelisted, prototype, _entManager, _protomanager, _configurationManager, out reasons, depth, mind);
    }

    public bool CheckRequirementsValid(List<CharacterRequirement> requirements, JobPrototype job,
        HumanoidCharacterProfile profile, Dictionary<string, TimeSpan> playTimes, bool whitelisted, IPrototype prototype,
        IEntityManager entityManager, IPrototypeManager prototypeManager, IConfigurationManager configManager,
        out List<string> reasons, int depth = 0, MindComponent? mind = null)
    {
        reasons = new List<string>();
        var valid = true;

        foreach (var requirement in requirements)
        {
            // Set valid to false if the requirement is invalid and not inverted
            // If it's inverted set valid to false when it's valid
            if (!requirement.IsValid(job, profile, playTimes, whitelisted, prototype,
                entityManager, prototypeManager, configManager,
                out var reason, depth, mind))
            {
                if (valid)
                    valid = requirement.Inverted;
            }
            else
            {
                if (valid)
                    valid = !requirement.Inverted;
            }

            if (reason != null)
                reasons.Add(reason);
        }

        return valid;
    }


    /// <summary>
    ///     Gets the reason text from <see cref="CheckRequirementsValid"/> as a <see cref="FormattedMessage"/>.
    /// </summary>
    public FormattedMessage GetRequirementsText(List<string> reasons)
    {
        return FormattedMessage.FromMarkup(GetRequirementsMarkup(reasons));
    }

    /// <summary>
    ///     Gets the reason text from <see cref="CheckRequirementsValid"/> as a markup string.
    /// </summary>
    public string GetRequirementsMarkup(List<string> reasons)
    {
        var text = new StringBuilder();
        foreach (var reason in reasons)
            text.Append($"\n{reason}");

        return text.ToString().Trim();
    }


    /// <summary>
    ///     Returns true if the given dummy can equip the given item.
    ///     Does not care if items are already in equippable slots, and ignores pockets.
    /// </summary>
    public bool CanEntityWearItem(EntityUid dummy, EntityUid clothing, bool bypassAccessCheck = false)
    {
        return _inventory.TryGetSlots(dummy, out var slots)
            && slots.Where(slot => !slot.SlotFlags.HasFlag(SlotFlags.POCKET))
                .Any(slot => _inventory.CanEquip(dummy, clothing, slot.Name, out _, onSpawn: true, bypassAccessCheck: bypassAccessCheck));
    }
}
