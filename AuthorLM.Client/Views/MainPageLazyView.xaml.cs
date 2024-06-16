using CommunityToolkit.Maui.Core.Platform;

namespace AuthorLM.Client.Views;

public partial class MainPageLazyView : ContentView
{
	public MainPageLazyView()
	{
		InitializeComponent();
	}
    private async void OpenProfileFlyout_Clicked(object sender, EventArgs e)
    {
        profileFlyout.IsVisible = true;
        profileContent.CancelAnimations();
        profileContent.TranslationX = 250;
        profileContent.TranslateTo(0, 0, 200, Easing.Linear);
        closeProfile.FadeTo(.25d, 150, Easing.Linear);
    }
    private async void CloseProfileFlyout_Clicked(object sender, EventArgs e)
    {
        profileContent.CancelAnimations();
        closeProfile.CancelAnimations();
        await Task.WhenAll(
            profileContent.TranslateTo(250, 0, 200, Easing.Linear),
        closeProfile.FadeTo(0d, 150, Easing.Linear)
            );
        profileFlyout.IsVisible = false;
    }
    private async void OpenNavigationFlyout_Clicked(object sender, EventArgs e)
    {
        navigationFlyout.IsVisible = true;
        navigationContent.CancelAnimations();
        navigationContent.TranslationX = -250;
        navigationContent.TranslateTo(0, 0, 200, Easing.Linear);
        closeNavigation.FadeTo(.25d, 150, Easing.Linear);
    }
    private async void CloseNavigationFlyout_Clicked(object sender, EventArgs e)
    {
        navigationContent.CancelAnimations();
        closeNavigation.CancelAnimations();
        await Task.WhenAll(
            navigationContent.TranslateTo(-250, 0, 200, Easing.Linear),
        closeNavigation.FadeTo(0d, 150, Easing.Linear)
            );
        navigationFlyout.IsVisible = false;
    }

    private void CloseFlyouts(object sender, TappedEventArgs e)
    {
        CloseProfileFlyout_Clicked(null, new());
        CloseNavigationFlyout_Clicked(null, new());
    }
    private async void OpenSearchEntry(object sender, EventArgs e)
    {
        searchEntry.IsEnabled = true;
        await searchEntry.ScaleXTo(1, 250, Easing.Linear);
        searchEntry.Focus();
    }

    private async void closeSearch(object sender, TappedEventArgs e)
    {
        await searchEntry.HideKeyboardAsync();
        await searchEntry.ScaleXTo(0, 250, Easing.Linear);
        searchEntry.IsEnabled = false;
        searchEntry.Unfocus();
    }
    private async void searchEntry_Completed(object sender, EventArgs e)
    {
        await searchEntry.HideKeyboardAsync();
        await searchEntry.ScaleXTo(0, 250, Easing.Linear);
        searchEntry.IsEnabled = false;
        searchEntry.Unfocus();
    }
}