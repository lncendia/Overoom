using System;
using System.Reflection;
using AspNetCore.Identity.Mongo.Model;
using AspNetCore.Identity.Mongo.Stores;
using Common.DI.Extensions;
using Identix.Infrastructure.Common.Mongo;
using MassTransit.MongoDbIntegration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace Identix.Extensions;

/// <summary>
/// Расширения для регистрации ASP.NET Core Identity поверх MongoDB.
/// Поддерживает интеграцию с MassTransit Outbox.
/// </summary>
public static class MongoIdentityExtensions
{
    /// <summary>
    /// Опции конфигурации MongoDB для Identity.
    /// </summary>
    public class MongoIdentityOptions
    {
        /// <summary>
        /// Имя базы данных MongoDB, используемой для хранения пользователей и ролей.
        /// </summary>
        public string DatabaseName { get; set; } = "Identity";

        /// <summary>
        /// Имя коллекции, в которой будут храниться пользователи.
        /// </summary>
        public string UsersCollection { get; set; } = "Users";

        /// <summary>
        /// Имя коллекции, в которой будут храниться роли.
        /// </summary>
        public string RolesCollection { get; set; } = "Roles";

        /// <summary>
        /// Имя коллекции для хранения истории миграций.
        /// </summary>
        public string MigrationCollection { get; set; } = "_Migrations";

        /// <summary>
        /// Отключает автоматический запуск миграций при инициализации.
        /// </summary>
        public bool DisableAutoMigrations { get; set; }
    }

    /// <summary>
    /// Выполняет миграции для Identity-схемы MongoDB через внутренние классы пакета AspNetCore.Identity.Mongo.
    /// </summary>
    private static void InvokeMigrator<TKey, TRole>(
        IMongoDatabase database,
        IMongoCollection<MigrationHistory> migrationCollection,
        IMongoCollection<TRole> roleCollection,
        string usersCollectionName)
    {
        // Получаем сборку, содержащую внутренние типы Identity.Mongo
        var asm = typeof(MongoRole).Assembly;

        // Находим generic тип MigrationMongoUser<TKey>
        var migrationUserType = asm.GetType("AspNetCore.Identity.Mongo.Model.MigrationMongoUser`1")
                                    ?.MakeGenericType(typeof(TKey))
                                ?? throw new InvalidOperationException("MigrationMongoUser<TKey> not found.");

        // Получаем коллекцию MigrationMongoUser<TKey> из базы данных
        var getCollection = typeof(IMongoDatabase)
            .GetMethod("GetCollection", 1, [typeof(string), typeof(MongoCollectionSettings)])!;

        var migrationUserCollection = getCollection
            .MakeGenericMethod(migrationUserType)
            .Invoke(database, [usersCollectionName, null]);

        // Находим внутренний тип Migrator
        var migratorType = asm.GetType("AspNetCore.Identity.Mongo.Migrations.Migrator")
                           ?? throw new InvalidOperationException("Migrator not found.");

        // Получаем метод Apply<MigrationMongoUser<TKey>, TRole, TKey>
        var applyMethod = migratorType.GetMethod("Apply", BindingFlags.Public | BindingFlags.Static)
                          ?? throw new InvalidOperationException("Migrator.Apply not found.");

        var genericApply = applyMethod.MakeGenericMethod(migrationUserType, typeof(TRole), typeof(TKey));

        // Вызываем Apply, чтобы применить миграции
        genericApply.Invoke(null, [migrationCollection, migrationUserCollection, roleCollection]);
    }

    /// <summary>
    /// Регистрирует internal ObjectId-конвертер через рефлексию, если используется ObjectId в качестве ключа.
    /// </summary>
    private static void RegisterObjectIdConverter()
    {
        // Получаем сборку, где определены внутренние сериализаторы Identity.Mongo
        var asm = typeof(MongoRole).Assembly;

        // Находим internal класс TypeConverterResolver
        var resolverType = asm.GetType("AspNetCore.Identity.Mongo.Mongo.TypeConverterResolver")
                           ?? throw new InvalidOperationException("TypeConverterResolver not found.");

        // Находим internal класс ObjectIdConverter
        var objectIdConverterType = asm.GetType("AspNetCore.Identity.Mongo.Mongo.ObjectIdConverter")
                                    ?? throw new InvalidOperationException("ObjectIdConverter not found.");

        // Получаем метод RegisterTypeConverter<TModel, TConverter>()
        var registerMethod = resolverType.GetMethod("RegisterTypeConverter", BindingFlags.Public | BindingFlags.Static)
                             ?? throw new InvalidOperationException("RegisterTypeConverter not found.");

        // Создаём generic-версию RegisterTypeConverter<ObjectId, ObjectIdConverter>()
        var genericRegister = registerMethod.MakeGenericMethod(typeof(ObjectId), objectIdConverterType);

        // Регистрируем конвертер
        genericRegister.Invoke(null, null);
    }

    /// <summary>
    /// Оборачивает коллекцию в OutboxMongoCollection для поддержки паттерна Outbox (MassTransit).
    /// </summary>
    /// <typeparam name="T">Тип документов в коллекции.</typeparam>
    /// <param name="serviceProvider">Провайдер сервисов.</param>
    /// <param name="inner">Оригинальная коллекция MongoDB.</param>
    /// <returns>Обёрнутая коллекция с Outbox-поддержкой.</returns>
    private static OutboxMongoCollection<T> GetOutboxedCollection<T>(IServiceProvider serviceProvider,
        IMongoCollection<T> inner)
    {
        // Получаем контекст MongoDB (в нём есть IClientSession для Outbox)
        var mongoDbContext = serviceProvider.GetRequiredService<MongoDbContext>();

        // Оборачиваем коллекцию в OutboxMongoCollection
        return new OutboxMongoCollection<T>(inner, mongoDbContext);
    }

    /// <summary>
    /// Регистрирует ASP.NET Core Identity с провайдером MongoDB.
    /// Поддерживает ObjectId, Guid и интеграцию с Outbox.
    /// </summary>
    /// <typeparam name="TUser">Тип пользователя (наследуется от MongoUser&lt;TKey&gt;).</typeparam>
    /// <typeparam name="TRole">Тип роли (наследуется от MongoRole&lt;TKey&gt;).</typeparam>
    /// <typeparam name="TKey">Тип ключа (ObjectId или Guid).</typeparam>
    /// <param name="services">Контейнер DI.</param>
    /// <param name="setupIdentityAction">Делегат настройки IdentityOptions.</param>
    /// <param name="setupDatabaseAction">Делегат настройки MongoIdentityOptions.</param>
    /// <param name="identityErrorDescriber">Необязательный описатель ошибок Identity.</param>
    /// <returns>Построитель Identity.</returns>
    public static IdentityBuilder AddIdentityMongoDbProviderWithOutbox<TUser, TRole, TKey>(
        this IServiceCollection services,
        Action<IdentityOptions>? setupIdentityAction,
        Action<MongoIdentityOptions> setupDatabaseAction,
        IdentityErrorDescriber? identityErrorDescriber = null)
        where TKey : IEquatable<TKey>
        where TUser : MongoUser<TKey>
        where TRole : MongoRole<TKey>
    {
        // Конфигурируем MongoDB-опции
        var dbOptions = new MongoIdentityOptions();
        setupDatabaseAction(dbOptions);

        // Получаем подключение к базе данных
        var database = MongoDbProvider.Client.GetDatabase(dbOptions.DatabaseName);

        // Получаем коллекции
        var migrationCollection = database.GetCollection<MigrationHistory>(dbOptions.MigrationCollection);
        var userCollection = database.GetCollection<TUser>(dbOptions.UsersCollection);
        var roleCollection = database.GetCollection<TRole>(dbOptions.RolesCollection);

        // Применяем миграции, если не отключены
        if (!dbOptions.DisableAutoMigrations)
        {
            InvokeMigrator<TKey, TRole>(database, migrationCollection, roleCollection, dbOptions.UsersCollection);
        }

        // Регистрируем стандартные Identity-сервисы
        var builder = services.AddIdentity<TUser, TRole>(setupIdentityAction ?? (_ => { }));

        builder.AddRoleStore<RoleStore<TRole, TKey>>()
            .AddUserStore<UserStore<TUser, TRole, TKey>>()
            .AddUserManager<UserManager<TUser>>()
            .AddRoleManager<RoleManager<TRole>>()
            .AddDefaultTokenProviders();

        // Регистрируем коллекции с поддержкой Outbox
        services.AddScoped<IMongoCollection<TUser>>(sp => GetOutboxedCollection(sp, userCollection));
        services.AddScoped<IMongoCollection<TRole>>(sp => GetOutboxedCollection(sp, roleCollection));

        // Настраиваем сериализаторы под тип ключа
        if (typeof(TKey) == typeof(ObjectId))
        {
            // Для ObjectId нужно зарегистрировать кастомный конвертер
            RegisterObjectIdConverter();
        }

        // Регистрируем сторы (user/role store) для Identity
        services.AddTransient<IRoleStore<TRole>>(sp =>
            new RoleStore<TRole, TKey>(GetOutboxedCollection(sp, roleCollection), identityErrorDescriber));

        services.AddTransient<IUserStore<TUser>>(sp =>
            new UserStore<TUser, TRole, TKey>(
                GetOutboxedCollection(sp, userCollection),
                GetOutboxedCollection(sp, roleCollection),
                identityErrorDescriber));

        // Возвращаем builder для дальнейшей конфигурации Identity
        return builder;
    }
}
