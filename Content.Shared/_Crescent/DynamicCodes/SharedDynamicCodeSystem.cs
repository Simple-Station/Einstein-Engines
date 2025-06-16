using Robust.Shared.GameStates;
using System.Linq;
using Robust.Shared.Serialization;
using static Content.Shared.Disposal.Components.SharedDisposalUnitComponent;

namespace Content.Shared._Crescent.DynamicCodes;

/// <summary>
/// This handles...
/// </summary>
public class SharedDynamicCodeSystem : EntitySystem
{
    [Serializable, NetSerializable]
    public sealed class DynamicCodeHolderComponentState : ComponentState
    {
        public HashSet<int> codes = new();
        public Dictionary<string, HashSet<int>> mappedCodes = new();
    }

    // checks if the holder has ALL keys from the hashset. Returns false is not
    public bool hasAllKeys(HashSet<int> keys, DynamicCodeHolderComponent holder)
    {
        return holder.codes.Intersect(keys).Count() == keys.Count;
    }
    /// <summary>
    /// Checks if all the KEYS are present in the targetCodes list.
    /// </summary>
    /// <param name="keys"></param>
    /// <param name="targetCodes"></param>
    /// <returns></returns>
    public bool hasAllKeys(HashSet<int> keys, HashSet<int> targetCodes)
    {
        return targetCodes.Intersect(keys).Count() == keys.Count;
    }

    public bool hasAllKeys(HashSet<int> keys, EntityUid target)
    {
        if (!TryComp<DynamicCodeHolderComponent>(target, out var component))
            return false;
        return hasAllKeys(keys, component);
    }

    public static bool hasKey(int key, DynamicCodeHolderComponent component)
    {
        return component.codes.Contains(key);
    }

    public bool hasKey(HashSet<int> keys, DynamicCodeHolderComponent component)
    {
        foreach (var key in keys)
        {
            if (component.codes.Contains(key))
                return true;
        }

        return false;
    }

    public bool hasKey(int key, EntityUid owner)
    {
        if(!TryComp<DynamicCodeHolderComponent>(owner, out var codeHolder))
            return false;
        return hasKey(key, codeHolder);
    }

    public bool hasKey(HashSet<int> keys, EntityUid owner)
    {
        if (!TryComp<DynamicCodeHolderComponent>(owner, out var codeHolder))
            return false;
        return hasKey(keys, codeHolder);
    }
}
