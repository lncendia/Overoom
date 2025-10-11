using Common.IntegrationEvents.Uploader;
using Films.Application.Abstractions.Commands.Films;
using MassTransit;
using MediatR;

namespace Films.Infrastructure.Bus.Uploader;

/// <summary>
/// Обработчик интеграционного события VersionDownloadedIntegrationEvent
/// </summary>
/// <param name="mediator">Медиатор</param>
public class VersionDownloadedConsumer(ISender mediator) : IConsumer<VersionDownloadedIntegrationEvent>
{
    /// <summary>
    /// Метод обработчик 
    /// </summary>
    /// <param name="context">Контекст сообщения</param>
    public Task Consume(ConsumeContext<VersionDownloadedIntegrationEvent> context)
    {
        // Получаем данные события
        var integrationEvent = context.Message;

        // Отправляем команду на обработку события
        return mediator.Send(new AddVersionCommand
        {
            FilmId = integrationEvent.FilmId,
            Version = integrationEvent.Version,
            Season = integrationEvent.Season,
            Episode = integrationEvent.Episode,
        }, context.CancellationToken);
    }
}