using System.Collections.Generic;
using Content.Server.Announcements.Systems;
using Content.Server.StationEvents;
using Content.Shared.Announcements.Prototypes;
using Robust.Shared.GameObjects;
using Robust.Shared.Localization;

namespace Content.IntegrationTests.Tests.Announcers;

[TestFixture]
[TestOf(typeof(AnnouncerPrototype))]
public sealed class AnnouncerLocalizationTest
{
    [Test]
    public async Task TestEventLocalization()
    {
        // Checks if every station event wanting the announcerSystem to send messages has a localization string
        // If an event doesn't have startAnnouncement or endAnnouncement set to true
        //  it will be expected for that system to handle the announcements if it wants them

        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;

        var locale = server.ResolveDependency<ILocalizationManager>();
        var entSysMan = server.ResolveDependency<IEntitySystemManager>();
        var announcer = entSysMan.GetEntitySystem<AnnouncerSystem>();
        var events = entSysMan.GetEntitySystem<EventManagerSystem>();

        await server.WaitAssertion(() =>
        {
            var succeeded = true;
            var why = new List<string>();

            foreach (var ev in events.AllEvents())
            {
                if (ev.Value.StartAnnouncement)
                {
                    var announcementId = announcer.GetAnnouncementId(ev.Key.ID);
                    var eventLocaleString = announcer.GetEventLocaleString(announcementId);

                    if (locale.GetString(eventLocaleString) == eventLocaleString)
                    {
                        succeeded = false;
                        why.Add($"\"{announcementId}\": \"{eventLocaleString}\"");
                    }
                }

                if (ev.Value.EndAnnouncement)
                {
                    var announcementId = announcer.GetAnnouncementId(ev.Key.ID, true);
                    var eventLocaleString = announcer.GetEventLocaleString(announcementId);

                    if (locale.GetString(eventLocaleString) == eventLocaleString)
                    {
                        succeeded = false;
                        why.Add($"\"{announcementId}\": \"{eventLocaleString}\"");
                    }
                }
            }

            Assert.That(succeeded, Is.True, $"The following announcements do not have a localization string:\n  {string.Join("\n  ", why)}");
        });

        await pair.CleanReturnAsync();
    }
}
