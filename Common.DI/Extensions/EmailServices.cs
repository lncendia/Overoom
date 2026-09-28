using Common.Application.EmailService;
using Common.DI.Exceptions;
using Common.Infrastructure.EmailService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Common.DI.Extensions;

/// <summary>
/// Статический класс, содержащий методы для работы с электронной почтой.
/// </summary>
public static class EmailServices
{
  /// <summary>
  /// Метод для добавления службы электронной почты в коллекцию служб.
  /// </summary>
  /// <param name="builder">Построитель веб-приложения.</param>
  public static void AddEmailService(this IHostApplicationBuilder builder)
  {
    SmtpConfiguration smtpConfiguration = GetEmailConfiguration(builder.Configuration);

    builder.Services.AddScoped<IEmailService>(sp =>
      new EmailService(smtpConfiguration, sp.GetRequiredService<IEmailVisitor>()));
  }

  /// <summary>
  /// Метод генерирует SMTP настройки системных Email
  /// </summary>
  /// <param name="configuration">Конфигурация приложения</param>
  /// <returns>Коллекция SMTP настроек системных Email</returns>
  private static SmtpConfiguration GetEmailConfiguration(IConfiguration configuration)
  {
    IConfigurationSection? smtpConfigurationSection = configuration.GetSection("Email:SmtpSettings");
    if (smtpConfigurationSection == null) throw new ConfigurationException("Email:SmtpSettings");

    string smtpHost = smtpConfigurationSection.GetRequiredValue<string>("Host");
    int smtpPort = smtpConfigurationSection.GetRequiredValue<int>("Port");
    string smtpLogin = smtpConfigurationSection.GetRequiredValue<string>("Email");
    string smtpPassword = smtpConfigurationSection.GetRequiredValue<string>("Password");
    string displayedName = smtpConfigurationSection.GetRequiredValue<string>("DisplayedName");

    return new SmtpConfiguration
    {
      Host = smtpHost,

      Port = smtpPort,

      Login = smtpLogin,

      Password = smtpPassword,

      DisplayedName = displayedName
    };
  }
}
