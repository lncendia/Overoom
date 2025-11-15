using System;
using Common.DI.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Identix.Application.Abstractions.Entities;
using Identix.Application.Services.Validators;
using Identix.Infrastructure.Web.Account.Services;

namespace Identix.Extensions;

/// <summary>
/// Статический класс, представляющий методы для добавления ASP.NET Identity.
/// </summary>
public static class AspIdentity
{
    /// <summary>
    /// Добавляет ASP.NET Identity в коллекцию сервисов.
    /// </summary>
    /// <param name="builder">Построитель веб-приложения.</param>
    public static void AddAspIdentity(this IHostApplicationBuilder builder)
    {
        // Извлекаем имя базы данных из конфигурации.
        var database = builder.Configuration.GetRequiredValue<string>("MongoDB:IdentityDB");

        // Добавляет валидатор для пользователя.
        builder.Services.AddTransient<IUserValidator<AppUser>, CustomUserValidator>();

        // Добавляет валидатор для пароля.
        builder.Services.AddTransient<IPasswordValidator<AppUser>, CustomPasswordValidator>();
        
        // Добавляет SignInManager
        builder.Services.AddScoped<SignInManager<AppUser>, OpenIdSignInManager<AppUser>>();

        // Добавляет и настраивает идентификационную систему для указанных пользователей и типов ролей.
        builder.Services.AddIdentityMongoDbProviderWithOutbox<AppUser, AppRole, Guid>
        (
            identityOptions =>
            {
                // Разрешает применение механизма блокировки для новых пользователей.
                identityOptions.Lockout.AllowedForNewUsers = true;

                // Задает временной интервал блокировки по умолчанию в 15 минут.
                identityOptions.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

                // Устанавливает максимальное количество неудачных попыток входа перед блокировкой.
                identityOptions.Lockout.MaxFailedAccessAttempts = 10;

                // Устанавливает необходимость подтвержденной электронной почты для входа.
                identityOptions.SignIn.RequireConfirmedEmail = true;
            },
            mongoOptions =>
            {
                // Устанавливает имя базы в MongoDB.
                mongoOptions.DatabaseName = database;
            }
        );

        // Добавляет службы политики авторизации в указанную коллекцию IServiceCollection.
        builder.Services.AddAuthorization();
    }
}