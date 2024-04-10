using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AuthorLM.Client.ViewModels
{
    public class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        public virtual Task OnNavigatingTo(object? parameter)
            => Task.CompletedTask;
        public virtual Task OnNavigatedFrom(bool isForwardNavigation)
            => Task.CompletedTask;
        public virtual Task OnNavigatedTo()
            => Task.CompletedTask;
    }
}
