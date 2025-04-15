using System.Diagnostics.CodeAnalysis;
using System.Xml;
using Content.Shared.Access.Components;
using Content.Shared.Inventory;
using Content.Shared.PDA;
using Content.Shared.Shuttles.BUIStates;

namespace Content.Shared._Crescent.Helpers;

/// <summary>
/// This handles... helpers!
/// </summary>
public sealed class CrescentHelperSystem : EntitySystem
{

    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    // Used for getting ID off any entity
    public bool GetPlayerId(EntityUid uid,[NotNullWhen(true)] out IdCardComponent? idCardUid)
    {
        if (_inventorySystem.TryGetSlotEntity(uid, "id", out var idUid))
        {
            // PDA
            if (EntityManager.TryGetComponent(idUid, out PdaComponent? pda) &&
                TryComp<IdCardComponent>(pda.ContainedId, out var id))
            {
                idCardUid = id;
                return true;
            }
            // ID Card
            if (EntityManager.TryGetComponent(idUid, out id))
            {
                idCardUid = id;
                return true;
            }
        }

        idCardUid = null;
        return false;
    }

    public bool GetPlayerIdEntity(EntityUid uid, [NotNullWhen(true)] out EntityUid? cardEntityUid)
    {
        if (_inventorySystem.TryGetSlotEntity(uid, "id", out var idUid))
        {
            // PDA
            if (EntityManager.TryGetComponent(idUid, out PdaComponent? pda) &&
                TryComp<IdCardComponent>(pda.ContainedId, out var id))
            {
                cardEntityUid = pda.ContainedId;
                return true;
            }
            // ID Card
            if (EntityManager.TryGetComponent(idUid, out id))
            {
                cardEntityUid = idUid;
                return true;
            }
        }

        cardEntityUid = null;
        return false;
    }




    public bool getGridOfEntity(EntityUid target, [NotNullWhen(true)]out EntityUid? gridId)
    {
        var internalId = Transform(target).GridUid;
        gridId = internalId;
        if (gridId is not null)
            return true;
        return false;

    }

}
