using Content.Shared.Medical;
using JetBrains.Annotations;
using Robust.Client.GameObjects;

namespace Content.Client.Eye.PenLight.UI
{
    [UsedImplicitly]
    public sealed class PenLightBoundUserInterface : BoundUserInterface
    {
        [ViewVariables]
        private PenLightWindow? _window;

        public PenLightBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey) { }

        protected override void Open()
        {
            base.Open();
            _window = new PenLightWindow
            {
                Title = EntMan.GetComponent<MetaDataComponent>(Owner).EntityName,
            };
            _window.OnClose += Close;
            _window.OpenCentered();
        }

        protected override void ReceiveMessage(BoundUserInterfaceMessage message)
        {
            if (_window == null
                || message is not PenLightUserMessage cast)
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
