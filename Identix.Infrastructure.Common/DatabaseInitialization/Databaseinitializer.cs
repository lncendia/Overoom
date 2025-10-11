using System;
using System.Threading.Tasks;

namespace Identix.Infrastructure.Common.DatabaseInitialization;

/// <summary>
/// Класс для инициализации начальных данных в базу данных сервиса аутентификации.
/// Содержит методы для настройки индексов и конфигурации MongoDB.
/// </summary>
public static class DatabaseInitializer
{
    /// <summary>
    /// Инициализирует начальные данные в базу данных.
    /// Выполняет настройку индексов и конфигурации для Identity и OpenId модулей.
    /// </summary>
    /// <param name="serviceProvider">Провайдер сервисов для создания области видимости.</param>
    /// <returns>Задача, представляющая асинхронную операцию инициализации.</returns>
    public static async Task InitAsync(IServiceProvider serviceProvider)
    {
        // Настройка индексов MongoDB для Identity модуля
        await IdentityMongoIndexCreator.ConfigureAsync(serviceProvider);
        
        // Конфигурация начальных данных для Identity модуля
        await IdentityConfiguration.ConfigureAsync(serviceProvider);
        
        // Настройка индексов MongoDB для OpenId модуля
        await OpenIdMongoIndexCreator.ConfigureAsync(serviceProvider);
        
        // Конфигурация начальных данных для OpenId модуля
        await OpenIdConfiguration.ConfigureAsync(serviceProvider);
    }
}