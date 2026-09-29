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
    UserManager<AppUser> userManager = scopeServiceProvider.GetRequiredService<UserManager<AppUser>>();
    RoleManager<AppRole> roleManager = scopeServiceProvider.GetRequiredService<RoleManager<AppRole>>();
    ILoggerFactory loggerFactory = scopeServiceProvider.GetRequiredService<ILoggerFactory>();
    ILogger logger = loggerFactory.CreateLogger("IdentityConfiguration");
    string? email = configuration.GetValue<string>("Identity:InitAdministratorEmail");
    string? password = configuration.GetValue<string>("Identity:InitAdministratorPassword");

    if (email == null)
      logger.LogWarning("Initial administrator email is not configured");

    if (password == null)
      logger.LogWarning("Initial administrator password is not configured");

    DateTime now = DateTime.UtcNow;

    if (await roleManager.FindByNameAsync("admin") == null)
    {
      await roleManager.CreateAsync(new AppRole
      {
        Name = "admin",
        Description = "Administrator"
      });
    }

    if (await userManager.Users.AnyAsync())
    {
      logger.LogInformation("Users already exist in the system. Administrator initialization will be skipped");
      return;
    }

    if (email == null || password == null)
    {
      logger.LogWarning(
        "Administrator credentials are not fully configured. Administrator initialization will be skipped");
      return;
    }

    var admin = new AppUser
    {
      RegistrationTimeUtc = now,
      LastAuthTimeUtc = now,
      Email = email,
      UserName = email.Split('@')[0],
      EmailConfirmed = true
    };

    IdentityResult result = await userManager.CreateAsync(admin, password);

    if (result.Succeeded)
    {
      await userManager.AddToRoleAsync(admin, "admin");
      logger.LogInformation("Administrator user created successfully and assigned to admin role");
    }
    else
    {
      foreach (IdentityError error in result.Errors)
      {
        logger.LogWarning("Failed to create administrator user. {Description}", error.Description);
      }
    }
  }
}
