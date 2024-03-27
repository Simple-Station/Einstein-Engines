namespace Content.Server.Species.Oni.Components
{
    [RegisterComponent]
    public sealed partial class HeldByOniComponent : Component
    {
        public EntityUid Holder = default!;
    }
}
