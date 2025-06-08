﻿using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Blob;

[Serializable, NetSerializable]
public sealed partial class CreateBlobObserverEvent : CancellableEntityEventArgs
{
    public NetUserId UserId { get; private set; }

    public CreateBlobObserverEvent(NetUserId userId)
    {
        UserId = userId;
    }
}
