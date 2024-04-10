using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthorLM.Client.ViewModels
{
    public class BookPageViewModel : ViewModel
    {
        private int bookId;
        public override Task OnNavigatingTo(object? parameter)
        {
            bookId = (int)parameter;
            return base.OnNavigatingTo(parameter);
        }
    }
}
