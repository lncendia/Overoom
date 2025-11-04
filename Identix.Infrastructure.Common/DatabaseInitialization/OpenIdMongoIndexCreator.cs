using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using OpenIddict.MongoDb;
using OpenIddict.MongoDb.Models;

namespace Identix.Infrastructure.Common.DatabaseInitialization;

/// <summary>
/// Класс для создания индексов в коллекциях OpenIddict MongoDB.
/// Обеспечивает оптимальную производительность запросов к базе данных.
/// </summary>
internal static class OpenIdMongoIndexCreator
{
    /// <summary>
    /// Настраивает все необходимые индексы для коллекций OpenIddict.
    /// </summary>
    /// <param name="provider">Провайдер служб для получения зависимостей.</param>
    /// <returns>Задача, представляющая асинхронную операцию создания индексов.</returns>
    public static async Task ConfigureAsync(IServiceProvider provider)
    {
        var context = provider.GetRequiredService<IOpenIddictMongoDbContext>();
        var options = provider.GetRequiredService<IOptionsMonitor<OpenIddictMongoDbOptions>>().CurrentValue;
        var database = await context.GetDatabaseAsync(CancellationToken.None);

        await CreateApplicationIndexesAsync(database, options);
        await CreateAuthorizationIndexesAsync(database, options);
        await CreateScopeIndexesAsync(database, options);
        await CreateTokenIndexesAsync(database, options);
    }

    /// <summary>
    /// Создает индексы для коллекции приложений (Applications).
    /// </summary>
    /// <param name="database">База данных MongoDB.</param>
    /// <param name="options">Настройки OpenIddict MongoDB.</param>
    private static async Task CreateApplicationIndexesAsync(IMongoDatabase database, OpenIddictMongoDbOptions options)
    {
        var applications = database.GetCollection<OpenIddictMongoDbApplication>(options.ApplicationsCollectionName);

        await applications.Indexes.CreateManyAsync(
        [
            // Уникальный индекс для ClientId обеспечивает уникальность идентификаторов клиентов
            new CreateIndexModel<OpenIddictMongoDbApplication>(
                Builders<OpenIddictMongoDbApplication>.IndexKeys.Ascending(application => application.ClientId),
                new CreateIndexOptions
                {
                    Unique = true
                }),

            // Индекс для оптимизации запросов по PostLogoutRedirectUris
            new CreateIndexModel<OpenIddictMongoDbApplication>(
                Builders<OpenIddictMongoDbApplication>.IndexKeys.Ascending(application =>
                    application.PostLogoutRedirectUris),
                new CreateIndexOptions
                {
                    Background = true
                }),

            // Индекс для оптимизации запросов по RedirectUris
            new CreateIndexModel<OpenIddictMongoDbApplication>(
                Builders<OpenIddictMongoDbApplication>.IndexKeys.Ascending(application => application.RedirectUris),
                new CreateIndexOptions
                {
                    Background = true
                })
        ]);
    }

    /// <summary>
    /// Создает индексы для коллекции авторизаций (Authorizations).
    /// </summary>
    /// <param name="database">База данных MongoDB.</param>
    /// <param name="options">Настройки OpenIddict MongoDB.</param>
    private static async Task CreateAuthorizationIndexesAsync(IMongoDatabase database, OpenIddictMongoDbOptions options)
    {
        var authorizations = database.GetCollection<OpenIddictMongoDbAuthorization>(options.AuthorizationsCollectionName);

        // Составной индекс для оптимизации запросов по основным полям авторизации
        await authorizations.Indexes.CreateOneAsync(
            new CreateIndexModel<OpenIddictMongoDbAuthorization>(
                Builders<OpenIddictMongoDbAuthorization>.IndexKeys
                    .Ascending(authorization => authorization.ApplicationId)
                    .Ascending(authorization => authorization.Scopes)
                    .Ascending(authorization => authorization.Status)
                    .Ascending(authorization => authorization.Subject)
                    .Ascending(authorization => authorization.Type),
                new CreateIndexOptions
                {
                    Background = true
                }));
    }

    /// <summary>
    /// Создает индексы для коллекции областей видимости (Scopes).
    /// </summary>
    /// <param name="database">База данных MongoDB.</param>
    /// <param name="options">Настройки OpenIddict MongoDB.</param>
    private static async Task CreateScopeIndexesAsync(IMongoDatabase database, OpenIddictMongoDbOptions options)
    {
        var scopes = database.GetCollection<OpenIddictMongoDbScope>(options.ScopesCollectionName);

        // Уникальный индекс для Name обеспечивает уникальность имен областей видимости
        await scopes.Indexes.CreateOneAsync(
            new CreateIndexModel<OpenIddictMongoDbScope>(
                Builders<OpenIddictMongoDbScope>.IndexKeys.Ascending(scope => scope.Name),
                new CreateIndexOptions
                {
                    Unique = true
                }));
    }

    /// <summary>
    /// Создает индексы для коллекции токенов (Tokens).
    /// </summary>
    /// <param name="database">База данных MongoDB.</param>
    /// <param name="options">Настройки OpenIddict MongoDB.</param>
    private static async Task CreateTokenIndexesAsync(IMongoDatabase database, OpenIddictMongoDbOptions options)
    {
        var tokens = database.GetCollection<OpenIddictMongoDbToken>(options.TokensCollectionName);

        await tokens.Indexes.CreateManyAsync(
        [
            // Уникальный индекс для ReferenceId с частичным фильтром
            new CreateIndexModel<OpenIddictMongoDbToken>(
                Builders<OpenIddictMongoDbToken>.IndexKeys.Ascending(token => token.ReferenceId),
                new CreateIndexOptions<OpenIddictMongoDbToken>
                {
                    // Примечание: выражения частичных фильтров не поддерживаются в Azure Cosmos DB.
                    // В качестве обходного пути можно удалить выражение и уникальное ограничение.
                    PartialFilterExpression =
                        Builders<OpenIddictMongoDbToken>.Filter.Exists(token => token.ReferenceId),
                    Unique = true
                }),

            // Индекс для AuthorizationId с частичным фильтром
            new CreateIndexModel<OpenIddictMongoDbToken>(
                Builders<OpenIddictMongoDbToken>.IndexKeys.Ascending(token => token.AuthorizationId),
                new CreateIndexOptions<OpenIddictMongoDbToken>()
                {
                    PartialFilterExpression =
                        Builders<OpenIddictMongoDbToken>.Filter.Exists(token => token.AuthorizationId)
                }),

            // Составной индекс для оптимизации запросов по основным полям токенов
            new CreateIndexModel<OpenIddictMongoDbToken>(
                Builders<OpenIddictMongoDbToken>.IndexKeys
                    .Ascending(token => token.ApplicationId)
                    .Ascending(token => token.Status)
                    .Ascending(token => token.Subject)
                    .Ascending(token => token.Type),
                new CreateIndexOptions
                {
                    Background = true
                })
        ]);
    }
}