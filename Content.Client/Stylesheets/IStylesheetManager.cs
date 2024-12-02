#region

using Robust.Client.UserInterface;

#endregion


namespace Content.Client.Stylesheets;


public interface IStylesheetManager
{
    Stylesheet SheetNano { get; }
    Stylesheet SheetSpace { get; }

    void Initialize();
}
