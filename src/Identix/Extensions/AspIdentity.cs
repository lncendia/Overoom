using System;

using Common.DI.Extensions;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Identix.Application.Abstractions.Entities;
using Identix.Application.Services.Validators;
using Identix.Infrastructure.Common.Identity.Stores;
using Identix.Infrastructure.Web.Account.Services;

using Incendia.Identity.Mongo;

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
    string database = builder.Configuration.GetRequiredValue<string>("MongoDB:IdentityDB");
    builder.Services.AddTransient<IUserValidator<AppUser>, CustomUserValidator>();
    builder.Services.AddTransient<IPasswordValidator<AppUser>, CustomPasswordValidator>();
    builder.Services.AddScoped<SignInManager<AppUser>, OpenIdSignInManager<AppUser>>();

    builder.Services.AddScoped<IUserStore<AppUser>, UserStore>();
    builder.Services.AddScoped<IRoleStore<AppRole>, RoleStore>();

    builder.Services.AddIdentity<AppUser, AppRole>(options =>
      {
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.MaxFailedAccessAttempts = 10;
        options.SignIn.RequireConfirmedEmail = true;
      })
      .AddMongoStores(options =>
      {
        options.CreateIndexes = true;
        options.Database = MongoDbProvider.Client.GetDatabase(database);
      })
      .AddDefaultTokenProviders();

    builder.Services.AddAuthorization();
  }
}
