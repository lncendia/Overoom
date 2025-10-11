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
        // Получаем конфигурацию для smtp из конфигурации приложения
        var smtpConfiguration = GetEmailConfiguration(builder.Configuration);

        // Регистрируем одиночный экземпляр интерфейса IEmailService с реализацией EmailService.
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
        // Получаем секцию с настройками SMTP
        var smtpConfigurationSection = configuration.GetSection("Email:SmtpSettings");

        // Если секции конфигурации нет - вызываем исключение
        if (smtpConfigurationSection == null) throw new ConfigurationException("Email:SmtpSettings");

        // Получаем данные из конфигурации
        var smtpHost = smtpConfigurationSection.GetRequiredValue<string>("Host");
        var smtpPort = smtpConfigurationSection.GetRequiredValue<int>("Port");
        var smtpLogin = smtpConfigurationSection.GetRequiredValue<string>("Email");
        var smtpPassword = smtpConfigurationSection.GetRequiredValue<string>("Password");
        var displayedName = smtpConfigurationSection.GetRequiredValue<string>("DisplayedName");

        // создаем объект данных об SMTP настройках
        return new SmtpConfiguration
        {
            // Хост
            Host = smtpHost,

            // Порт
            Port = smtpPort,

            // Логин
            Login = smtpLogin,

            // Пароль
            Password = smtpPassword,

            // Отображаемое имя
            DisplayedName = displayedName
        };
    }
}