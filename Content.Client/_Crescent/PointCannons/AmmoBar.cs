using System.Numerics;
using Content.Client.Stylesheets;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Label = Robust.Client.UserInterface.Controls.Label;

namespace Content.Client.PointCannons;

public sealed class AmmoBar : Control
{
    private int _value;
    public int Value
    {
        get
        {
            return _value;
        }
        set
        {
            _value = value;
            _bar.Value = value;
            _label.Text = value.ToString();
        }
    }

    private int _maxValue;
    public int MaxValue
    {
        get
        {
            return _maxValue;
        }
        set
        {
            _maxValue = value;
            _bar.MaxValue = value;
        }
    }

    private Label _label;
    private ProgressBar _bar;

    public AmmoBar()
    {
        MinHeight = 15;
        HorizontalExpand = true;
        VerticalAlignment = VAlignment.Center;

        AddChild(new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            HorizontalExpand = true,
            Margin = new Thickness(0, 1),
            Children =
            {
                new Control { MinSize = new Vector2(5, 0) },
                new Control
                {
                    HorizontalExpand = true,
                    MaxHeight = 18,
                    Children =
                    {
                        (_bar = new ProgressBar
                        {
                            MinValue = 0,
                        }),
                        (_label = new Label
                        {
                            StyleClasses = { StyleNano.StyleClassItemStatus },
                            Align = Label.AlignMode.Center
                        })
                    }
                },
                new Control { MinSize = new Vector2(5, 0) },
            }
        });
    }
}
