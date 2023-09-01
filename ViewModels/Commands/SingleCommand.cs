using System;
using System.Windows.Input;

namespace TestStandApp.ViewModels.Commands
{
    internal class SingleCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object?, bool>? _canExecute;

        public SingleCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            this._execute = execute;
            this._canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }
        public void Execute(object? parameter)
        {
            _execute(parameter ?? "Empty parameter");
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
