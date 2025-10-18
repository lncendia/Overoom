using Common.Application.FileStorage;
using Films.Application.Abstractions;
using Films.Application.Abstractions.Commands.Playlists;
using Films.Application.Abstractions.Exceptions;
using Films.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Films.Application.Services.CommandHandlers.Playlists;

/// <summary>
/// Обработчик команды изменения фото подборки
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с базой данных</param>
/// <param name="posterStore">Хранилище файлов для работы с постерами</param>
/// <param name="logger">Логгер для записи событий</param>
public class ChangePlaylistPosterCommandHandler(
    IUnitOfWork unitOfWork,
    IFileStorage posterStore,
    ILogger<ChangePlaylistPosterCommandHandler> logger) : IRequestHandler<ChangePlaylistPosterCommand>
{
    /// <summary>
    /// Обновляет информацию о подборке
    /// </summary>
    /// <param name="request">Команда с обновляемыми данными подборки</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <exception cref="FilmNotFoundException">Если подборка с указанным ID не найден</exception>
    public async Task Handle(ChangePlaylistPosterCommand request, CancellationToken cancellationToken)
    {
        // Получаем подборку по ID из репозитория
        var playlist = await unitOfWork.PlaylistRepository.Value.GetAsync(request.Id, cancellationToken);

        // Проверяем существование подборки
        if (playlist == null) throw new PlaylistNotFoundException(request.Id);

        // Генерируем новый ключ для хранения постера
        var newPosterKey = string.Format(Constants.Poster.PlaylistKeyFormat, Guid.NewGuid());

        // Загружаем новый постер в хранилище
        await posterStore.UploadAsync(newPosterKey, request.Poster.File, Constants.Mime.Photo,
            token: cancellationToken);

        if (playlist.PosterKey != Constants.Poster.PlaylistDefault)
        {
            try
            {
                // Пытаемся удалить старый постер
                await posterStore.DeleteAsync(playlist.PosterKey, token: cancellationToken);
            }
            catch (FileNotFoundException ex)
            {
                logger.LogWarning(ex, "Файл обложки {PosterKey} не найден в хранилище", playlist.PosterKey);
            }
        }

        // Сохраняем новый ключ постера
        playlist.PosterKey = newPosterKey;

        // Сохраняем изменения в репозитории
        await unitOfWork.PlaylistRepository.Value.UpdateAsync(playlist, cancellationToken);

        // Фиксируем изменения в базе данных
        await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
    }
}