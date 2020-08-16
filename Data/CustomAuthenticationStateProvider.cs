using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Tatneft.Data
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private ILocalStorageService _localStorageService { get; }
        public CustomAuthenticationStateProvider(ILocalStorageService localStorageService)
        {
            //throw new Exception("CustomAuthenticationStateProviderException");
            _localStorageService = localStorageService;
        }
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var accessToken = await _localStorageService.GetItemAsync<string>("accessToken");

            ClaimsIdentity identity;

            if (accessToken != null && accessToken != string.Empty)
            {
                User user = new User{
                    Email = "qqweqe@mail.ru",
                    Role = "ssss"
                };
                identity = GetClaimsIdentity(user);
            }
            else
            {
                identity = new ClaimsIdentity();
            }

            var claimsPrincipal = new ClaimsPrincipal(identity);

            return await Task.FromResult(new AuthenticationState(claimsPrincipal));
        }

        //Помечает пользователя как авторизованного
        public void  MarkUserAsAuthenticated(User user)
        {
            
            //_localStorageService.SetItemAsync("Email", user.Email);
            _localStorageService.SetItemAsync("Token", user.Token);
            
            var identity = GetClaimsIdentity(user);

            var claimsPrincipal = new ClaimsPrincipal(identity);

            //Уведомляет все конструкции авторизированного доступа об изменении авторизации пользователя
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
          
        }
        
        public async Task MarkUserAsLoggedOut()
        {
            var identity = new ClaimsIdentity();
            var claimsPrincipal = new ClaimsPrincipal(identity);

            //Уведомляет все конструкции авторизированного доступа об изменении авторизации пользователя
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
        }
        public string getLocalToken()
        {
            return _localStorageService.GetItemAsync<string>("Token").ToString();
        }
        private ClaimsIdentity GetClaimsIdentity(User user)
        {
            var claimsIdentity = new ClaimsIdentity();

            if (user.Email != null)
            {
                claimsIdentity = new ClaimsIdentity(new[]
                                {
                                    new Claim(ClaimTypes.Name, user.Email),
                                    new Claim(ClaimTypes.Role, user.Role)
                                }, "apiauth_type");
            }

            return claimsIdentity;
        }
    }
}
