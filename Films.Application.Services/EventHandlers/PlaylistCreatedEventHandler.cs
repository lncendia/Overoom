using Common.Application.Events;
using Common.Domain.Events;
using Films.Application.Abstractions.Exceptions;
using Films.Domain.Playlists;
using Films.Domain.Playlists.Specifications;
using Films.Domain.Repositories;

namespace Films.Application.Services.EventHandlers;

/// <summary>
/// Обработчик доменного события создания плейлиста
/// Выполняет проверку на уникальность имени плейлиста перед сохранением
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с базой данных</param>
public class PlaylistCreatedEventHandler(IUnitOfWork unitOfWork) : BeforeSaveNotificationHandler<CreateEvent<Playlist>>
{
    /// <summary>
    /// Обрабатывает событие создания плейлиста и проверяет уникальность имени
    /// </summary>
    /// <param name="notification">Доменное событие создания плейлиста</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <exception cref="PlaylistAlreadyExistsException">Если плейлист с таким именем уже существует</exception>
    protected override async Task Execute(CreateEvent<Playlist> notification, CancellationToken cancellationToken)
    {
        // Проверяем существование плейлиста с таким же именем
        var playlistSpec = new PlaylistByNameSpecification(notification.Aggregate.Name);
        var existingPlaylists = await unitOfWork.PlaylistRepository.Value
            .FindAsync(playlistSpec, cancellationToken: cancellationToken);

        // Если плейлист с таким именем уже существует - бросаем исключение
        // Это гарантирует уникальность имен плейлистов в системе
        if (existingPlaylists.Count > 0) throw new PlaylistAlreadyExistsException(notification.Aggregate.Name);
    }
}