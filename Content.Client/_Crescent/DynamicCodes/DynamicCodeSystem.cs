using Content.Shared._Crescent;
using Content.Shared._Crescent.DynamicCodes;
using Robust.Shared.GameStates;

namespace Content.Client._Crescent.DynamicCodes;

/// <summary>
/// This handles...
/// </summary>
public sealed class DynamicCodeSystem : SharedDynamicCodeSystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<DynamicCodeHolderComponent, ComponentHandleState>(OnHandleState);
    }

    private void OnHandleState(EntityUid uid, DynamicCodeHolderComponent component, ref ComponentHandleState args)
    {
        if (args.Current is not DynamicCodeHolderComponentState state)
            return;

        component.codes = state.codes;
        component.mappedCodes = state.mappedCodes;
    }
}
