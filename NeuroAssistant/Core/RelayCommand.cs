using System;
using System.Windows.Input;

namespace NeuroAssistant.Core
{
    public static class CommandFactory
    {
        public static ICommand CreateCommand(Action action) => new RelayCommand(action);
        public static ICommand CreateCommand(Action<object> action) => new RelayCommand(action);
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Action<object> _action;

        private readonly bool _canExecute = true;
        private readonly Func<bool> canExecuteFunc;

        public RelayCommand(Action execute) : this(execute, null)
        {
        }

        public RelayCommand(Action<object> action, bool canExecute)
        {
            _action = action;
            _canExecute = canExecute;
        }

        public RelayCommand(Action<object> action)
        {
            _action = action;
        }

        public RelayCommand(Action action, bool canExecute)
        {
            _execute = action;
            _canExecute = canExecute;
        }

        public RelayCommand(Action execute, Func<bool> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException("execute");
            canExecuteFunc = canExecute;
        }

        public bool CanExecute(object parameter) => canExecuteFunc == null ? _canExecute : canExecuteFunc();

        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (canExecuteFunc != null)
                {
                    CommandManager.RequerySuggested += value;
                }
            }
            remove
            {
                if (canExecuteFunc != null)
                {
                    CommandManager.RequerySuggested -= value;
                }
            }
        }

        public void Execute(object parameter)
        {
            if (_execute != null)
            {
                _execute();
                return;
            }
            else if (_action != null)
            {
                _action(parameter);
                return;
            }
        }
    }
}
