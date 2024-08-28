using Content.Shared.Humanoid;

namespace Content.Server.Chat
{
    public sealed partial class TelepathicChatSystem
    {
        private void InitializePsychognomy()
        {
            SubscribeLocalEvent<HumanoidAppearanceComponent, GetPsychognomicDescriptorEvent>(DescribeHumanoid);
        }

        private void DescribeHumanoid(EntityUid uid, HumanoidAppearanceComponent component, GetPsychognomicDescriptorEvent ev)
        {
            if (component.Sex != Sex.Unsexed)
                ev.Descriptors.Add(component.Sex == Sex.Male ? Loc.GetString("p-descriptor-male") : Loc.GetString("p-descriptor-female"));
        }

    }
    public sealed class GetPsychognomicDescriptorEvent : EntityEventArgs
    {
        public List<String> Descriptors = new List<String>();
    }
}