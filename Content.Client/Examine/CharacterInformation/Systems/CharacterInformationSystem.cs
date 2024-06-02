using System.Linq;
using Content.Client.Examine;
using Content.Client.Inventory;
using Content.Shared.Examine.CharacterInformation.Components;
using Content.Client.Examine.CharacterInformation.UI;
using Content.Shared.Access.Components;
using Content.Shared.CCVar;
using Content.Shared.DetailExaminable;
using Content.Shared.IdentityManagement.Components;
using Content.Shared.PDA;
using Content.Shared.Roles;
using Content.Shared.Verbs;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Client.Examine.CharacterInformation.Systems;

public sealed class CharacterInformationSystem : EntitySystem
{
    [Dependency] private readonly ExamineSystem _examine = default!;
    [Dependency] private readonly ClientInventorySystem _inventory = default!;
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;

    private CharacterInformationWindow? _window;


    public override void Initialize()
    {
        base.Initialize();

        _window = new CharacterInformationWindow();

        SubscribeLocalEvent<CharacterInformationComponent, GetVerbsEvent<ExamineVerb>>(OnGetExamineVerbs);
    }


    private void OnGetExamineVerbs(EntityUid uid, CharacterInformationComponent component, GetVerbsEvent<ExamineVerb> args)
    {
        var verb = new ExamineVerb
        {
            Act = () =>
            {
                ShowInfoWindow(args.Target);
            },
            Text = Loc.GetString("character-information-verb-text"),
            Message = Loc.GetString("character-information-verb-message"),
            Category = VerbCategory.Examine,
            Disabled = !_examine.IsInDetailsRange(args.User, uid),
            Icon = new SpriteSpecifier.Texture(new ("/Textures/Interface/VerbIcons/information.svg.192dpi.png")),
            ClientExclusive = true,
        };

        args.Verbs.Add(verb);
    }


    private void ShowInfoWindow(EntityUid uid)
    {
        if (_window == null)
            return;

        string? name = null;
        string? job = null;
        string? flavorText = null;

        // Get ID from inventory, get name and job from ID
        var info = GetNameAndJob(uid);

        name = info.Item1;
        job = info.Item2;

        // Fancy job title
        if (!string.IsNullOrEmpty(job))
        {
            var test = job.Replace(" ", "");
            // Command will be last in the list
            // TODO: Make this not revolve around this fact ^
            var departments = _prototype.EnumeratePrototypes<DepartmentPrototype>().OrderBy(d => d.ID).Reverse();
            var department = departments.FirstOrDefault(d => d.Roles.Contains(test));

            if (department is not null)
            {
                // Department (ex: Command or Security)
                var dept = string.Join(" ", Loc.GetString($"department-{department.ID}").Split(' ').Select(s => s[0].ToString().ToUpper() + s[1..].ToLower()));
                // Redo the job title with the department color and department (ex: Captain (Command) or Security Officer (Security))
                job = $"[color={department.Color.ToHex()}]{job} ({dept})[/color]";
            }
        }

        // Get and set flavor text
        if (_config.GetCVar(CCVars.FlavorText) &&
            _entity.TryGetComponent<DetailExaminableComponent>(uid, out var detail))
            flavorText = detail.Content;

        _window.UpdateUi(uid, name, job, flavorText);
        _window.Open();
    }

    /// <summary>
    ///     Gets the ID card component from either a PDA or an ID card if the entity has one
    /// </summary>
    /// <param name="idUid">Entity to check</param>
    /// <returns>ID card component if they have one on the entity</returns>
    /// <remarks>This function should not exist</remarks> // TODO Remove this function
    private IdCardComponent? GetId(EntityUid? idUid)
    {
        // PDA
        if (_entity.TryGetComponent(idUid, out PdaComponent? pda) && pda.ContainedId is not null)
            return _entity.GetComponent<IdCardComponent>(pda.ContainedId.Value);
        // ID Card
        if (_entity.TryGetComponent(idUid, out IdCardComponent? id))
            return id;

        return null;
    }

    /// <summary>
    ///     Gets the name and job title from an ID card component
    /// </summary>
    /// <param name="uid">The entity to attempt information retrieval from</param>
    /// <returns>Name, Job Title</returns>
    /// <remarks>This function should not exist</remarks>
    private (string, string) GetNameAndJob(EntityUid uid)
    {
        string? name = null;

        if (_entity.TryGetComponent<IdentityComponent>(uid, out var identity) &&
            identity.IdentityEntitySlot.ContainedEntity != null)
            name = _entity.GetComponent<MetaDataComponent>(identity.IdentityEntitySlot.ContainedEntity.Value).EntityName;

        if (_inventory.TryGetSlotEntity(uid, "id", out var idUid))
        {
            var id = GetId(idUid);
            if (id is not null)
            {
                name ??= id.FullName;
                if (string.IsNullOrEmpty(name))
                    name = "Unknown";

                var jobTitle = id.JobTitle;
                if (string.IsNullOrEmpty(jobTitle))
                    jobTitle = "Unknown";
                jobTitle = string.Join(" ", jobTitle.Split(' ').Select(s => s[0].ToString().ToUpper() + s[1..].ToLower()));

                return (name, jobTitle);
            }
        }

        return (name ?? "Unknown", "Unknown");
    }
}
