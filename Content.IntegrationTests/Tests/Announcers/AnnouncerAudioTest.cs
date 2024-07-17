using System.Collections.Generic;
using Content.Server.Announcements.Systems;
using Content.Server.StationEvents;
using Content.Shared.Announcements.Prototypes;
using Robust.Client.ResourceManagement;
using Robust.Shared.GameObjects;
using Robust.Shared.Localization;

namespace Content.IntegrationTests.Tests.Announcers;

/// <summary>
///     Checks if every station event wanting the announcerSystem to send audios has a sound file
///     Sound collections are checked elsewhere
/// </summary>
[TestFixture]
[TestOf(typeof(AnnouncerPrototype))]
public sealed class AnnouncerAudioTest
{
    /// <inheritdoc cref="AnnouncerAudioTest" />
    [Test]
    public async Task TestEventSounds()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Connected = true });
        var server = pair.Server;
        var client = pair.Client;

        var entSysMan = server.ResolveDependency<IEntitySystemManager>();
        var cache = client.ResolveDependency<IResourceCache>();
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
                    var path = announcer.GetAnnouncementPath(announcementId, announcer.Announcer);

                    try
                    {
                        if (!cache.TryGetResource<AudioResource>(path, out _))
                        {
                            succeeded = false;
                            why.Add($"\"{announcementId}\": \"{path}\"");
                        }
                    }
                    catch (Exception)
                    {
                        // Working as intended
                    }
                }

                if (ev.Value.EndAnnouncement)
                {
                    var announcementId = announcer.GetAnnouncementId(ev.Key.ID, true);
                    var path = announcer.GetAnnouncementPath(announcementId, announcer.Announcer);

                    try
                    {
                        if (!cache.TryGetResource<AudioResource>(path, out _))
                        {
                            succeeded = false;
                            why.Add($"\"{announcementId}\": \"{path}\"");
                        }
                    }
                    catch (Exception)
                    {
                        // Working as intended
                    }
                }
            }

            Assert.That(succeeded, Is.True, $"The following announcements do not have a valid announcement audio:\n  {string.Join("\n  ", why)}");
        });

        await pair.CleanReturnAsync();
    }
}
