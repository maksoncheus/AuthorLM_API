using AuthorLM.Client.ViewModels;

namespace AuthorLM.Client.Views;

public partial class AuthorizationPage : ContentPage
{
	public AuthorizationPage(AuthorizationViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}