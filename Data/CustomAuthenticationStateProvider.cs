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
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var identity=new ClaimsIdentity(new[]
            {
            new Claim(ClaimTypes.Name, "ruslan@mail.ru"),
            } , "apiauth_type");
            var user = new ClaimsPrincipal(identity);

            return Task.FromResult(new AuthenticationState(user));
        }

        //Помечает пользователя как авторизованного
        public void MarkUserAsAuthenticated(string email)
        {
            var identity = new ClaimsIdentity(new[] 
            {
            new Claim(ClaimTypes.Name, "ruslan@mail.ru"),
            }, "apiauth_type");

            var user = new ClaimsPrincipal(identity);
            //Уведомляет все конструкции авторизированного доступа об изменении авторизации пользователя
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }
        public async Task MarkUserAsLoggedOut()
        {
           // await _localStorageService.RemoveItemAsync("refreshToken");
           // await _localStorageService.RemoveItemAsync("accessToken");

            var identity = new ClaimsIdentity();

            var user = new ClaimsPrincipal(identity);

            //Уведомляет все конструкции авторизированного доступа об изменении авторизации пользователя
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }
    }
}
