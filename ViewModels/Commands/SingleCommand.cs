using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TestStandApp.ViewModels.Commands
{
    internal class SingleCommand : Command
    {
        private readonly Action<object> _execute;
        private readonly Func<object?, bool>? _canExecute;

        public SingleCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public override bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute(parameter);
            //throw new NotImplementedException();
        }

        public override async void Execute(object? parameter)
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
