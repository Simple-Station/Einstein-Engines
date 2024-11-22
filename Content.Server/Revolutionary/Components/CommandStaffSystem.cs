using Content.Server.Psionics;

namespace Content.Server.Revolutionary.Components;
public sealed partial class CommandStaffSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CommandStaffComponent, OnRollPsionicsEvent>(OnRollPsionics);
    }

    private void OnRollPsionics(EntityUid uid, CommandStaffComponent component, ref OnRollPsionicsEvent args)
    {
        args.BaselineChance = args.BaselineChance * component.PsionicBonusModifier + component.PsionicBonusOffset;
    }
}