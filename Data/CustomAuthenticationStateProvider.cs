﻿using Microsoft.AspNetCore.Components.Authorization;
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
           
            var identity = new ClaimsIdentity();

            var user = new ClaimsPrincipal(identity);

            return Task.FromResult(new AuthenticationState(user));
        }

        //Помечает пользователя как авторизованного
        public void MarkUserAsAuthenticated(User user)
        {
            user.ClaimsIdentity = new ClaimsIdentity(new[]
            {
            new Claim(ClaimTypes.Name, user.Email),
            }, "apiauth_type");
            user.ClaimsPrincipal = new ClaimsPrincipal(user.ClaimsIdentity);
            //Уведомляет все конструкции авторизированного доступа об изменении авторизации пользователя
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user.ClaimsPrincipal)));
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
