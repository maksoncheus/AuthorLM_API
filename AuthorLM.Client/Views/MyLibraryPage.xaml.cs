using AuthorLM.Client.ViewModels;

namespace AuthorLM.Client.Views;

public partial class MyLibraryPage : TabbedPage
{
	public MyLibraryPage(MyLibraryViewModel vm)
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
    }
}