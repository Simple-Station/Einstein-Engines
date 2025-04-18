using Content.Shared.Parallax;

namespace Content.Server.Parallax;


public sealed class ParallaxSystem : SharedParallaxSystem
{
    public void SwapParallax(EntityUid uid, ParallaxComponent parallax, string newParallax, float duration)
    {
        parallax.SwappedParallax = parallax.Parallax;
        parallax.Parallax = newParallax;
        parallax.SwapTimer = 0;
        parallax.SwapDuration = duration;
        Dirty(uid, parallax);
    }
}
