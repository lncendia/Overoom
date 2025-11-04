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
                    // Если true, новые пользователи могут быть заблокированы после превышения лимита неудачных попыток входа.
                    identityOptions.Lockout.AllowedForNewUsers = true;

                    // Задает временной интервал блокировки по умолчанию в 15 минут.
                    // Пользователь будет заблокирован на этот период после превышения лимита неудачных попыток входа.
                    identityOptions.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

                    // Устанавливает максимальное количество неудачных попыток входа перед блокировкой.
                    // После 10 неудачных попыток учетная запись пользователя будет заблокирована.
                    identityOptions.Lockout.MaxFailedAccessAttempts = 10;
                },
                mongoOptions =>
                {
                    // Устанавливает имя базы в MongoDB.
                    mongoOptions.DatabaseName = database;
                }
            )

            // Добавляет поставщиков токенов по умолчанию, используемых для создания токенов для сброса паролей,
            // операций изменения электронной почты и номера телефона, а также для создания токенов двухфакторной аутентификации.
            .AddDefaultTokenProviders();

        // Добавляет службы политики авторизации в указанную коллекцию IServiceCollection.
        builder.Services.AddAuthorization();
    }
}