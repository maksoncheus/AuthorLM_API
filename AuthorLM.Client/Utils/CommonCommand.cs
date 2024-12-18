﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AuthorLM.Client.Utils
{
    public class CommonCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public CommonCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter = null) => _canExecute?.Invoke() ?? true;

        public void Execute(object? parameter)
        {
            if (CanExecute(parameter)) _execute.Invoke();
            RaiseCanExecuteChanged();
        }

        public void RaiseCanExecuteChanged() =>
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public class CommonCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Predicate<T>? _canExecute;

        public CommonCommand(Action<T> execute, Predicate<T>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object parameter) => _canExecute?.Invoke((T)parameter) ?? true;


        public void Execute(object parameter)
        {
            if (CanExecute(parameter)) _execute.Invoke((T)parameter);
            RaiseCanExecuteChanged();
        }

        public void RaiseCanExecuteChanged() =>
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
