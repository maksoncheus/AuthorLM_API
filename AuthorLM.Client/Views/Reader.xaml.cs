using AuthorLM.Client.ViewModels;
using Microsoft.Maui.Controls;
using Syncfusion.Maui.Core.Carousel;

namespace AuthorLM.Client.Views;

public partial class Reader : ContentPage
{
	private readonly ReaderViewModel _viewModel;
	public Reader(ReaderViewModel vm)
	{
		InitializeComponent();
		BindingContext = _viewModel = vm;
		_viewModel.NotifyScroll += Scroll;
	}
	private async void Scroll(double scroll)
	{
		await BookContent.ScrollToAsync(0, scroll, animated: true);
	}
    private void BookContent_Scrolled(object sender, ScrolledEventArgs e)
    {
		_viewModel.Scroll = e.ScrollY;
    }

    private void CarouselView_CurrentItemChanged(object sender, CurrentItemChangedEventArgs e)
    {
        var item = _viewModel.Fonts.FirstOrDefault(m => m == (string)e.CurrentItem);
        carousel.ScrollTo(item);
    }
    private async void OpenNavigationFlyout_Clicked(object sender, EventArgs e)
    {
        CloseTOCFlyout_Clicked(null, new());
        settings.Clicked -= OpenNavigationFlyout_Clicked;
        settings.Clicked += CloseNavigationFlyout_Clicked;
        navigationFlyout.IsVisible = true;
        navigationContent.CancelAnimations();
        await navigationContent.TranslateTo(0, 0, 250, Easing.Linear);
    }
    private async void CloseNavigationFlyout_Clicked(object sender, EventArgs e)
    {
        settings.Clicked += OpenNavigationFlyout_Clicked;
        settings.Clicked -= CloseNavigationFlyout_Clicked;
        navigationContent.CancelAnimations();
        await navigationContent.TranslateTo(0, 350, 250, Easing.Linear);
        navigationFlyout.IsVisible = false;
    }
    private async void OpenTOCFlyout_Clicked(object sender, EventArgs e)
    {
        CloseNavigationFlyout_Clicked(null, new());
        tableOfContents.Clicked -= OpenTOCFlyout_Clicked;
        tableOfContents.Clicked += CloseTOCFlyout_Clicked;
        tocFlyout.IsVisible = true;
        tocContent.CancelAnimations();
        await tocContent.TranslateTo(0, 0, 250, Easing.Linear);
    }
    private async void CloseTOCFlyout_Clicked(object sender, EventArgs e)
    {
        tableOfContents.Clicked += OpenTOCFlyout_Clicked;
        tableOfContents.Clicked -= CloseTOCFlyout_Clicked;
        tocContent.CancelAnimations();
        await tocContent.TranslateTo(0, 350, 250, Easing.Linear);
        tocFlyout.IsVisible = false;
    }
    private void CloseFlyouts(object sender, TappedEventArgs e)
    {
        CloseNavigationFlyout_Clicked(null, new());
    }

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        navigationFlyout.IsVisible = true;
        navigationContent.CancelAnimations();
        navigationContent.TranslationY = 350;
        navigationContent.TranslateTo(0, 0, 0, Easing.Linear);
    }
}