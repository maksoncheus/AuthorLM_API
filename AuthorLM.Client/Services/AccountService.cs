using DbLibrary.Entities;
namespace AuthorLM.Client.Services
{
    public class AccountService
    {
        public const string USER_ID_TAG = "userID";
        public const string USER_TOKEN_TAG = "token";
        public const string USER_ADMIN_TAG = "isAdmin";
        public bool IsLoggedIn { get => Preferences.ContainsKey(USER_TOKEN_TAG); }
        public bool IsAdmin { get => Preferences.Get(USER_ADMIN_TAG, false); }
        public string? Token
        {
            get => IsLoggedIn ? Preferences.Get(USER_TOKEN_TAG, null) : null;
        }
        public void LogIn(string token, bool isAdmin)
        {
            Preferences.Set(USER_TOKEN_TAG, token);
            Preferences.Set(USER_ADMIN_TAG, isAdmin);
        }
        public void LogOut()
        {
            Preferences.Remove(USER_TOKEN_TAG);
            Preferences.Remove(USER_ADMIN_TAG);
        }
    }
}
