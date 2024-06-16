using AuthorLM.Client.Services;

namespace AuthorLM.Client
{
    public partial class App : Application
    {
        public const string APP_NAME = "Книжечки";
        public App(NavigationService navigationService)
        {
            InitializeComponent();

            MainPage = new NavigationPage() { BarBackgroundColor = Color.FromRgb(162, 118, 118) };
            navigationService.NavigateToMainPage();
        }
    }
}
