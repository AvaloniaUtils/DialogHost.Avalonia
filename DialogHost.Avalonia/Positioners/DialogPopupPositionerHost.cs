using Avalonia.Controls.Primitives.PopupPositioning;

namespace DialogHostAvalonia.Positioners {
    internal class DialogPopupPositionerHost : IPopupPositioner {
        private IManagedPopupPositionerPopup _popup;
        internal IDialogPopupPositioner? _dialogPopupPositioner;

        public DialogPopupPositionerHost(IManagedPopupPositionerPopup popup, IDialogPopupPositioner? dialogPopupPositioner) {
            _popup = popup;
            _dialogPopupPositioner = dialogPopupPositioner;
        }

        public void Update(PopupPositionerParameters parameters) {
            var dialogPopupPositioner = _dialogPopupPositioner ?? CenteredDialogPopupPositioner.Instance;
            dialogPopupPositioner.Update(_popup, parameters);
        }
    }
}