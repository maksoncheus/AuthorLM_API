using AuthorLM.Client.ViewModels;

namespace AuthorLM.Client.Views;

public partial class ProfilePage : ContentPage
{
	public ProfilePage( ProfilePageViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
        Task t = new Task(loadView);
        t.Start();
    }

    private async void loadView()
    {
        await Task.Delay(1000);
        await Dispatcher.DispatchAsync(async () => await lazyViewProfilePage.LoadViewAsync());
    }
}