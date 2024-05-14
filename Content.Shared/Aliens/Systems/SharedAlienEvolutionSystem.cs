using Content.Shared.Actions;

namespace Content.Shared.Aliens.Systems;

/// <summary>
/// This handles...
/// </summary>
public sealed class SharedAlienEvolutionSystem : EntitySystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {

    }
}

public sealed partial class AlienDroneEvolveActionEvent : InstantActionEvent { }

public sealed partial class AlienSentinelEvolveActionEvent : InstantActionEvent { }

public sealed partial class AlienPraetorianEvolveActionEvent : InstantActionEvent { }

public sealed partial class AlienHunterEvolveActionEvent : InstantActionEvent { }

public sealed partial class AlienQueenEvolveActionEvent : InstantActionEvent { }
