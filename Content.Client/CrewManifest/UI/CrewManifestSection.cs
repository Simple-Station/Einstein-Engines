#region

using Content.Shared.CrewManifest;
using Content.Shared.Roles;
using Content.Shared.StatusIcon;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Prototypes;

#endregion


namespace Content.Client.CrewManifest.UI;


public sealed class CrewManifestSection : BoxContainer
{
    public CrewManifestSection(
        IPrototypeManager prototypeManager,
        SpriteSystem spriteSystem,
        DepartmentPrototype section,
        List<CrewManifestEntry> entries
    )
    {
        Orientation = LayoutOrientation.Vertical;
        HorizontalExpand = true;

        AddChild(
            new Label
            {
                StyleClasses = { "LabelBig", },
                Text = Loc.GetString($"department-{section.ID}")
            });

        var gridContainer = new GridContainer
        {
            HorizontalExpand = true,
            Columns = 2
        };

        AddChild(gridContainer);

        foreach (var entry in entries)
        {
            var name = new RichTextLabel
            {
                HorizontalExpand = true
            };
            name.SetMessage(entry.Name);

            var titleContainer = new BoxContainer
            {
                Orientation = LayoutOrientation.Horizontal,
                HorizontalExpand = true
            };

            var title = new RichTextLabel();
            title.SetMessage(entry.JobTitle);


            if (prototypeManager.TryIndex<StatusIconPrototype>(entry.JobIcon, out var jobIcon))
            {
                var icon = new TextureRect
                {
                    TextureScale = new(2, 2),
                    VerticalAlignment = VAlignment.Center,
                    Texture = spriteSystem.Frame0(jobIcon.Icon),
                    Margin = new(0, 0, 4, 0)
                };

                titleContainer.AddChild(icon);
                titleContainer.AddChild(title);
            }
            else
                titleContainer.AddChild(title);

            gridContainer.AddChild(name);
            gridContainer.AddChild(titleContainer);
        }
    }
}
