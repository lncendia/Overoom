using Films.Application.Abstractions.Commands.Films;
using Films.Domain.Films;
using Films.Domain.Films.ValueObjects;
using Films.Domain.Repositories;
using MediatR;

namespace Films.Application.Services.CommandHandlers.Films;

/// <summary>
/// Обработчик команды добавления нового фильма/сериала
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с базой данных</param>
public class AddFilmCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<AddFilmCommand, Guid>
{
    /// <summary>
    /// Обрабатывает запрос на добавление нового фильма
    /// </summary>
    /// <param name="request">Данные о фильме (основная информация, постер)</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>ID созданного фильма</returns>
    public async Task<Guid> Handle(AddFilmCommand request, CancellationToken cancellationToken)
    {
        // Генерируем новый уникальный идентификатор для фильма
        var id = Guid.NewGuid();

        // Создаем объект фильма с основной информацией
        var film = new Film(id)
        {
            Title = request.Title,
            Description = request.Description,
            Date = request.Date,
            Genres = request.Genres,
            Countries = request.Countries,
            Directors = request.Directors,
            Actors = request.Actors,
            Screenwriters = request.Screenwriters,
            RatingImdb = request.RatingImdb.HasValue ? new Rating(request.RatingImdb.Value) : null,
            RatingKp = request.RatingKp.HasValue ? new Rating(request.RatingKp.Value) : null,
            PosterKey = Constants.FilmPosterKeyDefault,
        };

        // Опциональное поле короткого описания
        if (!string.IsNullOrEmpty(request.ShortDescription))
            film.ShortDescription = request.ShortDescription;

        // Добавляем фильм в репозиторий
        await unitOfWork.FilmRepository.Value.AddAsync(film, cancellationToken);

        // Сохраняем изменения в базе данных
        await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

        // Возвращаем ID созданного фильма
        return film.Id;
    }
}