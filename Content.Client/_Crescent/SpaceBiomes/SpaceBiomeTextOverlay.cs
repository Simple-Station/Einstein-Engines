using System.ComponentModel;
using System.Numerics;
using System.Text;
using Content.Client.Resources;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Shared.Enums;
using Robust.Shared.Timing;

namespace Content.Client._Crescent.SpaceBiomes;

public sealed class SpaceBiomeTextOverlay : Overlay
{
    [Dependency] private IResourceCache _cache = default!;
    [Dependency] private IGameTiming _timing = default!;

    public override OverlaySpace Space => OverlaySpace.ScreenSpace;
    private Font _font;
    private Font _descriptionfont;

    public string? Text;
    public int Index;
    public bool Reverse;
    public Vector2 Position;
    public TimeSpan CharInterval;
    private TimeSpan _nextUpd = TimeSpan.Zero;

    public string? TextDescription;
    public TimeSpan CharIntervalDescription;
    public int IndexDescription;
    public bool ReverseDescription;
    public Vector2 PositionDescription;
    private TimeSpan _nextUpdDescription = TimeSpan.Zero;

    public SpaceBiomeTextOverlay()
    {
        IoCManager.InjectDependencies(this);
        _font = _cache.GetFont("/Fonts/Fondamento-Regular.ttf", 25);
        _descriptionfont = _cache.GetFont("/Fonts/Fondamento-Regular.ttf", 15);
    }

    public void Reset()
    {
        Text = null;
        Index = 0;
        Reverse = false;
        Position = Vector2.Zero;
        _nextUpd = TimeSpan.Zero;
    }
    public void ResetDescription()
    {
        TextDescription = null;
        IndexDescription = 0;
        ReverseDescription = false;
        PositionDescription = Vector2.Zero;
        _nextUpdDescription = TimeSpan.Zero;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (Text == null || Text == string.Empty)
            return;

        DrawDescription(args); //if there is no description, this returns almost immediately so it shouldnt interfere

        if (Position == Vector2.Zero)
            Position = CalcPosition(_font, Text, new Vector2(args.ViewportBounds.Width, args.ViewportBounds.Height));

        args.ScreenHandle.DrawString(_font, Position, Text[..Index]);

        if (_nextUpd > _timing.CurTime)
            return;

        if (!Reverse && Index == Text.Length)
        {
            Reverse = true;

            //delay before text is erased
            _nextUpd += TimeSpan.FromSeconds(2);
            Index++;
        }

        if (Reverse && Index == 0)
        {
            Reset();
            return;
        }

        Index = Reverse ? Index - 1 : Index + 1;

        if (_nextUpd == TimeSpan.Zero)
            _nextUpd = _timing.CurTime;
        _nextUpd += CharInterval;
    }

    private void DrawDescription(OverlayDrawArgs args)
    {
        if (TextDescription == null || TextDescription == string.Empty)
            return;


        if (PositionDescription == Vector2.Zero)
            PositionDescription = CalcPositionDescription(_descriptionfont, TextDescription, new Vector2(args.ViewportBounds.Width, args.ViewportBounds.Height));


        args.ScreenHandle.DrawString(_descriptionfont, PositionDescription, TextDescription[..IndexDescription], Color.DarkGray);

        if (_nextUpdDescription > _timing.CurTime)
            return;

        if (!ReverseDescription && IndexDescription == TextDescription.Length)
        {
            ReverseDescription = true;

            //delay before description is erased
            _nextUpdDescription += TimeSpan.FromSeconds(2);
            IndexDescription++;
        }

        if (ReverseDescription && IndexDescription == 0)
        {
            ResetDescription();
            return;
        }

        IndexDescription = ReverseDescription ? IndexDescription - 1 : IndexDescription + 1;

        if (_nextUpdDescription == TimeSpan.Zero)
            _nextUpdDescription = _timing.CurTime;
        _nextUpdDescription += CharIntervalDescription;
    }

    private Vector2 CalcPosition(Font font, string str, Vector2 viewport)
    {
        Vector2 strSize = new();
        foreach (Rune r in str)
        {
            if (font.TryGetCharMetrics(r, 1, out var metrics))
            {
                strSize.X += metrics.Width;
                strSize.Y = Math.Max(strSize.Y, metrics.Height);
            }
        }

        Vector2 pos = new Vector2((viewport.X - strSize.X) / 2, strSize.Y + 110);
        return pos;
    }

    private Vector2 CalcPositionDescription(Font font, string str, Vector2 viewport)
    {
        Vector2 strSize = new();
        foreach (Rune r in str)
        {
            if (font.TryGetCharMetrics(r, 1, out var metrics))
            {
                strSize.X += metrics.Width;
                strSize.Y = Math.Max(strSize.Y, metrics.Height);
            }
        }

        Vector2 pos = new Vector2((viewport.X - strSize.X) / 2, strSize.Y + 110 + 70); //70 should be enough to give the title font space
        return pos;
    }
}
