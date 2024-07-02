using System.Collections.Generic;
using System.Linq;
using Content.Shared.Announcements.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests.Announcers;

[TestFixture]
[TestOf(typeof(AnnouncerPrototype))]
public sealed class AnnouncerPrototypeTests
{
    [Test]
    public async Task TestAnnouncerFallbacks()
    {
        // Checks if every announcer has a fallback announcement

        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;

        var prototype = server.ResolveDependency<IPrototypeManager>();

        await server.WaitAssertion(() =>
        {
            var success = true;
            var why = new List<string>();

            foreach (var announcer in prototype.EnumeratePrototypes<AnnouncerPrototype>())
            {
                if (announcer.Announcements.All(a => a.ID.ToLower() != "fallback"))
                {
                    success = false;
                    why.Add(announcer.ID);
                }
            }

            Assert.That(success, Is.True, $"The following announcers do not have a fallback announcement:\n  {string.Join("\n  ", why)}");
        });

        await pair.CleanReturnAsync();
    }
}
