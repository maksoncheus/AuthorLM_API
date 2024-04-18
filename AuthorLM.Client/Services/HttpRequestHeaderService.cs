using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AuthorLM.Client.Services
{
    public class HttpRequestHeaderService
    {
        private readonly AccountService _accountService;
        public HttpRequestHeaderService(AccountService accountService)
        {
            _accountService = accountService;
        }
        public void AddAuthorizationHeader(HttpRequestMessage request)
        {
            if(request == null) throw new ArgumentNullException(nameof(request));
            if(_accountService.IsLoggedIn)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Preferences.Get(AccountService.USER_TOKEN_TAG, ""));
            }
        }
    }
}
