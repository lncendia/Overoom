using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Common.Infrastructure.Repositories;

/// <summary>
/// Базовый абстрактный класс для реализации Unit of Work паттерна.
/// Координирует работу репозиториев и обеспечивает атомарность операций.
/// с поддержкой различных стратегий работы с сессиями MongoDB.
/// </summary>
/// <param name="handlerFactory">Фабрика для создания обработчиков сессий MongoDB.</param>
/// <param name="publisher">Сервис публикации доменных событий (MediatR).</param>
/// <param name="logger">Логгер для записи информации о выполнении операций.</param>
public abstract class UnitOfWorkBase(ISessionHandlerFactory handlerFactory, IPublisher publisher, ILogger<UnitOfWorkBase> logger)
{
    /// <summary>
    /// Асинхронно сохраняет все изменения, внесенные в репозитории, и завершает транзакцию.
    /// Также отправляет все события доменной модели, которые были зарегистрированы в контексте.
    /// </summary>
    public async Task SaveChangesAsync(ISessionHandler? handler = null, CancellationToken token = default)
    {
        // Если обработчик не передан, создаем обработчик по умолчанию через фабрику.
        handler ??= handlerFactory.CreateDefaultHandler();
        
        // Создаем таймер для замера времени.
        var stopwatch = Stopwatch.StartNew();

        // Применяем изменения к базе данных, используя сессию.
        await handler.ExecuteAsync(async (session, ct) =>
        {
            // Выполняем действия перед началом транзакции
            await BeforeCommitSessionAsync(ct);
            
            // Применяем изменения к базе данных
            await ApplyChanges(session, ct);
        }, token);

        // Останавливаем общий таймер.
        stopwatch.Stop();

        // Логируем о запуске всех инстансов
        logger.LogInformation("Transaction commited in {elapsed} ms.", stopwatch.ElapsedMilliseconds);

        // Выполняем действия после завершения транзакции
        await AfterCommitSessionAsync(token);
    }

    /// <summary>
    /// Применяет все изменения, внесенные в репозитории, к базе данных в рамках указанной сессии MongoDB.
    /// </summary>
    /// <param name="sessionHandle">Существующая сессия MongoDB, в которой будут применены изменения.</param>
    /// <param name="token">Токен отмены для отслеживания отмены операции.</param>
    /// <remarks>
    /// Метод проверяет, были ли созданы изменения в каждом из репозиториев.
    /// Если изменения есть, они применяются к соответствующей коллекции базы данных через метод CommitChangesAsync.
    /// </remarks>
    private async Task ApplyChanges(IClientSessionHandle sessionHandle, CancellationToken token)
    {
        // Получаем массив всех репозиториев с изменениями
        var repositories = GetRepositories().ToArray();

        // Применяем изменения для каждого репозитория
        foreach (var repository in repositories)
        {
            await repository.CommitAsync(sessionHandle, token);
        }
    }

    /// <summary>
    /// Выполняет обработку событий перед подтверждением транзакции
    /// </summary>
    /// <param name="token">Токен отмены для асинхронной операции</param>
    /// <remarks>
    /// Метод обрабатывает все доменные события, помеченные для выполнения перед сохранением.
    /// </remarks>
    private async Task BeforeCommitSessionAsync(CancellationToken token = default)
    {
        // Получаем массив всех репозиториев с изменениями
        var repositories = GetRepositories().ToArray();

        // Выбираем все доменные события, которые должны быть обработаны после сохранения
        var domainEvents = repositories.SelectMany(r => r.Events);

        // Создаем таймер для замера времени выполнения операций
        var stopwatch = Stopwatch.StartNew();

        // Публикуем все события, которые должны быть обработаны после сохранения
        foreach (var domainEvent in domainEvents)
        {
            domainEvent.BeforeSave = true;
            await publisher.Publish(domainEvent, token);
        }

        // Останавливаем общий таймер
        stopwatch.Stop();

        // Логируем время выполнения операций
        logger.LogInformation("Before save events executed in {elapsed} ms.", stopwatch.ElapsedMilliseconds);
    }

    /// <summary>
    /// Выполняет обработку событий после подтверждения транзакции
    /// </summary>
    /// <param name="token">Токен отмены для асинхронной операции</param>
    /// <remarks>
    /// Метод обрабатывает все доменные события, помеченные для выполнения после сохранения.
    /// В случае ошибки при обработке события, ошибка логируется, но не прерывает выполнение.
    /// </remarks>
    private async Task AfterCommitSessionAsync(CancellationToken token = default)
    {
        // Получаем массив всех репозиториев с изменениями
        var repositories = GetRepositories().ToArray();

        // Выбираем все доменные события, которые должны быть обработаны после сохранения
        var domainEvents = repositories.SelectMany(r => r.Events);

        // Создаем таймер для замера времени выполнения операций
        var stopwatch = Stopwatch.StartNew();

        // Публикуем все события, которые должны быть обработаны после сохранения
        foreach (var domainEvent in domainEvents)
        {
            try
            {
                domainEvent.BeforeSave = false;
                await publisher.Publish(domainEvent, token);
            }
            catch (Exception ex)
            {
                // Логируем ошибку, но продолжаем выполнение для остальных событий
                logger.LogWarning(ex, "An error occured while executing after save event.");
            }
        }

        // Останавливаем общий таймер
        stopwatch.Stop();

        // Логируем время выполнения операций
        logger.LogInformation("After save events executed in {elapsed} ms.", stopwatch.ElapsedMilliseconds);
    }

    /// <summary>
    /// Получает коллекцию репозиториев, в которых были изменения
    /// </summary>
    /// <returns>Коллекция измененных репозиториев</returns>
    protected abstract IEnumerable<IRepository> GetRepositories();
}