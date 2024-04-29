using AuthorLM.Client.ViewModels;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Views;

namespace AuthorLM.Client.Views;

public partial class ChangePasswordPopup : Popup
{
	private ChangePasswordViewModel _viewModel;
	public ChangePasswordPopup(ChangePasswordViewModel vm)
	{
		InitializeComponent();
		BindingContext = _viewModel = vm;
	}

    private async void Change_Clicked(object sender, EventArgs e)
    {
		if(_viewModel.NewPassword != _viewModel.ConfirmPassword)
		{
			await Toast.Make("Пароли не совпадают").Show();
			return;
		}
		HttpResponseMessage msg = await _viewModel.ChangePassword(_viewModel.OldPassword, _viewModel.NewPassword);
		await Toast.Make(await msg.Content.ReadAsStringAsync()).Show();
		if(msg.IsSuccessStatusCode)
			Close();
    }
}