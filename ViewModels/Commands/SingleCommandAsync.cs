using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TestStandApp.ViewModels.Commands
{
    internal class SingleCommandAsync : Command
    {
        private readonly Func<Task> _executeAsync;
        private readonly Func<object?, bool>? _canExecute;

        public SingleCommandAsync(Func<Task> executeAsync, Func<object?, bool>? canExecute = null)
        {
            _executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
            _canExecute = canExecute;
        }

        public override bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute(parameter);
            //throw new NotImplementedException();
        }

        public override async void Execute(object? parameter)
        {
            await ExecuteAsync(parameter);
            //throw new NotSupportedException();
        }

        public async Task ExecuteAsync(object? parameter)
        {
            if (CanExecute(parameter))
            {
                await _executeAsync();
            }
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
