using Content.Shared.Roles;
using Content.Shared.Inventory;
using JetBrains.Annotations;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
using Content.Server.Station.Systems;
using Content.Server.Power.Components;
using Content.Server.Factory.Components;
using Content.Shared.Physics;
using Robust.Shared.Physics.Collision.Shapes;
using Robust.Shared.Physics.Components;
using Content.Server.DeviceLinking.Events;
using Content.Server.DeviceLinking.Systems;
using System.Linq;
using Content.Server.Stack;
using Content.Shared.Stacks;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.GameObjects;
using Robust.Server.GameObjects;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Dynamics;
using Content.Shared.Item;
using Content.Server.Item;
using Robust.Shared.Containers;
using Content.Server.Sound;
using Content.Shared.Sound;
using Robust.Server.Audio;
using System.Collections.Generic;
using Content.Server.Access.Systems;
using Content.Server.Chat.Systems;
using Content.Server.Verbs;
using Content.Shared.Examine;
using Content.Shared.Factory.Components;
using Content.Shared.Verbs;
using Content.Shared.Popups;
using Robust.Shared.Utility;
using Content.Shared.Interaction;
using Content.Shared.Tools.Components;
using static Content.Shared.Administration.Notes.AdminMessageEuiState;

namespace Content.Server._Crescent.Factions;

/// <summary>
/// This handles...
/// </summary>
public sealed partial class AccesElevatorSystem : EntitySystem
{
    [Dependency] private readonly AccessSystem _acces = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AccesElevatorComponent,AfterInteractEvent>(AfterUse);

    }

    public void AfterUse(EntityUid owner, AccesElevatorComponent comp, ref AfterInteractEvent args)
    {
        if (args.Target is null)
            return;
        if(_acces.AddExtraTags(args.Target.Value, comp.giveAcces, null))
            _chat.TrySendInGameICMessage(owner, "Acces codes succesfully transmitted!", InGameICChatType.Speak, ChatTransmitRange.Normal);
        else
            _chat.TrySendInGameICMessage(owner, "Error 404: Unable to locate RFID Receiver", InGameICChatType.Speak, ChatTransmitRange.Normal);


    }
}

