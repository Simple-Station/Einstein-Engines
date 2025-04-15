using Robust.Shared.Serialization;


namespace Content.Shared.Crescent.Redirect;


[Serializable, NetSerializable]
public sealed class RedirectMessage : EntityEventArgs
{
    public string RedirectUrl = "";

    public RedirectMessage(string url)
    {
        RedirectUrl = url;
    }
}
