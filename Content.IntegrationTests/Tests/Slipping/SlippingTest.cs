#nullable enable
using System.Collections.Generic;
using Content.IntegrationTests.Tests.Interaction;
using Content.Shared.CCVar;
using Content.Shared.Movement.Components;
using Content.Shared.Slippery;
using Content.Shared.Stunnable;
using Robust.Shared.Configuration;
using Robust.Shared.GameObjects;
using Robust.Shared.Input;
using Robust.Shared.IoC;
using Robust.Shared.Maths;

namespace Content.IntegrationTests.Tests.Slipping;

public sealed class SlippingTest : MovementTest
{
    public sealed class SlipTestSystem : EntitySystem
    {
        [Dependency] public readonly IConfigurationManager Config = default!;
        public HashSet<EntityUid> Slipped = new();
        public override void Initialize()
        {
            SubscribeLocalEvent<SlipperyComponent, SlipEvent>(OnSlip);
        }

        private void OnSlip(EntityUid uid, SlipperyComponent component, ref SlipEvent args)
        {
            Slipped.Add(args.Slipped);
        }
    }

    [Test]
    public async Task BananaSlipTest()
    {
        var sys = SEntMan.System<SlipTestSystem>();
        var sprintWalks = sys.Config.GetCVar(CCVars.DefaultWalk);
        await SpawnTarget("TrashBananaPeel");

        // var modifier = Comp<MovementSpeedModifierComponent>(Player).SprintSpeedModifier;
        // Assert.That(modifier, Is.EqualTo(1), "Player is not moving at full speed."); // Yeeting this pointless Assert because it's not actually important.

        // Player is to the left of the banana peel and has not slipped.
#pragma warning disable NUnit2045
        Assert.That(Delta(), Is.GreaterThan(0.5f));
        Assert.That(sys.Slipped, Does.Not.Contain(SEntMan.GetEntity(Player)));
#pragma warning restore NUnit2045

        // Walking over the banana slowly does not trigger a slip.
        await SetKey(EngineKeyFunctions.Walk, sprintWalks ? BoundKeyState.Up : BoundKeyState.Down);
        await Move(DirectionFlag.East, 1f);
#pragma warning disable NUnit2045
        Assert.That(Delta(), Is.LessThan(0.5f));
        Assert.That(sys.Slipped, Does.Not.Contain(SEntMan.GetEntity(Player)));
#pragma warning restore NUnit2045
        AssertComp<KnockedDownComponent>(false, Player);

        // Moving at normal speeds does trigger a slip.
        await SetKey(EngineKeyFunctions.Walk, sprintWalks ? BoundKeyState.Down : BoundKeyState.Up);
        await Move(DirectionFlag.West, 1f);
        Assert.That(sys.Slipped, Does.Contain(SEntMan.GetEntity(Player)));
        AssertComp<KnockedDownComponent>(true, Player);
    }
}
