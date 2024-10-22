using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;

namespace Content.Client._LostParadise.Clothing;

/// <summary>
/// Made by BL02DL from _LostParadise
/// </summary>

public sealed class NightVisionOverlay : Overlay
{
    private readonly IPrototypeManager _prototypeManager;
    private readonly NightVisionEntitySystem _nightVisionEntitySystem;

    public override bool RequestScreenTexture => true;
    public override OverlaySpace Space => OverlaySpace.WorldSpace;

    private readonly ShaderInstance _shader;

    public NightVisionOverlay()
    {
        IoCManager.InjectDependencies(this);
        _nightVisionEntitySystem = IoCManager.Resolve<NightVisionEntitySystem>();
        _prototypeManager = IoCManager.Resolve<IPrototypeManager>();
        _shader = _prototypeManager.Index<ShaderPrototype>("LPPNightVision").InstanceUnique();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture == null)
            return;

        var handle = args.WorldHandle;
        var nightcomp = _nightVisionEntitySystem.GetNightComp();

        // Получение данных об оверлее Ночного Видения
        if (nightcomp == null)
        {
            Logger.Error("Failed to get night vision component from eyes.");
            return;
        }

        _shader.SetParameter("SCREEN_TEXTURE", ScreenTexture);
        _shader.SetParameter("tint", nightcomp.Tint);
        _shader.SetParameter("luminance_threshold", nightcomp.Strength);
        _shader.SetParameter("noise_amount", nightcomp.Noise);

        handle.UseShader(_shader);
        handle.DrawRect(args.WorldBounds, Color.White);
        handle.UseShader(null);
    }
}
