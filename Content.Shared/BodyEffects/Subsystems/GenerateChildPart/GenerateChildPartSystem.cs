using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Body.Events;
using Robust.Shared.Map;
using Robust.Shared.Timing;
using Robust.Shared.Network;
using System.Numerics;

namespace Content.Shared.BodyEffects.Subsystems;

public sealed class GenerateChildPartSystem : EntitySystem
{
    [Dependency] private readonly SharedBodySystem _bodySystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GenerateChildPartComponent, BodyPartComponentsModifyEvent>(OnPartComponentsModify);
    }

    private void OnPartComponentsModify(EntityUid uid, GenerateChildPartComponent component, ref BodyPartComponentsModifyEvent args)
    {
        if (args.Add)
            CreatePart(uid, component);
        //else
            //DeletePart(uid, component);
    }

    private void CreatePart(EntityUid uid, GenerateChildPartComponent component)
    {
        if (!TryComp(uid, out BodyPartComponent? partComp)
            || partComp.Body is null)
            return;

        if (_net.IsServer)
        {
            var childPart = Spawn(component.Id, new EntityCoordinates(partComp.Body.Value, Vector2.Zero));

            if (!TryComp(childPart, out BodyPartComponent? childPartComp))
                return;

            var slotName = _bodySystem.GetSlotFromBodyPart(childPartComp);
            _bodySystem.TryCreatePartSlot(uid, slotName, childPartComp.PartType, out var _);
            _bodySystem.AttachPart(uid, slotName, childPart, partComp, childPartComp);
            component.ChildPart = childPart;
            Dirty(childPart, childPartComp);
        }

        _bodySystem.ChangeSlotState((uid, partComp), false);
    }

    private void DeletePart(EntityUid uid, GenerateChildPartComponent component)
    {
        if (!TryComp(uid, out BodyPartComponent? partComp))
            return;

        _bodySystem.ChangeSlotState((uid, partComp), true);
        var ev = new BodyPartDroppedEvent((uid, partComp));
        RaiseLocalEvent(uid, ref ev);
        QueueDel(uid);
    }
}

