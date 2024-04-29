using AuthorLM.Client.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthorLM.Client.ViewModels
{
    public class MyLibraryViewModel : ViewModel
    {
        private readonly NavigationService _navigation;
        public Command ToBook
        {
            get => new(async () => await _navigation.NavigateToBookPage(3));
        }
        public MyLibraryViewModel( NavigationService navigation )
        {
            _navigation = navigation;
        }
    }
}
