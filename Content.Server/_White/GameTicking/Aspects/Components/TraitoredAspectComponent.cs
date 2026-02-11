using Content.Server.GameTicking.Rules.Components;

namespace Content.Server._White.GameTicking.Aspects.Components;

[RegisterComponent]
public sealed partial class TraitoredAspectComponent : Component
{
    [DataField]
    public TimeSpan AnnouncedForTraitorsVia = TimeSpan.FromSeconds(60);

    [DataField]
    public TimeSpan AnnouncedForAllViaMin = TimeSpan.FromSeconds(300);

    [DataField]
    public TimeSpan AnnouncedForAllViaMax = TimeSpan.FromSeconds(360);

    [DataField]
    public string AnnouncementForTraitorSound = "/Audio/_White/Aspects/palevo.ogg";

    [ViewVariables]
    public bool AnnouncedForTraitors;

    [ViewVariables]
    public TimeSpan AnnouncedForTraitorsAt;

    [ViewVariables]
    public TimeSpan AnnouncedForAllAt;

    [ViewVariables]
    public TraitorRuleComponent? TraitorRuleComponent;
}
