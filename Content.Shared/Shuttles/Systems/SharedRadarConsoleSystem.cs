using Content.Shared.Shuttles.Components;

namespace Content.Shared.Shuttles.Systems;

public abstract class SharedRadarConsoleSystem : EntitySystem
{
    public const float DefaultMaxRange = 1256f;
    ///Changed from 256f to 1256f, affects bullet ranges only. over 1024 because 1024 causes rendering to stop at edges.
    ///1448 would be best but this is a less common situation so players will not encounter it.
    ///reggie plans to do circle viewport this will work for now, at that time we can do 1024. --wabash

    protected virtual void UpdateState(EntityUid uid, RadarConsoleComponent component)
    {
    }

    public void SetRange(EntityUid uid, float value, RadarConsoleComponent component)
    {
        if (component.MaxRange.Equals(value))
            return;

        component.MaxRange = value;
        Dirty(uid, component);
        UpdateState(uid, component);
    }
}
