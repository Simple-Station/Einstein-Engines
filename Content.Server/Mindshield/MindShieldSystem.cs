using Content.Server.Administration.Logs;
using Content.Server.Mind;
using Content.Server.Popups;
using Content.Server.Roles;
using Content.Shared.Database;
using Content.Shared.Implants;
using Content.Shared.Implants.Components;
using Content.Shared.Mindshield.Components;
using Content.Shared.Revolutionary.Components;
using Content.Shared.Tag;

namespace Content.Server.Mindshield;

/// <summary>
/// System used for checking if the implanted is a Rev or Head Rev.
/// </summary>
public sealed class MindShieldSystem : EntitySystem
{
    [Dependency] private readonly IAdminLogManager _adminLogManager = default!;
    [Dependency] private readonly RoleSystem _roleSystem = default!;
    [Dependency] private readonly MindSystem _mindSystem = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;

    [ValidatePrototypeId<TagPrototype>]
    public const string MindShieldTag = "MindShield";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SubdermalImplantComponent, ImplantImplantedEvent>(ImplantCheck);
    }

    /// <summary>
    /// Checks if the implant was a mindshield or not
    /// </summary>
    public void ImplantCheck(EntityUid uid, SubdermalImplantComponent comp, ref ImplantImplantedEvent ev)
    {
        if (_tag.HasTag(ev.Implant, MindShieldTag) && ev.Implanted != null)
        {
            EnsureComp<MindShieldComponent>(ev.Implanted.Value);
            MindShieldRemovalCheck(ev.Implanted.Value, ev.Implant);
        }
    }

    /// <summary>
    /// Checks if the implanted person was a Rev or Head Rev and remove role or destroy mindshield respectively.
    /// </summary>
    public void MindShieldRemovalCheck(EntityUid implanted, EntityUid implant)
    {
        if (HasComp<HeadRevolutionaryComponent>(implanted))
        {
            _popupSystem.PopupEntity(Loc.GetString("head-rev-break-mindshield"), implanted);
            //_revolutionarySystem.ToggleConvertAbility((implanted, headRevComp), false); // GoobStation - turn off headrev ability to convert //Commented out due to missing components at compile time -Ithral
            //QueueDel(implant); - Goobstation - Headrevs should remove implant before turning on ability
            return;
        }

        if (_mindSystem.TryGetMind(implanted, out var mindId, out _) &&
            _roleSystem.MindTryRemoveRole<RevolutionaryRoleComponent>(mindId))
        {
            _adminLogManager.Add(LogType.Mind, LogImpact.Medium, $"{ToPrettyString(implanted)} was deconverted due to being implanted with a Mindshield.");
        }
       /* if (HasComp<MindcontrolledComponent>(implanted))   //Goobstation - Mindcontrol Implant
            RemComp<MindcontrolledComponent>(implanted); */  //TODO: Fix rev system
    }

    // GoobStation
    /// <summary>
    /// Removes mindshield comp if mindshield implant was ejected
    /// </summary>
  /*  public void OnMindShieldRemoved(Entity<MindShieldComponent> mindshielded, ref ImplantRemovedFromEvent args)
    {
        if (!_tag.HasTag(args.Implant, MindShieldTag))
            return;

        _popupSystem.PopupEntity(Loc.GetString("mindshield-implant-effect-removed"), mindshielded, mindshielded);

        if (TryComp<HeadRevolutionaryComponent>(mindshielded, out var headRevComp))
            _revolutionarySystem.ToggleConvertAbility((mindshielded, headRevComp), true);

        RemComp<MindShieldComponent>(mindshielded);
    }*/ //TODO: fix rev system
}
