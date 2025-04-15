using Content.Shared._Crescent.SpaceBiomes;
using Robust.Shared.Prototypes;
using Content.Client.Audio;
using Robust.Client.Graphics;

namespace Content.Client._Crescent.SpaceBiomes;

public sealed class SpaceBiomeSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _protMan = default!;
    [Dependency] private readonly IOverlayManager _overMan = default!;
    [Dependency] private readonly ContentAudioSystem _audioSys = default!;

    private SpaceBiomeTextOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<SpaceBiomeSwapMessage>(OnSwap);
        SubscribeNetworkEvent<NewVesselEnteredMessage>(OnNewVesselEntered);
        _overlay = new();
        _overMan.AddOverlay(_overlay);
    }

    private void OnSwap(SpaceBiomeSwapMessage ev)
    {
        _audioSys.ForceUpdateAmbientMusic();
        SpaceBiomePrototype biome = _protMan.Index<SpaceBiomePrototype>(ev.Biome);
        _overlay.Reset();
        _overlay.Text = biome.Name;
        _overlay.CharInterval = TimeSpan.FromSeconds(2f / biome.Name.Length);
    }

    private void OnNewVesselEntered(NewVesselEnteredMessage ev)
    {
        if (_overlay.Text != null)
            return;

        _overlay.Text = ev.Name + ", " + ev.Designation;
        _overlay.CharInterval = TimeSpan.FromSeconds(2f / _overlay.Text.Length);
    }
}
