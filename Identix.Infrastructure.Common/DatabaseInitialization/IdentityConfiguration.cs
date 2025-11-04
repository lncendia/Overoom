using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Identix.Application.Abstractions.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver.Linq;

namespace Identix.Infrastructure.Common.DatabaseInitialization;

/// <summary>
/// Класс для инициализации начальных данных в базу данных.
/// </summary>
internal static class IdentityConfiguration
{
    /// <summary>
    /// Инициализация начальных данных в базу данных.
    /// </summary>
    /// <param name="scopeServiceProvider">Провайдер служб для извлечения необходимых сервисов.</param>
    /// <param name="configuration">Конфигурация приложения</param>
    public static async Task ConfigureAsync(IServiceProvider scopeServiceProvider, IConfiguration configuration)
    {
        // Получение необходимых сервисов из контейнера зависимостей
        var userManager = scopeServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scopeServiceProvider.GetRequiredService<RoleManager<AppRole>>();
        var loggerFactory = scopeServiceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("IdentityConfiguration");

        // Получение учетных данных администратора из конфигурации
        var email = configuration.GetValue<string>("Identity:InitAdministratorEmail");
        var password = configuration.GetValue<string>("Identity:InitAdministratorPassword");

        // Проверка конфигурации email администратора
        if (email == null)
            logger.LogWarning("Initial administrator email is not configured");

        // Проверка конфигурации пароля администратора
        if (password == null)
            logger.LogWarning("Initial administrator password is not configured");

        // Получаем текущую дату и время в UTC
        var now = DateTime.UtcNow;

        // Проверяем, существует ли роль "admin". Если нет, создаем новую роль.
        if (await roleManager.FindByNameAsync("admin") == null)
        {
            await roleManager.CreateAsync(new AppRole
            {
                Id = Guid.NewGuid(),
                Name = "admin",
                Description = "Administrator"
            });
        }

        // Проверка наличия пользователей в системе
        if (await userManager.Users.AnyAsync())
        {
            logger.LogInformation("Users already exist in the system. Administrator initialization will be skipped");
            return;
        }

        // Если учетные данные администратора не настроены, прерываем выполнение
        if (email == null || password == null)
        {
            logger.LogWarning("Administrator credentials are not fully configured. Administrator initialization will be skipped");
            return;
        }

        // Создаем нового пользователя с ролью администратора
        var admin = new AppUser
        {
            Id = Guid.NewGuid(),
            RegistrationTimeUtc = now,
            LastAuthTimeUtc = now,
            Email = email,
            UserName = email.Split('@')[0],
            EmailConfirmed = true
        };

        // Создаем пользователя с паролем
        var result = await userManager.CreateAsync(admin, password);

        // Если пользователь успешно создан, добавляем его в роль "admin"
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "admin");
            logger.LogInformation("Administrator user created successfully and assigned to admin role");
        }
        else
        {
            foreach (var error in result.Errors)
            {
                logger.LogWarning("Failed to create administrator user. {Description}", error.Description);
            }
        }
    }
}
