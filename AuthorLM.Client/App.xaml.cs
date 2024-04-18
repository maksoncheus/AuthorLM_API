using AuthorLM.Client.Services;

namespace AuthorLM.Client
{
    public partial class App : Application
    {
        public const string APP_NAME = "Бомбуха приложуха";
        public App(NavigationService navigationService)
        {
            InitializeComponent();

            MainPage = new NavigationPage();
            navigationService.NavigateToMainPage();
        }
    }
}
