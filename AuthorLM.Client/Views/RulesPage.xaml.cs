using AuthorLM.Client.ViewModels;

namespace AuthorLM.Client.Views;

public partial class RulesPage : ContentPage
{
	public RulesPage(RulesPageViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}