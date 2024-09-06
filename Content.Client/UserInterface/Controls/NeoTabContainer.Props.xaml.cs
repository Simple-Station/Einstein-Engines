using static Robust.Client.UserInterface.Controls.BoxContainer.LayoutOrientation;
using static Robust.Shared.Maths.Direction;

namespace Content.Client.UserInterface.Controls;

public sealed partial class NeoTabContainer
{
    // Too many computed properties...

    /// <summary>
    ///     Where to place the tabs in relation to the contents
    ///     <br />
    ///     If <see cref="Direction.North"/>, the tabs will be above the contents
    /// </summary>
    private Direction _tabLocation = North;
    public Direction TabLocation
    {
        get => _tabLocation;
        set => LayoutChanged(value);
    }

    /// If the <see cref="ContentContainer"/>'s horizontal scroll is enabled
    private bool _hScrollEnabled;
    /// <inheritdoc cref="_hScrollEnabled"/>
    public bool HScrollEnabled
    {
        get => _hScrollEnabled;
        set => ScrollingChanged(value, _vScrollEnabled);
    }

    /// If the <see cref="ContentContainer"/>'s vertical scroll is enabled
    private bool _vScrollEnabled;
    /// <inheritdoc cref="_vScrollEnabled"/>
    public bool VScrollEnabled
    {
        get => _vScrollEnabled;
        set => ScrollingChanged(_hScrollEnabled, value);
    }

    /// The margin around the whole UI element
    private Thickness _containerMargin = new(0);
    /// <inheritdoc cref="_containerMargin"/>
    public Thickness ContainerMargin
    {
        get => _containerMargin;
        set => ContainerMarginChanged(value);
    }

    /// The margin around the separator between the tabs and contents
    public Thickness SeparatorMargin
    {
        get => Separator.Margin;
        set => Separator.Margin = value;
    }

    private bool _firstTabOpenBoth;
    public bool FirstTabOpenBoth
    {
        get => _firstTabOpenBoth;
        set => TabStyleChanged(value, LastTabOpenBoth);
    }

    private bool _lastTabOpenBoth;
    public bool LastTabOpenBoth
    {
        get => _lastTabOpenBoth;
        set => TabStyleChanged(FirstTabOpenBoth, value);
    }


    /// Changes the layout of the tabs and contents based on the value
    /// <param name="direction">See <see cref="TabLocation"/></param>
    private void LayoutChanged(Direction direction)
    {
        _tabLocation = direction;

        LayoutOrientation DirectionalOrientation(Direction direction, LayoutOrientation north, LayoutOrientation south,
            LayoutOrientation east, LayoutOrientation west)
        {
            return direction switch
            {
                North => north,
                South => south,
                East => east,
                West => west,
                _ => Vertical,
            };
        }
        TabContainer.Orientation = DirectionalOrientation(direction, Horizontal, Horizontal, Vertical, Vertical);
        Container.Orientation = DirectionalOrientation(direction, Vertical, Vertical, Horizontal, Horizontal);

        var containerMargin = direction switch
        {
            North => new Thickness(_containerMargin.Left, 0, _containerMargin.Right, _containerMargin.Bottom),
            South => new Thickness(_containerMargin.Left, _containerMargin.Top, _containerMargin.Right, 0),
            East => new Thickness(_containerMargin.Left, _containerMargin.Top, 0, _containerMargin.Bottom),
            West => new Thickness(0, _containerMargin.Top, _containerMargin.Right, _containerMargin.Bottom),
            _ => _containerMargin,
        };
        TabScrollContainer.Margin = containerMargin;
        ContentScrollContainer.Margin = containerMargin;

        bool DirectionalBool(Direction direction, bool north, bool south, bool east, bool west)
        {
            return direction switch
            {
                North => north,
                South => south,
                East => east,
                West => west,
                _ => false,
            };
        }
        TabScrollContainer.HorizontalExpand = DirectionalBool(direction, true, true, false, false);
        TabScrollContainer.VerticalExpand = DirectionalBool(direction, false, false, true, true);
        TabScrollContainer.HScrollEnabled = DirectionalBool(direction, true, true, false, false);
        TabScrollContainer.VScrollEnabled = DirectionalBool(direction, false, false, true, true);


        // Move the TabScrollContainer, Separator, and ContentScrollContainer to the correct positions
        TabScrollContainer.Orphan();
        Separator.Orphan();
        ContentScrollContainer.Orphan();

        if (direction is North or West)
        {
            Container.AddChild(TabScrollContainer);
            Container.AddChild(Separator);
            Container.AddChild(ContentScrollContainer);
        }
        else
        {
            Container.AddChild(ContentScrollContainer);
            Container.AddChild(Separator);
            Container.AddChild(TabScrollContainer);
        }

        UpdateTabMerging();
    }

    private void ScrollingChanged(bool hScroll, bool vScroll)
    {
        _hScrollEnabled = hScroll;
        _vScrollEnabled = vScroll;

        ContentScrollContainer.HScrollEnabled = hScroll;
        ContentScrollContainer.VScrollEnabled = vScroll;
    }

    private void ContainerMarginChanged(Thickness value)
    {
        _containerMargin = value;
        LayoutChanged(TabLocation);
    }

    private void TabStyleChanged(bool firstTabOpenBoth, bool lastTabOpenBoth)
    {
        _firstTabOpenBoth = firstTabOpenBoth;
        _lastTabOpenBoth = lastTabOpenBoth;

        UpdateTabMerging();
    }
}
