using Content.Shared.CombatMode;
using Content.Shared.Input;
using Robust.Shared.Input.Binding;
using Robust.Shared.Player;

namespace Content.Shared._ES.Interaction.HoldToFace;

public sealed class ESHoldToFaceSystem : EntitySystem
{
    [Dependency] private readonly SharedCombatModeSystem _combat = default!;

    public override void Initialize()
    {
        base.Initialize();

        CommandBinds.Builder
            .Bind(ContentKeyFunctions.ESHoldToFace,
                InputCmdHandler.FromDelegate(args => ToggleRotator(args, true), args => ToggleRotator(args, false), false, false))
            .Register<ESHoldToFaceSystem>();
    }

    private void ToggleRotator(ICommonSession? session, bool value)
    {
        if (session?.AttachedEntity is not { } ent || !HasComp<ESHoldToFaceComponent>(ent))
            return;

        // Don't try and override combat mode doing the same thing
        if (TryComp<CombatModeComponent>(ent, out var combat) && combat is { ToggleMouseRotator: true, IsInCombatMode: true })
            return;

        _combat.SetMouseRotatorComponents(ent, value);
    }
}
