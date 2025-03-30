using Robust.Shared.Prototypes;
using Robust.Shared.Localization;
using Content.Shared.Traits;
using System.Linq;
using System.Collections.Generic;

namespace Content.IntegrationTests.Tests.Traits;

/// <summary>
///    Checks if every trait has a valid desc and name localization string.
/// </summary>
[TestFixture]
[TestOf(typeof(TraitPrototype))]
public sealed class TraitLocalizationTest
{

    [Test]
    public async Task TestTraitLocalization()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;

        var locale = server.ResolveDependency<ILocalizationManager>();
        var proto = server.ResolveDependency<IPrototypeManager>();

        await server.WaitAssertion(() =>
        {
            var passed = true;
            var missingStrings = new List<string>();

            foreach (var traitProto in proto.EnumeratePrototypes<TraitPrototype>().OrderBy(a => a.ID))
            {
                var name = "trait-name-" + traitProto.ID;
                var desc = "trait-description-" + traitProto.ID;

                if (!locale.HasString(name))
                {
                    missingStrings.Add($"\"{traitProto.ID}\", \"{name}\"");
                    passed = false;
                }
                if (!locale.HasString(desc))
                {
                    missingStrings.Add($"\"{traitProto.ID}\", \"{desc}\"");
                    passed = false;
                }
            }

            Assert.That(passed, Is.True, $"The following traits are missing localization strings:\n  {string.Join("\n  ", missingStrings)}");
        });

        await pair.CleanReturnAsync();
    }
}

