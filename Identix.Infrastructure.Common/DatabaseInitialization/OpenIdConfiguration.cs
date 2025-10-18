using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using Identix.Application.Abstractions.Extensions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Identix.Infrastructure.Common.DatabaseInitialization;

/// <summary>
/// Класс для настройки начальных данных OpenIddict (приложения и области видимости).
/// </summary>
internal static class OpenIdConfiguration
{
    /// <summary>
    /// Настраивает начальные данные OpenIddict в базе данных.
    /// Создает клиентские приложения и области видимости, если они отсутствуют.
    /// </summary>
    /// <param name="provider">Провайдер служб для получения менеджеров OpenIddict.</param>
    /// <returns>Задача, представляющая асинхронную операцию настройки.</returns>
    public static async Task ConfigureAsync(IServiceProvider provider)
    {
        var applicationManager = provider.GetRequiredService<IOpenIddictApplicationManager>();
        var scopeManager = provider.GetRequiredService<IOpenIddictScopeManager>();

        await SeedApplicationsAsync(applicationManager);
        await SeedScopesAsync(scopeManager);
    }

    /// <summary>
    /// Создает клиентские приложения, если они отсутствуют в базе данных.
    /// </summary>
    /// <param name="manager">Менеджер приложений OpenIddict.</param>
    private static async Task SeedApplicationsAsync(IOpenIddictApplicationManager manager)
    {
        await CreateOveroomApplicationAsync(manager);
        await CreateSwaggerApplicationAsync(manager);
    }

    /// <summary>
    /// Создает SPA клиент Overoom с настройками для авторизации.
    /// </summary>
    /// <param name="manager">Менеджер приложений OpenIddict.</param>
    private static async Task CreateOveroomApplicationAsync(IOpenIddictApplicationManager manager)
    {
        if (await manager.FindByClientIdAsync("overoom") is not null) return;

        var overoom = new OpenIddictApplicationDescriptor
        {
            ClientId = "overoom",
            DisplayName = "Overoom",
            ConsentType = ConsentTypes.Explicit,
            RedirectUris =
            {
                new Uri("https://localhost:5173/signin-oidc"),
                new Uri("https://localhost:5173/signin-silent-oidc"),
                new Uri("https://overoom.ru/signin-oidc"),
                new Uri("https://overoom.ru/signin-silent-oidc")
            },
            PostLogoutRedirectUris =
            {
                new Uri("https://localhost:5173/signout-oidc"),
                new Uri("https://overoom.ru/signout-oidc")
            },
            Permissions =
            {
                Permissions.Endpoints.Authorization,
                Permissions.Endpoints.Token,
                Permissions.Endpoints.EndSession,
                Permissions.Endpoints.Revocation,
                Permissions.GrantTypes.AuthorizationCode,
                Permissions.GrantTypes.RefreshToken,
                Permissions.ResponseTypes.Code,
                Permissions.Scopes.Email,
                Permissions.Scopes.Profile,
                Permissions.Scopes.Roles,
                Permissions.Prefixes.Scope + "films",
                Permissions.Prefixes.Scope + "rooms",
                Permissions.Prefixes.Scope + "uploader"
            },
            Requirements =
            {
                Requirements.Features.ProofKeyForCodeExchange
            }
        };

        await manager.CreateAsync(overoom);
    }

    /// <summary>
    /// Создает клиент Swagger UI для тестирования API.
    /// </summary>
    /// <param name="manager">Менеджер приложений OpenIddict.</param>
    private static async Task CreateSwaggerApplicationAsync(IOpenIddictApplicationManager manager)
    {
        if (await manager.FindByClientIdAsync("swagger") is not null) return;

        var swagger = new OpenIddictApplicationDescriptor
        {
            ClientId = "swagger",
            DisplayName = "SwaggerUI",
            RedirectUris =
            {
                new Uri("https://localhost:7131/swagger/oauth2-redirect.html"),
                new Uri("https://films.overoom.ru/swagger/oauth2-redirect.html"),
            },
            Permissions =
            {
                Permissions.Endpoints.Authorization,
                Permissions.Endpoints.Token,
                Permissions.Endpoints.EndSession,
                Permissions.Endpoints.Revocation,
                Permissions.GrantTypes.AuthorizationCode,
                Permissions.ResponseTypes.Code,
                Permissions.Prefixes.Scope + "films",
                Permissions.Prefixes.Scope + "uploader"
            },
            Requirements = { Requirements.Features.ProofKeyForCodeExchange }
        };

        swagger.SetLogoUri("/img/swagger.png");

        await manager.CreateAsync(swagger);
    }

    /// <summary>
    /// Создает области видимости (scopes) для приложения.
    /// </summary>
    /// <param name="scopeManager">Менеджер областей видимости OpenIddict.</param>
    private static async Task SeedScopesAsync(IOpenIddictScopeManager scopeManager)
    {
        var scopes = GetScopeDefinitions();

        foreach (var scope in scopes)
        {
            if (await scopeManager.FindByNameAsync(scope.Name) is not null) continue;

            await CreateScopeAsync(scopeManager, scope);
        }
    }

    /// <summary>
    /// Определяет набор областей видимости для приложения.
    /// </summary>
    /// <returns>Коллекция определений областей видимости.</returns>
    private static IEnumerable<dynamic> GetScopeDefinitions()
    {
        return
        [
            // Кастомные области видимости для API
            new
            {
                Name = "films",
                DisplayNames = new Dictionary<CultureInfo, string>
                {
                    [new CultureInfo("ru")] = "Фильмы",
                    [new CultureInfo("en")] = "Films"
                },
                Descriptions = new Dictionary<CultureInfo, string>
                {
                    [new CultureInfo("ru")] = "Доступ к API фильмов",
                    [new CultureInfo("en")] = "Access to films API"
                },
                Emphasize = false,
                Required = false,
                Resources = new List<string> { "films" }
            },
            new
            {
                Name = "rooms",
                DisplayNames = new Dictionary<CultureInfo, string>
                {
                    [new CultureInfo("ru")] = "Комнаты",
                    [new CultureInfo("en")] = "Rooms"
                },
                Descriptions = new Dictionary<CultureInfo, string>
                {
                    [new CultureInfo("ru")] = "Доступ к API комнат",
                    [new CultureInfo("en")] = "Access to rooms API"
                },
                Emphasize = false,
                Required = false,
                Resources = new List<string> { "rooms" }
            },
            new
            {
                Name = "uploader",
                DisplayNames = new Dictionary<CultureInfo, string>
                {
                    [new CultureInfo("ru")] = "Загрузчик",
                    [new CultureInfo("en")] = "Uploader"
                },
                Descriptions = new Dictionary<CultureInfo, string>
                {
                    [new CultureInfo("ru")] = "Доступ к API сервиса загрузки",
                    [new CultureInfo("en")] = "Access to uploader service API"
                },
                Emphasize = false,
                Required = false,
                Resources = new List<string> { "uploader" }
            },

            // Стандартные области видимости OpenIddict
            new
            {
                Name = Scopes.OpenId,
                DisplayNames = new Dictionary<CultureInfo, string>
                {
                    [new CultureInfo("ru")] = "Ваш идентификатор",
                    [new CultureInfo("en")] = "Your ID"
                },
                Descriptions = new Dictionary<CultureInfo, string>
                {
                    [new CultureInfo("ru")] = "Доступ к идентификатору",
                    [new CultureInfo("en")] = "Access to the ID"
                },
                Emphasize = true,
                Required = true,
                Resources = new List<string>()
            },
            new
            {
                Name = Scopes.Profile,
                DisplayNames = new Dictionary<CultureInfo, string>
                {
                    [new CultureInfo("ru")] = "Профиль",
                    [new CultureInfo("en")] = "Profile"
                },
                Descriptions = new Dictionary<CultureInfo, string>
                {
                    [new CultureInfo("ru")] = "Доступ к данным профиля",
                    [new CultureInfo("en")] = "Access to profile data"
                },
                Emphasize = true,
                Required = false,
                Resources = new List<string>()
            },
            new
            {
                Name = Scopes.Email,
                DisplayNames = new Dictionary<CultureInfo, string>
                {
                    [new CultureInfo("ru")] = "Email",
                    [new CultureInfo("en")] = "Email"
                },
                Descriptions = new Dictionary<CultureInfo, string>
                {
                    [new CultureInfo("ru")] = "Доступ к email",
                    [new CultureInfo("en")] = "Access to email"
                },
                Emphasize = true,
                Required = false,
                Resources = new List<string>()
            },
            new
            {
                Name = Scopes.Roles,
                DisplayNames = new Dictionary<CultureInfo, string>
                {
                    [new CultureInfo("ru")] = "Роли",
                    [new CultureInfo("en")] = "Roles"
                },
                Descriptions = new Dictionary<CultureInfo, string>
                {
                    [new CultureInfo("ru")] = "Доступ к ролям",
                    [new CultureInfo("en")] = "Access to roles"
                },
                Emphasize = true,
                Required = false,
                Resources = new List<string>()
            },
            new
            {
                Name = Scopes.OfflineAccess,
                DisplayNames = new Dictionary<CultureInfo, string>
                {
                    [new CultureInfo("ru")] = "Оффлайн доступ",
                    [new CultureInfo("en")] = "Offline Access"
                },
                Descriptions = new Dictionary<CultureInfo, string>
                {
                    [new CultureInfo("ru")] = "Доступ к приложениям и ресурсам в оффлайн-режиме",
                    [new CultureInfo("en")] = "Access to your applications and resources when you are offline"
                },
                Emphasize = true,
                Required = false,
                Resources = new List<string>()
            }
        ];
    }

    /// <summary>
    /// Создает область видимости на основе определения.
    /// </summary>
    /// <param name="scopeManager">Менеджер областей видимости.</param>
    /// <param name="scope">Определение области видимости.</param>
    private static async Task CreateScopeAsync(IOpenIddictScopeManager scopeManager, dynamic scope)
    {
        var descriptor = new OpenIddictScopeDescriptor
        {
            Name = scope.Name
        };

        foreach (var resource in scope.Resources)
        {
            descriptor.Resources.Add(resource);
        }

        // Добавление локализованных отображаемых имен
        foreach (var dn in scope.DisplayNames)
            descriptor.DisplayNames.Add(dn.Key, dn.Value);

        // Добавление локализованных описаний
        foreach (var desc in scope.Descriptions)
            descriptor.Descriptions.Add(desc.Key, desc.Value);

        // Настройка дополнительных свойств
        if (scope.Emphasize)
            descriptor.SetEmphasize(true);

        if (scope.Required)
            descriptor.SetRequired(true);

        await scopeManager.CreateAsync(descriptor);
    }
}