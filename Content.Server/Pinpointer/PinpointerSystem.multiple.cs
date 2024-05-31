using Content.Shared.Interaction;
using Content.Shared.Pinpointer;
using Robust.Shared.Utility;

namespace Content.Server.Pinpointer;

public sealed partial class PinpointerSystem
{
    public void InitializeMultiplePinpointer()
    {
        SubscribeLocalEvent<MultiplePinpointerComponent, ComponentStartup>(OnMultiplePinpointerStartup);
        SubscribeLocalEvent<MultiplePinpointerComponent, ActivateInWorldEvent>(OnMultiplePinpointerActivated);
        SubscribeLocalEvent<MultiplePinpointerComponent, AfterAutoHandleStateEvent>(OnMultiplePinpointerHandleState);
    }

    private void OnMultiplePinpointerHandleState(EntityUid uid, MultiplePinpointerComponent component, ref AfterAutoHandleStateEvent args)
    {
        SetMultiplePinpointer(uid, component);
    }

    private void OnMultiplePinpointerStartup(EntityUid uid, MultiplePinpointerComponent multiple, ComponentStartup args)
    {
        if (EntityManager.TryGetComponent(uid, out PinpointerComponent? tool))
            SetMultiplePinpointer(uid, multiple, tool);
        else
        {
            Log.Warning($"{MetaData(uid).EntityPrototype?.ID}({uid.Id}) does not have a Pinpointer component");
            DebugTools.Assert(false);
        }
    }

    private void OnMultiplePinpointerActivated(EntityUid uid, MultiplePinpointerComponent multiple, ActivateInWorldEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = CycleMultiplePinpointer(uid, multiple, args.User);
    }

    private bool CycleMultiplePinpointer(EntityUid uid, MultiplePinpointerComponent? multiple = null, EntityUid? user = null)
    {
        if (!Resolve(uid, ref multiple))
            return false;

        if (multiple.Modes.Length == 0)
            return false;

        multiple.CurrentEntry = (uint)((multiple.CurrentEntry + 1) % multiple.Modes.Length);
        SetMultiplePinpointer(uid, multiple, user: user);

        return true;
    }

    private void SetMultiplePinpointer(EntityUid uid,
        MultiplePinpointerComponent? multiple = null,
        PinpointerComponent? pin = null,
        EntityUid? user = null)
    {
        if (!Resolve(uid, ref multiple, ref pin))
            return;

        if (multiple.Modes.Length <= multiple.CurrentEntry)
            return;

        var current = multiple.Modes[multiple.CurrentEntry];
        SetDistance(uid, Distance.Unknown, pin);
        if (current == "Off")
        {
            SetActive(uid, true, pin);
            pin.Component = null;
        }
        else
        {
            SetActive(uid, false, pin);
            pin.Component = current;
        }
        LocateTarget(uid, pin);
    }
}
