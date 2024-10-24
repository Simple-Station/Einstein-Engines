using System.Numerics;
using Content.Shared.Camera;
using Content.Shared.Hands;
using Content.Shared.Hands.Components;
using Content.Shared.Item;
using Robust.Shared.Serialization;

namespace Content.Shared.Telescope;

public abstract class SharedTelescopeSystem : EntitySystem
{
    [Dependency] private readonly SharedEyeSystem _eye = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeAllEvent<EyeOffsetChangedEvent>(OnEyeOffsetChanged);
        SubscribeLocalEvent<TelescopeComponent, GotUnequippedHandEvent>(OnUnequip);
        SubscribeLocalEvent<TelescopeComponent, HandDeselectedEvent>(OnHandDeselected);
        SubscribeLocalEvent<TelescopeComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnShutdown(Entity<TelescopeComponent> ent, ref ComponentShutdown args)
    {
        if (!TryComp(ent.Comp.LastEntity, out EyeComponent? eye)
            || ent.Comp.LastEntity == ent && TerminatingOrDeleted(ent))
            return;

        SetOffset((ent.Comp.LastEntity.Value, eye), Vector2.Zero, ent);
    }

    private void OnHandDeselected(Entity<TelescopeComponent> ent, ref HandDeselectedEvent args)
    {
        if (!TryComp(args.User, out EyeComponent? eye))
            return;

        SetOffset((args.User, eye), Vector2.Zero, ent);
    }

    private void OnUnequip(Entity<TelescopeComponent> ent, ref GotUnequippedHandEvent args)
    {
        if (!TryComp(args.User, out EyeComponent? eye)
            || !HasComp<ItemComponent>(ent.Owner))
            return;

        SetOffset((args.User, eye), Vector2.Zero, ent);
    }

    public TelescopeComponent? GetRightTelescope(EntityUid? ent)
    {
        TelescopeComponent? telescope = null;

        if (TryComp<HandsComponent>(ent, out var hands)
            && hands.ActiveHandEntity.HasValue
            && TryComp<TelescopeComponent>(hands.ActiveHandEntity, out var handTelescope))
            telescope = handTelescope;
        else if (TryComp<TelescopeComponent>(ent, out var entityTelescope))
            telescope = entityTelescope;

        return telescope;
    }

    private void OnEyeOffsetChanged(EyeOffsetChangedEvent msg, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is not { } ent
            || !TryComp<EyeComponent>(ent, out var eye))
            return;

        var telescope = GetRightTelescope(ent);

        if (telescope == null)
            return;

        var offset = Vector2.Lerp(eye.Offset, msg.Offset, telescope.LerpAmount);

        SetOffset((ent, eye), offset, telescope);
    }

    private void SetOffset(Entity<EyeComponent> ent, Vector2 offset, TelescopeComponent telescope)
    {
        telescope.LastEntity = ent;

        if (TryComp(ent, out CameraRecoilComponent? recoil))
        {
            recoil.BaseOffset = offset;
            _eye.SetOffset(ent, offset + recoil.CurrentKick, ent);
        }
        else
        {
            _eye.SetOffset(ent, offset, ent);
        }
    }

    public void SetParameters(Entity<TelescopeComponent> ent, float? divisor = null, float? lerpAmount = null)
    {
        var telescope = ent.Comp;

        telescope.Divisor = divisor ?? telescope.Divisor;
        telescope.LerpAmount = lerpAmount ?? telescope.LerpAmount;

        Dirty(ent.Owner, telescope);
    }
}

[Serializable, NetSerializable]
public sealed class EyeOffsetChangedEvent : EntityEventArgs
{
    public Vector2 Offset;
}
