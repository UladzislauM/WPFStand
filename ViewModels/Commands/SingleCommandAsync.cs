using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TestStandApp.ViewModels.Commands
{
    internal class SingleCommandAsync : ICommand
    {
        private readonly Func<Task> _executeAsync;
        private readonly Func<object?, bool>? _canExecute;

        public SingleCommandAsync(Func<Task> executeAsync, Func<object?, bool>? canExecute = null)
        {
            this._executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
            this._canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public async void Execute(object? parameter)
        {
            await ExecuteAsync(parameter);
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
