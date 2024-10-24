using System.Numerics;
using Content.Shared.Camera;
using Content.Shared.CCVar;
using Content.Shared.Contests;
using Robust.Shared.Configuration;

namespace Content.Client.Camera;

public sealed class CameraRecoilSystem : SharedCameraRecoilSystem
{
    [Dependency] private readonly IConfigurationManager _configManager = default!;
    [Dependency] private readonly ContestsSystem _contests = default!;

    private float _intensity;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<CameraKickEvent>(OnCameraKick);

        Subs.CVar(_configManager, CCVars.ScreenShakeIntensity, OnCvarChanged, true);
    }

    private void OnCvarChanged(float value)
    {
        _intensity = value;
    }

    private void OnCameraKick(CameraKickEvent ev)
    {
        KickCamera(GetEntity(ev.NetEntity), ev.Recoil);
    }

    public override void KickCamera(EntityUid uid, Vector2 recoil, CameraRecoilComponent? component = null)
    {
        if (_intensity == 0)
            return;

        if (!Resolve(uid, ref component, false))
            return;

        var massRatio = _contests.MassContest(uid);
        var maxRecoil = KickMagnitudeMax / massRatio;
        recoil *= _intensity / massRatio;

        var existing = component.CurrentKick.Length();
        component.CurrentKick += recoil * (1 - existing);

        if (component.CurrentKick.Length() > maxRecoil)
            component.CurrentKick = component.CurrentKick.Normalized() * maxRecoil;

        component.LastKickTime = 0;
    }
}
