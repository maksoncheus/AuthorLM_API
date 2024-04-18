using AuthorLM.Client.Services;
using AuthorLM.Client.ViewModels;

namespace AuthorLM.Client.Views
{
    public partial class MainPage : ContentPage
    {
        int count = 0;
        private MainPageViewModel viewModel;
        public MainPage(MainPageViewModel vm)
        {
            InitializeComponent();
            BindingContext = viewModel = vm;
            Init();
        }
        private async void Init()
        {
            //await Task.Delay(1000);
            await lazyViewMainPage.LoadViewAsync();
        }
    }

}
