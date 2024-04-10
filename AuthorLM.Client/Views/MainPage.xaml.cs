using AuthorLM.Client.Services;
using AuthorLM.Client.ViewModels;

namespace AuthorLM.Client.Views
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage(MainPageViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }

}
