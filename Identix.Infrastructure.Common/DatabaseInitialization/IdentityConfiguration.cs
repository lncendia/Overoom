using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Identix.Application.Abstractions.Entities;

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
    public static async Task ConfigureAsync(IServiceProvider scopeServiceProvider)
    {
        // Получаем сервис UserManager<AppUser> для управления пользователями
        var userManager = scopeServiceProvider.GetRequiredService<UserManager<AppUser>>();

        // Получаем сервис RoleManager<AppRole> для управления ролями
        var roleManager = scopeServiceProvider.GetRequiredService<RoleManager<AppRole>>();

        // Определение константной переменной для email администратора
        const string adminEmail = "admin@gmail.com";

        // Получаем текущую дату и время в UTC
        var now = DateTime.UtcNow;

        // Проверяем, существует ли роль "admin". Если нет, создаем новую роль.
        if (await roleManager.FindByNameAsync("admin") == null)
        {
            // Создаем новую роль "admin"
            await roleManager.CreateAsync(new AppRole
            {
                Id = Guid.NewGuid(), // Генерация уникального идентификатора
                Name = "admin", // Название роли
                Description = "Administrator" // Описание роли
            });
        }
        
        // Проверяем, существует ли роль "superadmin". Если нет, создаем новую роль.
        if (await roleManager.FindByNameAsync("superadmin") == null)
        {
            // Создаем новую роль "superadmin"
            await roleManager.CreateAsync(new AppRole
            {
                Id = Guid.NewGuid(), // Генерация уникального идентификатора
                Name = "superadmin", // Название роли
                Description = "Super administrator" // Описание роли
            });
        }

        // Проверяем, существует ли пользователь с email администратора
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            // Создаем нового пользователя с ролью администратора
            var admin = new AppUser
            {
                Id = Guid.NewGuid(), // Генерация уникального идентификатора
                RegistrationTimeUtc = now, // Время регистрации
                LastAuthTimeUtc = now, // Время последней авторизации
                Email = adminEmail, // Email администратора
                UserName = adminEmail.Split('@')[0], // Имя пользователя (часть email до @)
                EmailConfirmed = true // Подтверждение email
            };

            // Создаем пользователя с паролем
            var result = await userManager.CreateAsync(admin, "LuX995_WFB");

            // Если пользователь успешно создан
            if (result.Succeeded)
            {
                // Добавляем пользователя в роль "superadmin"
                await userManager.AddToRoleAsync(admin, "superadmin");
            }
        }
    }
}