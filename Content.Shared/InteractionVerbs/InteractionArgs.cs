using System.Diagnostics.CodeAnalysis;
using Content.Shared.Verbs;
using Robust.Shared.Serialization;

namespace Content.Shared.InteractionVerbs;

public sealed partial class InteractionArgs
{
    public EntityUid User, Target;
    public EntityUid? Used;
    public bool CanAccess, CanInteract;

    /// <summary>
    ///     A dictionary for actions and requirements to store data between different execution stages.
    ///     For instance, an action can cache some data in its CanPerform check and later use it in Perform.
    /// </summary>
    /// <remarks>
    ///     Non-action classes are highly not recommended to write anything to this dictionary - it can easily lead to errors.
    /// </remarks>
    public Dictionary<string, object> Blackboard => _blackboardField ??= new(3);
    private Dictionary<string, object>? _blackboardField; // null by default, allocated lazily (only if actually needed)

    public InteractionArgs(EntityUid user, EntityUid target, EntityUid? used, bool canAccess, bool canInteract)
    {
        User = user;
        Target = target;
        Used = used;
        CanAccess = canAccess;
        CanInteract = canInteract;
    }

    public static InteractionArgs From<T>(GetVerbsEvent<T> ev) where T : Verb => new(ev.User, ev.Target, ev.Using, ev.CanAccess, ev.CanInteract);

    /// <summary>
    ///     Tries to get a value from the blackboard as an instance of a specific type.
    /// </summary>
    public bool TryGetBlackboard<T>(string key, [NotNullWhen(true)] out T? value)
    {
        value = default;
        if (_blackboardField == null || !_blackboardField.TryGetValue(key, out var maybeValue))
            return false;

        // Cannot use a type check here. If someone fucks up, it's gonna be on them.
        value = (T?) maybeValue;
        return value != null;
    }
}
