using System.Linq;
using System.Text;
using Content.Shared.Inventory;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.Customization.Systems;


public sealed class CharacterRequirementsSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;


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

    public bool CheckRequirementsValid(List<CharacterRequirement> requirements, JobPrototype job,
        HumanoidCharacterProfile profile, Dictionary<string, TimeSpan> playTimes, bool whitelisted, IPrototype prototype,
        IEntityManager entityManager, IPrototypeManager prototypeManager, IConfigurationManager configManager,
        out List<string> reasons, int depth = 0)
    {
        reasons = new List<string>();
        var valid = true;

        foreach (var requirement in requirements)
        {
            // Set valid to false if the requirement is invalid and not inverted
            // If it's inverted set valid to false when it's valid
            if (!requirement.IsValid(job, profile, playTimes, whitelisted, prototype,
                entityManager, prototypeManager, configManager,
                out var reason, depth))
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
