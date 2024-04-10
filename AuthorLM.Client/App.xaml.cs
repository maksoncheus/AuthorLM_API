using AuthorLM.Client.Services;

namespace AuthorLM.Client
{
    public partial class App : Application
    {
        public App(NavigationService navigationService)
        {
            InitializeComponent();

            MainPage = new NavigationPage();
            navigationService.NavigateToMainPage();
        }
    }
}
