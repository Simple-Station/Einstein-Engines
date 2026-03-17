using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Shared.Utility;
using System.Numerics;

namespace Content.Client._White.UserInterface.Controls;

public sealed class BarControl : Control
{
    private float _fill;
    public float Fill
    {
        get => _fill;
        set
        {
            DebugTools.Assert(value is >= 0 and <= 1);
            _fill = value;
        }
    }
    public float Percentage
    {
        get => _fill * 100f;
        set => Fill = value / 100;
    }
    public int Rows = 1;

    public Color FillColor = Color.Red;
    public Color EmptyColor = Color.DarkRed;
    public Color AltFillColor = Color.InterpolateBetween(Color.Red, Color.Black, 0.125f);
    public Color AltEmptyColor = Color.InterpolateBetween(Color.DarkRed, Color.Black, 0.075f);


    protected override void Draw(DrawingHandleScreen handle)
    {
        // Scale rendering in this control by UIScale.
        var currentTransform = handle.GetTransform();
        handle.SetTransform(Matrix3Helpers.CreateScale(new Vector2(UIScale)) * currentTransform);

        float fillLeft = _fill * Rows;
        float rowHeight = Size.Y / Rows;
        // Draw by rows, bottom to top.
        bool alt = false;
        for (var row = Rows - 1; row >= 0; row--)
        {
            Color fill = alt ? FillColor : AltFillColor;
            Color empty = alt ? EmptyColor : AltEmptyColor;
            float rowFill = MathF.Min(fillLeft, 1);
            fillLeft -= 1;
            Vector2 topLeft = new Vector2(0, rowHeight * row);
            Vector2 bottomRight = topLeft + new Vector2(Size.X, Size.Y / Rows);

            handle.DrawRect(new UIBox2(topLeft, bottomRight), empty);
            if(rowFill > 0)
                handle.DrawRect(new UIBox2(topLeft + new Vector2(Size.X * (1 - rowFill), 0), bottomRight), fill);
            alt = !alt;
            //handle.DrawRect(new UIBox2(Vector2.Zero, Vector2.One*5), Color.Yellow);
        }
    }

    protected override Vector2 MeasureOverride(Vector2 availableSize) => availableSize;
}

public sealed class FluxBarControl : Control
{

    private float _fill;
    private float _mark;
    public float Mark
    {
        get => _mark;
        set
        {
            DebugTools.Assert(value is >= -1 and <= 1);
            _mark = value;
        }
    }
    public float Fill
    {
        get => _fill;
        set
        {
            DebugTools.Assert(value is >= 0 and <= 1);
            _fill = value;
        }
    }

    private float? _safeLimit;
    public float? SafeLimit
    {
        get => _safeLimit;
        set
        {
            DebugTools.Assert(value is >= 0 and <= 1 or null);
            _safeLimit = value;
        }
    }

    public Color SafeLimitColor = Color.Green;


    public Color FillColor = Color.Red;
    public Color MarkColor = Color.InterpolateBetween(Color.Red, Color.Maroon, 0.5f);
    public Color EmptyColor = Color.Maroon;

    private Texture _emptyTex;
    private Texture _markTex;
    private Texture _fillTex;

    public FluxBarControl() : base()
    {
        var spriteSys = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<SpriteSystem>();
        _emptyTex = spriteSys.Frame0(new SpriteSpecifier.Texture(new ResPath("/Textures/_White/Interface/gun-flux-readout-empty.png")));
        _markTex = spriteSys.Frame0(new SpriteSpecifier.Texture(new ResPath("/Textures/_White/Interface/gun-flux-readout-mark.png")));
        _fillTex = spriteSys.Frame0(new SpriteSpecifier.Texture(new ResPath("/Textures/_White/Interface/gun-flux-readout-fill.png")));
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        var currentTransform = handle.GetTransform();
        handle.SetTransform(Matrix3Helpers.CreateScale(new Vector2(UIScale)) * currentTransform);

        var textureWidth = _emptyTex.Width * UIScale;
        var fillWidth = Width * Fill;
        var markWidth = Width * MathHelper.Clamp01(Fill + Mark);

        var emptyBox = new UIBox2(Vector2.Zero, Size);
        var fillBox = new UIBox2(Vector2.Zero, new Vector2(fillWidth, Height));
        var markBox = new UIBox2(new Vector2(fillWidth, 0), new Vector2(markWidth,Height));

        handle.DrawTextureRectRegion(_emptyTex, emptyBox);
        handle.DrawTextureRectRegion(_fillTex, fillBox, fillBox);
        handle.DrawTextureRectRegion(_markTex, markBox, markBox);
        if (_safeLimit is { } safe)
            handle.DrawLine(new(safe * Width, 0), new Vector2(safe * Width, Height), SafeLimitColor);


        UIBox2 SizeBox(UIBox2 box) => new UIBox2(Vector2.Zero, box.Size);
    }

    protected override Vector2 MeasureOverride(Vector2 availableSize) => _emptyTex.Size * UIScale;
}

