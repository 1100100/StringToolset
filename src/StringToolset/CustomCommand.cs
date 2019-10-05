using System;
using System.Windows.Input;
using ICSharpCode.AvalonEdit;

namespace StringToolset
{
    public class CustomCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;
        private TextEditor _editor;


        public CustomCommand(Action<object> execute, Predicate<object> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute ?? throw new ArgumentNullException(nameof(canExecute));
        }

        public CustomCommand(Action<object> execute) : this(execute, DefaultCanExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute != null && _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }
        private static bool DefaultCanExecute(object parameter)
        {
            return true;
        }

        public void OnCanExecuteChanged()
        {
            var handler = CanExecuteChangedInternal;
            handler?.Invoke(this, EventArgs.Empty);
        }


        private event EventHandler CanExecuteChangedInternal;

        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
                CanExecuteChangedInternal += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
                CanExecuteChangedInternal -= value;
            }
        }
    }
}
