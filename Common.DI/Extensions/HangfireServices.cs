using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Microsoft.Extensions.Hosting;

namespace Common.DI.Extensions;

/// <summary>
/// Статический класс для сервисов Hangfire
/// </summary>
public static class HangfireServices
{
    /// <summary>
    ///  Расширяющий метод для добавления сервисов Hangfire в коллекцию служб.
    /// </summary>
    /// <param name="builder">Построитель веб-приложения.</param>
    /// <param name="queueName">Имя очереди асинхронных задач.</param>
    public static void AddHangfireServices(this IHostApplicationBuilder builder, string queueName)
    {
        // Извлекаем имя базы данных для приложения из конфигурации.
        var applicationDatabaseName = builder.Configuration.GetRequiredValue<string>("MongoDB:HangfireDB");

        // Формируем настройки базы данных Mongo
        var mongoStorageOptions = new MongoStorageOptions
        {
            // Формируем настройки миграции
            MigrationOptions = new MongoMigrationOptions
            {
                // Настраиваем стратегию миграции
                MigrationStrategy = new MigrateMongoMigrationStrategy()
            }
        };

        // Настраиваем сервис Hangfire
        builder.Services.AddHangfire(options =>
        {
            // Настраиваем подключение базы данных
            options.UseMongoStorage(MongoDbProvider.Client, applicationDatabaseName, mongoStorageOptions);
        });

        // Регистрируем сервер Hangfire
        builder.Services.AddHangfireServer(options => { options.Queues = [queueName]; });
    }
}