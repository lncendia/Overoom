using Common.DI.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;

namespace Common.DI.WebApi.Extensions;

/// <summary>
/// Статический класс, предоставляющий метод расширения для добавления авторизации по JWT в коллекцию сервисов.
/// </summary>
public static class JwtAuthentication
{
  /// <summary>
  /// Добавляет авторизацию по JWT в коллекцию сервисов.
  /// </summary>
  /// <param name="builder">Построитель веб-приложений и сервисов.</param>
  public static void AddJwtAuthentication(this IHostApplicationBuilder builder)
  {
    IConfigurationSection section = builder.Configuration.GetSection("Authentication");
    string authority = section.GetRequiredValue<string>("Authority");
    string issuer = section.GetRequiredValue<string>("Issuer");
    string audience = section.GetRequiredValue<string>("Audience");
    bool insecureConnection = section.GetValue<bool>("InsecureConnection");
    string[] signalRPaths = section.GetSection("SignalR").Get<string[]>() ?? [];

    builder.Services.AddAuthentication(options =>
      {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
      })
      .AddJwtBearer(options =>
      {
        if (insecureConnection)
        {
          options.BackchannelHttpHandler = new HttpClientHandler
          {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
          };
        }

        options.RequireHttpsMetadata = !insecureConnection;
        options.Authority = authority;
        options.Audience = audience;
        options.TokenValidationParameters = new TokenValidationParameters
        {
          ValidIssuer = issuer,
          ValidateIssuerSigningKey = true,
          ValidateIssuer = true,
          ValidateAudience = true,
          ValidateLifetime = true
        };
        options.Events = new JwtBearerEvents
        {
          OnMessageReceived = context =>
          {
            StringValues accessToken = context.Request.Query["access_token"];
            PathString path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && signalRPaths.Any(p => path.StartsWithSegments(p)))
            {
              context.Token = accessToken;
            }

            return Task.CompletedTask;
          }
        };
      });
  }
}