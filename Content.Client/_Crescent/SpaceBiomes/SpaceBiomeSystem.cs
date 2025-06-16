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
        _audioSys.DisableAmbientMusic();
        SpaceBiomePrototype biome = _protMan.Index<SpaceBiomePrototype>(ev.Biome);
        _overlay.Reset();
        _overlay.ResetDescription();
        _overlay.Text = biome.Name;
        _overlay.TextDescription = biome.Description;
        _overlay.CharInterval = TimeSpan.FromSeconds(2f / biome.Name.Length);
        if (_overlay.TextDescription == "")                   //if we have a biome with no description, it's default is "" and that has length 0.
            _overlay.CharIntervalDescription = TimeSpan.Zero;       //we need to calculate it here because otherwise...
        else
            _overlay.CharIntervalDescription = TimeSpan.FromSeconds(2f / biome.Description.Length);      //this would throw an exception
    }

    private void OnNewVesselEntered(NewVesselEnteredMessage ev)
    {
        if (_overlay.Text != null)
            return;

        _overlay.Text = ev.Name + ", " + ev.Designation;
        _overlay.TextDescription = ""; // ships have no description for now
        _overlay.CharInterval = TimeSpan.FromSeconds(2f / _overlay.Text.Length);
        _overlay.CharIntervalDescription = TimeSpan.Zero;
        //_overlay.CharIntervalDescription = TimeSpan.FromSeconds(2f / _overlay.TextDescription.Length);
    }
}
