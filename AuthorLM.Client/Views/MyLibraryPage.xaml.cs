using AuthorLM.Client.ViewModels;

namespace AuthorLM.Client.Views;

public partial class MyLibraryPage : TabbedPage
{
	public MyLibraryPage(MyLibraryViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}