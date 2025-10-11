using Common.Application.FileStorage;
using Films.Application.Abstractions.Commands.Playlists;
using Films.Application.Abstractions.Exceptions;
using Films.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Films.Application.Services.CommandHandlers.Playlists;

/// <summary>
/// Обработчик команды удаления плейлиста
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с базой данных</param>
/// <param name="posterStore">Хранилище файлов для удаления обложки плейлиста</param>
public class DeletePlaylistCommandHandler(IUnitOfWork unitOfWork, IFileStorage posterStore, ILogger<DeletePlaylistCommandHandler> logger)
    : IRequestHandler<DeletePlaylistCommand>
{
    /// <summary>
    /// Обрабатывает запрос на удаление плейлиста
    /// </summary>
    /// <param name="request">Команда удаления (ID плейлиста)</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <exception cref="PlaylistNotFoundException">Если плейлист с указанным ID не найден</exception>
    public async Task Handle(DeletePlaylistCommand request, CancellationToken cancellationToken)
    {
        // Получаем плейлист по ID из репозитория
        var playlist = await unitOfWork.PlaylistRepository.Value.GetAsync(request.Id, cancellationToken);

        // Проверяем существование плейлиста
        if (playlist == null) throw new PlaylistNotFoundException(request.Id);
        
        try
        {
            // Удаляем обложку плейлиста из файлового хранилища, если она существует
            await posterStore.DeleteAsync(playlist.PosterKey);
        }
        catch (FileNotFoundException)
        {
            // Логируем отсутствие файла, но не прерываем выполнение
            logger.LogWarning("Файл обложки {poster} не найден в хранилище", playlist.PosterKey);
        }

        // Удаляем запись о плейлисте из репозитория
        await unitOfWork.PlaylistRepository.Value.DeleteAsync(playlist.Id, cancellationToken);

        // Сохраняем изменения в базе данных
        await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
    }
}