using AuthorLM.Client.ViewModels;
using CommunityToolkit.Maui.Core.Platform;

namespace AuthorLM.Client.Views;

public partial class CatalogPage : ContentPage
{
	public CatalogPage(CatalogPageViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}

    private async void searchButton_Clicked(object sender, EventArgs e)
    {
		(sender as Button).Clicked -= searchButton_Clicked;
		(sender as Button).Clicked += searchButton_ClickedClose;
		searchContent.IsVisible = true;
		await searchContent.ScaleYTo(1, easing: Easing.Linear);
    }
	private async void searchButton_ClickedClose(object sender, EventArgs e)
	{
        (sender as Button).Clicked += searchButton_Clicked;
        (sender as Button).Clicked -= searchButton_ClickedClose;
		foreach(var child in  searchContent.Children)
		{
			child.Unfocus();
			if (child is ITextInput)
				await (child as ITextInput).HideKeyboardAsync();
		}
        await searchContent.ScaleYTo(0, easing: Easing.Linear);
        searchContent.IsVisible = false;
    }
}