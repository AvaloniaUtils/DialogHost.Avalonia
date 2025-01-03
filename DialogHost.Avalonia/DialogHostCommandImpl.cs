using System;
using System.Windows.Input;
using DialogHostAvalonia.Utilities;

namespace DialogHostAvalonia;

internal class DialogHostCommandImpl : ICommand {
    private readonly Func<object, bool> _canExecuteFunc;
    private readonly Action<object> _executeFunc;

    public DialogHostCommandImpl(Action<object> executeFunc, Func<object, bool>? canExecuteFunc, IObservable<bool> canExecuteChangedObservable) {
        _canExecuteFunc = canExecuteFunc ?? (o => true) ;
        _executeFunc = executeFunc;
        canExecuteChangedObservable.Subscribe(_ => OnCanExecuteChanged());
    }

    public bool CanExecute(object parameter) {
        return _canExecuteFunc(parameter);
    }

    public void Execute(object parameter) {
        _executeFunc(parameter);
    }

    public event EventHandler? CanExecuteChanged;

    protected internal virtual void OnCanExecuteChanged() {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}