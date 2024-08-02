
using System.Linq;
using System.Text;
using Content.Shared.Clothing.Components;
using Content.Shared.Clothing.Loadouts.Prototypes;
using Content.Shared.Inventory;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.Customization.Systems;


public sealed class CharacterRequirementsSystem : EntitySystem
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    public bool CheckRequirementsValid(List<CharacterRequirement> requirements, JobPrototype job,
        HumanoidCharacterProfile profile, Dictionary<string, TimeSpan> playTimes, bool whitelisted,
        IEntityManager entityManager, IPrototypeManager prototypeManager, IConfigurationManager configManager,
        out List<FormattedMessage> reasons, LoadoutPrototype? loadout = null, EntityUid? dummy = null, List<SlotDefinition>? dummySlots = null)
    {
        reasons = new List<FormattedMessage>();
        var valid = true;

        foreach (var requirement in requirements)
        {
            // Set valid to false if the requirement is invalid and not inverted
            // If it's inverted set valid to false when it's valid
            if (!requirement.IsValid(job, profile, playTimes, whitelisted,
                entityManager, prototypeManager, configManager,
                out var reason))
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

        if (loadout is not null
            && dummy is not null
            && dummySlots is not null
            && CheckSpeciesCanEquip(dummy.Value, dummySlots, loadout, profile, out var species))
        {
            foreach (var speciesReason in species)
                reasons.Add(speciesReason);
            if (species.Count != 0)
                valid = false;
        }

        return valid;
    }


    /// <summary>
    ///     Gets the reason text from <see cref="CheckRequirementsValid"/> as a <see cref="FormattedMessage"/>.
    /// </summary>
    public FormattedMessage GetRequirementsText(List<FormattedMessage> reasons)
    {
        var text = new StringBuilder();
        foreach (var reason in reasons)
            text.Append($"\n{reason.ToMarkup()}");

        return FormattedMessage.FromMarkup(text.ToString().Trim());
    }

    /// <summary>
    ///     Gets the reason text from <see cref="CheckRequirementsValid"/> as a markup string.
    /// </summary>
    public string GetRequirementsMarkup(List<FormattedMessage> reasons)
    {
        var text = new StringBuilder();
        foreach (var reason in reasons)
            text.Append($"\n{reason.ToMarkup()}");

        return text.ToString().Trim();
    }

    public bool CheckSpeciesCanEquip(EntityUid dummy, List<SlotDefinition> dummySlots, LoadoutPrototype loadout, HumanoidCharacterProfile profile, out List<FormattedMessage> reasons)
    {
        reasons = new List<FormattedMessage>();
        if (!_entityManager.TryGetComponent<InventoryComponent>(dummy, out var inv))
            return false;

        foreach (var item in loadout.Items)
        {
            var toEquip = _entityManager.Spawn(item);
            if (!_entityManager.TryGetComponent<ClothingComponent>(toEquip, out var clothing))
            {
                QueueDel(toEquip);
                continue;
            }

            foreach (var slots in dummySlots)
                if (slots.SlotFlags == clothing.Slots)
                {
                    if (slots.Whitelist != null && !slots.Whitelist.IsValid(toEquip))
                        reasons.Add(FormattedMessage.FromMarkup(Loc.GetString("species-cannot-equip", ("species", profile.Species), ("item", item))));

                    if (slots.Blacklist != null && slots.Blacklist.IsValid(toEquip))
                        reasons.Add(FormattedMessage.FromMarkup(Loc.GetString("species-cannot-equip", ("species", profile.Species), ("item", item))));
                }

            var slotCount = inv.Slots.Count();
            foreach (var fullSlots in inv.Slots)
                if (fullSlots.SlotFlags == clothing.Slots)
                    --slotCount;
            if (slotCount != inv.Slots.Count())
                reasons.Add(FormattedMessage.FromMarkup(Loc.GetString("species-cannot-equip", ("species", profile.Species), ("item", item))));

            QueueDel(toEquip);
        }
        return true;
    }
}
