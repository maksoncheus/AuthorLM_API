using AuthorLM.Client.ViewModels;

namespace AuthorLM.Client.Views;

public partial class BookPage : ContentPage
{
	public BookPage(BookPageViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}