using DbLibrary.Entities;
namespace AuthorLM.Client.Services
{
    public class AccountService
    {
        public const string USER_ID_TAG = "userID";
        private User? _user;
        public User? User
        {
            get => _user;
            private set => _user = value;
        }
        private readonly ApiCallService _callService;
        public AccountService(ApiCallService api)
        {
            _callService = api;
            if (IsLoggedIn)
                User = api.GetUserById(Convert.ToInt32(Preferences.Get(USER_ID_TAG, "0"))).Result;
            else User = null;
        }
        public bool IsLoggedIn { get => Preferences.ContainsKey(USER_ID_TAG); }
        public async void LogIn(int userId)
        {
            Preferences.Set(USER_ID_TAG, userId);
            User = await _callService.GetUserById(Convert.ToInt32(Preferences.Get(USER_ID_TAG, "0")));
        }
        public void LogOut()
        {
            Preferences.Remove(USER_ID_TAG);
            User = null;
        }
    }
}
