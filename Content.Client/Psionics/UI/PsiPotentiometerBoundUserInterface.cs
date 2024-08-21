using Content.Shared.Psionics;
using JetBrains.Annotations;

namespace Content.Client.Psionics.UI
{
    [UsedImplicitly]
    public sealed class PsiPotentiometerBoundUserInterface : BoundUserInterface
    {
        [ViewVariables]
        private PsiPotentiometerWindow? _window;

        public PsiPotentiometerBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey) { }

        protected override void Open()
        {
            base.Open();
            _window = new PsiPotentiometerWindow
            {
                Title = EntMan.GetComponent<MetaDataComponent>(Owner).EntityName,
            };
            _window.OnClose += Close;
            _window.OpenCentered();
        }

        protected override void ReceiveMessage(BoundUserInterfaceMessage message)
        {
            if (_window == null
                || message is not PsiPotentiometerUserMessage cast)
                return;

            _window.Diagnose(cast);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing)
                return;

            if (_window != null)
                _window.OnClose -= Close;

            _window?.Dispose();
        }
    }
}