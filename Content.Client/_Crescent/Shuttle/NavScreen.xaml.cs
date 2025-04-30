using Content.Shared._NF.Shuttles.Events;
using Content.Shared.Fax;
using Content.Shared.NamedModules.Components;
using Content.Shared.Shuttles.BUIStates;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.Shuttles.UI
{
    public sealed partial class NavScreen
    {
        private EntityUid? console;

        public Action<int>? modulePressed;
        public Action<List<string>>? OnRename;

        private Dictionary<int, Button> _buttons = new();
        private Dictionary<int, LineEdit> _editable = new();

        public bool RenameModeToggle = false;

        private void HullrotInitialize()
        {

            _buttons[1] = Group1;
            _buttons[2] = Group2;
            _buttons[3] = Group3;
            _buttons[4] = Group4;
            _buttons[5] = Group5;
            for (int i = 1; i <= 5; i++)
            {
                int copy = i;
                _buttons[i].OnPressed += (args) => modulePressed?.Invoke(copy);
            }
            foreach(var (index, button) in _buttons)
            {
                var specialUi = new LineEdit();
                if(button.Text is not null)
                    specialUi.Text = button.Text;
                _editable[index] = specialUi;
            }

            RenameButton.OnPressed += RenamePressed;

        }

        private void RenamePressed(BaseButton.ButtonEventArgs args)
        {
            RenameModeToggle = !RenameModeToggle;
            if (RenameModeToggle)
                ButtonsToEditState();
            else
            {
                List<string> newNames = new();
                foreach(var (index, lineedit) in _editable)
                {
                    newNames.Add(lineedit.Text);
                }
                UpdateComponentNames(newNames);
                OnRename?.Invoke(newNames);
                ButtonsToReadyState();
            }
        }

        public void ButtonsToReadyState()
        {
            UpdateButtonNames();
            ButtonHolder.RemoveAllChildren();
            foreach (var (index, button) in _buttons)
            {
                ButtonHolder.AddChild(button);
            }

        }

        public void ButtonsToEditState()
        {
            UpdateButtonNames();
            ButtonHolder.RemoveAllChildren();
            foreach (var (index, line) in _editable)
                ButtonHolder.AddChild(line);
        }


        private void HullrotUpdateState(NavInterfaceState scc)
        {
            console = _entManager.GetEntity(scc.console);
            UpdateButtonNames();
        }

        public void UpdateButtonNames()
        {
            if (console is not null && _entManager.TryGetComponent<NamedModulesComponent>(console, out var namesComp))
            {
                foreach (var (index, button) in _buttons)
                {
                    button.Text = namesComp.ButtonNames[index-1];
                    _editable[index].Text = namesComp.ButtonNames[index-1];
                }
            }
        }

        private void UpdateComponentNames(List<string> newNames)
        {
            if (console is not null && _entManager.TryGetComponent<NamedModulesComponent>(console, out var namesComp))
                namesComp.ButtonNames = newNames;
        }

    }
}
