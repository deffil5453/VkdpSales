using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;

namespace VkdpSales.ViewModels.Commands
{
    public class VKDPCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public VKDPCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            if (_canExecute == null)
            {
                return true;
            }
            return _canExecute(parameter);
        }

        public void Execute(object? parameter)
        {
            _execute(parameter);
        }
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
