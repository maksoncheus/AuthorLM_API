using AuthorLM.Client.ViewModels;

namespace AuthorLM.Client.Views;

public partial class BookPage : ContentPage
{
	public BookPage(BookPageViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
        Task t = new Task(loadView);
		t.Start();
		//loadView(vm);
	}
	private async void loadView()
	{
		await Task.Delay(1000);
        await Dispatcher.DispatchAsync(async () => await lazyViewBookPage.LoadViewAsync());
    }
}