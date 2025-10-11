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
        // Получаем конфигурацию для шаблона письма конфигурации приложения
        var templateConfiguration = GetTemplateConfiguration(builder.Configuration);

        // Регистрируем экземпляр интерфейса IEmailVisitor с реализацией EmailContentVisitor.
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
        // Получаем секцию с настройками шаблона письма
        var templateConfigurationSection = configuration.GetSection("Email:TemplateSettings");

        // Если секции конфигурации нет - вызываем исключение
        if (templateConfigurationSection == null) throw new ConfigurationException("Email:TemplateSettings");

        // Получаем данные из конфигурации
        var companyName = templateConfigurationSection.GetRequiredValue<string>("CompanyName");
        var logoLink = templateConfigurationSection.GetRequiredValue<string>("LogoLink");
        var privatePolicyLink = templateConfigurationSection.GetRequiredValue<string>("PrivatePolicyLink");
        var homePageLink = templateConfigurationSection.GetRequiredValue<string>("HomePageLink");
        var sideLogoLink = templateConfigurationSection.GetRequiredValue<string>("SideLogoLink");

        // создаем объект данных об настройках шаблона письма
        return new EmailTemplateConfiguration
        {
            // Имя компании
            CompanyName = companyName,

            // Ссылка на лого компании
            LogoLink = logoLink,

            // Ссылка на страницу приватной политики
            PrivatePolicyLink = privatePolicyLink,

            // Ссылка на домашнюю страницу
            HomePageLink = homePageLink,

            // Ссылка на вторичное лого компании
            SideLogoLink = sideLogoLink
        };
    }
}