using Common.Application.FileStorage;
using Films.Application.Abstractions;
using Films.Application.Abstractions.Commands.Films;
using Films.Application.Abstractions.Exceptions;
using Films.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Films.Application.Services.CommandHandlers.Films;

/// <summary>
/// Обработчик команды изменения фото фильма
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с базой данных</param>
/// <param name="posterStore">Хранилище файлов для работы с постерами</param>
/// <param name="logger">Логгер для записи событий</param>
public class ChangeFilmPosterCommandHandler(
    IUnitOfWork unitOfWork,
    IFileStorage posterStore,
    ILogger<ChangeFilmPosterCommandHandler> logger) : IRequestHandler<ChangeFilmPosterCommand>
{
    /// <summary>
    /// Обновляет информацию о фильме
    /// </summary>
    /// <param name="request">Команда с обновляемыми данными фильма</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <exception cref="FilmNotFoundException">Если фильм с указанным ID не найден</exception>
    public async Task Handle(ChangeFilmPosterCommand request, CancellationToken cancellationToken)
    {
        // Получаем фильм по ID из репозитория
        var film = await unitOfWork.FilmRepository.Value.GetAsync(request.Id, cancellationToken);

        // Проверяем существование фильма
        if (film == null) throw new FilmNotFoundException(request.Id);

        // Генерируем новый ключ для хранения постера
        var newPosterKey = string.Format(Constants.Poster.FilmKeyFormat, Guid.NewGuid());

        // Загружаем новый постер в хранилище
        await posterStore.UploadAsync(newPosterKey, request.Poster.File, Constants.Mime.Photo,
            token: cancellationToken);

        if (film.PosterKey != Constants.Poster.FilmDefault)
        {
            try
            {
                // Пытаемся удалить старый постер
                await posterStore.DeleteAsync(film.PosterKey, token: cancellationToken);
            }
            catch (FileNotFoundException ex)
            {
                logger.LogWarning(ex, "Файл обложки {PosterKey} не найден в хранилище", film.PosterKey);
            }
        }

        // Сохраняем новый ключ постера
        film.PosterKey = newPosterKey;

        // Сохраняем изменения в репозитории
        await unitOfWork.FilmRepository.Value.UpdateAsync(film, cancellationToken);

        // Фиксируем изменения в базе данных
        await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
    }
}