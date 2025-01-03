using Robust.Shared.Prototypes;
using Robust.Shared.Localization;
using Content.Shared.Humanoid.Markings;
using System.Linq;
using System.Collections.Generic;

namespace Content.IntegrationTests.Tests.Traits;

/// <summary>
///    Checks if every marking has a valid name localization string.
/// </summary>
[TestFixture]
[TestOf(typeof(MarkingPrototype))]
public sealed class MarkingLocalizationTest
{
    [Test]
    public async Task TestMarkingLocalization()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;

        var locale = server.ResolveDependency<ILocalizationManager>();
        var proto = server.ResolveDependency<IPrototypeManager>();

        await server.WaitAssertion(() =>
        {
            var missingStrings = new List<string>();

            foreach (var markingProto in proto.EnumeratePrototypes<MarkingPrototype>().OrderBy(a => a.ID))
                if (!locale.HasString($"marking-{markingProto.ID}") && string.IsNullOrEmpty(markingProto.Name))
                    missingStrings.Add($"\"{markingProto.ID}\", \"marking-{markingProto.ID}\"");

            Assert.That(!missingStrings.Any(), Is.True, $"The following markings are missing localization strings:\n  {string.Join("\n  ", missingStrings)}");
        });

        await pair.CleanReturnAsync();
    }
}
