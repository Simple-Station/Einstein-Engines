using Content.Shared.CartridgeLoader;
using Robust.Shared.Serialization;

namespace Content.Shared.NanoMessage.Events.Cartridge;

[Serializable, NetSerializable]
public sealed class NanoMessageCartridgeAddRecipientRequest : CartridgeMessageEvent
{
    public ulong Id;
}
