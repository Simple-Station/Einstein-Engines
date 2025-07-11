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
            // If there's a stored message, apply it
            if (LastReceivedMessage is PenLightUserMessage lastMessage)
                _window.Diagnose(lastMessage);
        }

        private PenLightUserMessage? LastReceivedMessage;

        protected override void ReceiveMessage(BoundUserInterfaceMessage message)
        {
            if (message is not PenLightUserMessage cast)
                return;

            LastReceivedMessage = cast; // Store the message in case UI isn't open yet

            if (_window == null)
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
