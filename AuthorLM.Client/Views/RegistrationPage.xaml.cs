using AuthorLM.Client.ViewModels;

namespace AuthorLM.Client.Views;

public partial class RegistrationPage : ContentPage
{
	public RegistrationPage(RegistrationPageViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}