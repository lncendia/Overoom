using Common.Application.EmailService;
using Common.DI.Exceptions;
using Common.DI.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Identix.Infrastructure.Common.Emails;

namespace Identix.Extensions;

/// <summary>
/// Статический класс, содержащий методы для работы с электронной почтой.
/// </summary>
public static class EmailTemplates
{
  /// <summary>
  /// Метод для добавления службы электронной почты в коллекцию служб.
  /// </summary>
  /// <param name="builder">Построитель веб-приложения.</param>
  public static void AddEmailTemplates(this IHostApplicationBuilder builder)
  {
    EmailTemplateConfiguration templateConfiguration = GetTemplateConfiguration(builder.Configuration);

    builder.Services.AddScoped<IEmailVisitor>(sp =>
      new EmailContentVisitor(templateConfiguration,
        sp.GetRequiredService<IStringLocalizer<EmailContentVisitor>>()));
  }

  /// <summary>
  /// Метод генерирует настройки для шаблона электронного письма
  /// </summary>
  /// <param name="configuration">Конфигурация приложения</param>
  /// <returns>Настройки для шаблона электронного письма</returns>
  private static EmailTemplateConfiguration GetTemplateConfiguration(IConfiguration configuration)
  {
    IConfigurationSection? templateConfigurationSection = configuration.GetSection("Email:TemplateSettings");
    if (templateConfigurationSection == null) throw new ConfigurationException("Email:TemplateSettings");

    string companyName = templateConfigurationSection.GetRequiredValue<string>("CompanyName");
    string logoLink = templateConfigurationSection.GetRequiredValue<string>("LogoLink");
    string privatePolicyLink = templateConfigurationSection.GetRequiredValue<string>("PrivatePolicyLink");
    string homePageLink = templateConfigurationSection.GetRequiredValue<string>("HomePageLink");
    string sideLogoLink = templateConfigurationSection.GetRequiredValue<string>("SideLogoLink");

    return new EmailTemplateConfiguration
    {
      CompanyName = companyName,

      LogoLink = logoLink,

      PrivatePolicyLink = privatePolicyLink,

      HomePageLink = homePageLink,

      SideLogoLink = sideLogoLink
    };
  }
}
