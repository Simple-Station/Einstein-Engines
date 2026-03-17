using System.Linq;
using System.Numerics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Client._White.UserInterface.Controls;

[Virtual]
public class FlexBox : Container
{
    public enum FlexAlignContent
    {
        FlexStart,
        FlexEnd,
        Center,
        Stretch,
        SpaceBetween,
        SpaceAround
    }

    public enum FlexAlignItems
    {
        FlexStart,
        FlexEnd,
        Center,
        Stretch
    }

    public enum FlexDirection
    {
        Row,
        RowReverse,
        Column,
        ColumnReverse
    }

    public enum FlexJustifyContent
    {
        FlexStart,
        FlexEnd,
        Center,
        SpaceBetween,
        SpaceAround,
        SpaceEvenly
    }

    public enum FlexWrap
    {
        NoWrap,
        Wrap,
        WrapReverse
    }

    public const string StylePropertyGap = "gap";
    public const string StylePropertyRowGap = "row-gap";
    public const string StylePropertyColumnGap = "column-gap";
    public const string StylePropertyAlignItems = "align-items";
    public const string StylePropertyOrder = "order";

    private const float DefaultGap = 0f;

    public FlexDirection Direction { get; set; } = FlexDirection.Row;
    public FlexWrap Wrap { get; set; } = FlexWrap.Wrap;
    public FlexAlignItems AlignItems { get; set; } = FlexAlignItems.Stretch;
    public FlexJustifyContent JustifyContent { get; set; } = FlexJustifyContent.FlexStart;
    public FlexAlignContent AlignContent { get; set; } = FlexAlignContent.FlexStart;

    public float? GapOverride { get; set; }
    public float? RowGapOverride { get; set; }
    public float? ColumnGapOverride { get; set; }

    private float ActualGap => GetStyleFloat(StylePropertyGap, GapOverride, DefaultGap);
    private float ActualRowGap => GetStyleFloat(StylePropertyRowGap, RowGapOverride, ActualGap);
    private float ActualColumnGap => GetStyleFloat(StylePropertyColumnGap, ColumnGapOverride, ActualGap);

    private float GetStyleFloat(string property, float? overrideValue, float defaultValue)
    {
        if (overrideValue.HasValue)
            return overrideValue.Value;

        if (TryGetStyleProperty(property, out float value))
            return value;

        return defaultValue;
    }

    protected override Vector2 MeasureOverride(Vector2 availableSize)
    {
        var children = new List<Control>(Children.Count());
        foreach (var child in Children)
        {
            if (child.Visible)
                children.Add(child);
        }

        if (children.Count == 0)
            return Vector2.Zero;

        foreach (var child in children)
            child.Measure(Vector2.PositiveInfinity);

        var lines = BuildLines(children, availableSize);

        var isRow = IsRow;
        var main = lines.Max(l => l.MainSize);
        var cross = lines.Sum(l => l.FinalCrossSize) +
                    Math.Max(0, lines.Count - 1) * CrossGap;

        return isRow
            ? new Vector2(main, cross)
            : new Vector2(cross, main);
    }

    protected override Vector2 ArrangeOverride(Vector2 finalSize)
    {
        var children = Children.Where(c => c.Visible).ToList();
        if (children.Count == 0)
            return finalSize;

        var lines = BuildLines(children, finalSize);

        PositionLines(lines, finalSize);

        foreach (var line in lines)
            PositionItems(line, finalSize);

        return finalSize;
    }

    private bool IsRow =>
        Direction == FlexDirection.Row ||
        Direction == FlexDirection.RowReverse;

    private bool IsReverse =>
        Direction == FlexDirection.RowReverse ||
        Direction == FlexDirection.ColumnReverse;

    private float MainGap => IsRow ? ActualColumnGap : ActualRowGap;
    private float CrossGap => IsRow ? ActualRowGap : ActualColumnGap;

    private List<FlexLine> BuildLines(
        List<Control> children,
        Vector2 size)
    {
        children.Sort(static (a, b) =>
        {
            var ao = a.TryGetStyleProperty(StylePropertyOrder, out int av) ? av : 0;
            var bo = b.TryGetStyleProperty(StylePropertyOrder, out int bv) ? bv : 0;
            return ao.CompareTo(bo);
        });

        var lines = new List<FlexLine>();
        var line = new FlexLine();

        var maxMain = IsRow ? size.X : size.Y;
        var wrap = Wrap != FlexWrap.NoWrap && !float.IsInfinity(maxMain);

        foreach (var child in children)
        {
            var main = IsRow ? child.DesiredSize.X : child.DesiredSize.Y;
            var cross = IsRow ? child.DesiredSize.Y : child.DesiredSize.X;

            var projected =
                line.Items.Count == 0
                    ? main
                    : line.MainSize + MainGap + main;

            if (wrap && projected > maxMain && line.Items.Count > 0)
            {
                lines.Add(line);
                line = new FlexLine();
            }

            line.Add(child, main, cross, MainGap);
        }

        if (line.Items.Count > 0)
            lines.Add(line);

        return lines;
    }

    private void PositionLines(List<FlexLine> lines, Vector2 size)
    {
        var crossSize = IsRow ? size.Y : size.X;
        var totalCross =
            lines.Sum(l => l.FinalCrossSize) +
            Math.Max(0, lines.Count - 1) * CrossGap;

        var free = Math.Max(0, crossSize - totalCross);

        float offset = 0;
        float extraGap = 0;

        foreach (var l in lines)
            l.ExtraCrossSize = 0;

        switch (AlignContent)
        {
            case FlexAlignContent.FlexEnd:
                offset = free;
                break;
            case FlexAlignContent.Center:
                offset = free / 2;
                break;
            case FlexAlignContent.SpaceBetween:
                if (lines.Count > 1)
                    extraGap = free / (lines.Count - 1);
                break;
            case FlexAlignContent.SpaceAround:
                extraGap = free / lines.Count;
                offset = extraGap / 2;
                break;
            case FlexAlignContent.Stretch:
                var add = free / lines.Count;
                foreach (var l in lines)
                    l.ExtraCrossSize = add;
                break;
        }

        var pos = offset;

        var wrapReverse = Wrap == FlexWrap.WrapReverse;

        foreach (var line in lines)
        {
            line.CrossPos = wrapReverse
                ? crossSize - pos - line.FinalCrossSize
                : pos;

            pos += line.FinalCrossSize + CrossGap + extraGap;
        }
    }

    private void PositionItems(FlexLine line, Vector2 size)
    {
        var mainSize = IsRow ? size.X : size.Y;
        var free = Math.Max(0, mainSize - line.MainSize);

        float offset = 0;
        float extraGap = 0;

        switch (JustifyContent)
        {
            case FlexJustifyContent.FlexEnd:
                offset = free;
                break;
            case FlexJustifyContent.Center:
                offset = free / 2;
                break;
            case FlexJustifyContent.SpaceBetween:
                if (line.Items.Count > 1)
                    extraGap = free / (line.Items.Count - 1);
                break;
            case FlexJustifyContent.SpaceAround:
                extraGap = free / line.Items.Count;
                offset = extraGap / 2;
                break;
            case FlexJustifyContent.SpaceEvenly:
                extraGap = free / (line.Items.Count + 1);
                offset = extraGap;
                break;
        }

        var pos = offset;

        if (IsReverse)
        {
            for (var i = line.Items.Count - 1; i >= 0; i--)
                PositionItem(line.Items[i]);
        }
        else
        {
            for (var i = 0; i < line.Items.Count; i++)
                PositionItem(line.Items[i]);
        }

        return;

        void PositionItem(FlexItem item)
        {
            var align = GetEffectiveAlignItems(item.Control);
            var crossSize = align == FlexAlignItems.Stretch
                ? line.FinalCrossSize
                : item.CrossSize;

            var crossPos = align switch
            {
                FlexAlignItems.Center => line.CrossPos + (line.FinalCrossSize - crossSize) / 2,
                FlexAlignItems.FlexEnd => line.CrossPos + line.FinalCrossSize - crossSize,
                _ => line.CrossPos
            };

            var rect = IsRow
                ? new UIBox2(pos, crossPos, pos + item.MainSize, crossPos + crossSize)
                : new UIBox2(crossPos, pos, crossPos + crossSize, pos + item.MainSize);

            item.Control.Arrange(rect);

            pos += item.MainSize + MainGap + extraGap;
        }
    }

    private int GetFlexOrder(Control c) =>
        c.TryGetStyleProperty(StylePropertyOrder, out int o) ? o : 0;

    private FlexAlignItems GetEffectiveAlignItems(Control c) =>
        c.TryGetStyleProperty(StylePropertyAlignItems, out FlexAlignItems a)
            ? a
            : AlignItems;

    private sealed class FlexLine
    {
        public readonly List<FlexItem> Items = new();
        public float MainSize;
        public float CrossSize;
        public float ExtraCrossSize;
        public float CrossPos;
        public float FinalCrossSize => CrossSize + ExtraCrossSize;

        public void Add(Control c, float main, float cross, float gap)
        {
            if (Items.Count > 0)
                MainSize += gap;

            Items.Add(new FlexItem(c, main, cross));
            MainSize += main;
            CrossSize = Math.Max(CrossSize, cross);
        }
    }

    private sealed class FlexItem
    {
        public readonly Control Control;
        public readonly float MainSize;
        public readonly float CrossSize;

        public FlexItem(Control c, float main, float cross)
        {
            Control = c;
            MainSize = main;
            CrossSize = cross;
        }
    }
}
