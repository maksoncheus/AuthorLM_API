using DbLibrary.Entities;
namespace AuthorLM.Client.Services
{
    public class AccountService
    {
        public const string USER_ID_TAG = "userID";
        public const string USER_TOKEN_TAG = "token";
        public bool IsLoggedIn { get => Preferences.ContainsKey(USER_TOKEN_TAG); }
        public string? Token
        {
            get => IsLoggedIn ? Preferences.Get(USER_TOKEN_TAG, null) : null;
            
        }
        public async void LogIn( string token)
        {
            Preferences.Set(USER_TOKEN_TAG, token);
        }
        public void LogOut()
        {
            Preferences.Remove(USER_TOKEN_TAG);
        }
    }
}
