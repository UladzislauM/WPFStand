using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TestStandApp.Infrastructure.Commands
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
            //return _canExecute == null || _canExecute(parameter);
            throw new NotImplementedException();
        }

        public async void Execute(object? parameter)
        {
            //await ExecuteAsync(parameter);
            throw new NotSupportedException();
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
