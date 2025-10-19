using Content.Shared._Crescent.ShipShields;
using Robust.Shared.Physics.Systems;
using Robust.Server.GameObjects;
using Robust.Server.GameStates;
using Content.Server.Power.Components;
using Content.Shared.Interaction;
using Content.Shared.Preferences;
using Content.Server.Preferences.Managers;
using Robust.Shared.Network;
using Content.Shared._Crescent.HullrotFaction;
using Robust.Shared.Player;
using Content.Server.Announcements.Systems;
using Content.Server.GameTicking;
using Content.Server.Popups;
using Content.Server.DoAfter;
using Content.Shared.Item.ItemToggle.Components;
using Robust.Shared.Serialization;
using Content.Shared.DoAfter;
using Content.Server._Crescent.SelfDeleteTimer;
using Robust.Shared.Timing;
using Robust.Shared.Map.Components;


namespace Content.Server._Crescent.SelfDeleteTimer;

public sealed class SelfDeleteSystem : EntitySystem
{
    [Dependency] private readonly EntityManager _entityManager = default!;
    [Dependency] private readonly IEntityManager _IentityManager = default!;


    public override void Initialize()
    {
        SubscribeLocalEvent<SelfDeleteComponent, ComponentInit>(OnInitEntity);
        SubscribeLocalEvent<SelfDeleteGridComponent, ComponentInit>(OnInitGrid);
    }

    private void OnInitEntity(EntityUid uid, SelfDeleteComponent component, ComponentInit args)
    {
        Timer.Spawn(component.TimeToDelete, () => DeleteEntity(uid));
    }

    private void OnInitGrid(EntityUid uid, SelfDeleteGridComponent component, ComponentInit args)
    {
        Timer.Spawn(component.TimeToDelete, () => DeleteGrid(uid));
    }

    private void DeleteEntity(EntityUid uid)
    {
        //_entityManager.QueueDeleteEntity(uid); this worked before but Ientity might work
        _IentityManager.DeleteEntity(uid);
    }

    private void DeleteGrid(EntityUid uid)
    {
        _IentityManager.DeleteEntity(uid);
    }

}
