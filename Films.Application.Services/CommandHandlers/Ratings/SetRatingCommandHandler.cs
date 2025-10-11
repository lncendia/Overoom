using Common.Domain.Specifications;
using Films.Application.Abstractions.Commands.Ratings;
using Films.Application.Abstractions.Exceptions;
using Films.Domain.Ratings;
using Films.Domain.Ratings.Specifications;
using Films.Domain.Repositories;
using MediatR;

namespace Films.Application.Services.CommandHandlers.Ratings;

/// <summary>
/// Обработчик команды добавления/обновления оценки фильма пользователем
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с репозиториями</param>
public class SetRatingCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<SetRatingCommand>
{
    /// <summary>
    /// Добавляет новую или обновляет существующую оценку фильма пользователем
    /// </summary>
    /// <param name="request">Команда с данными оценки (ID пользователя, ID фильма, оценка)</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <exception cref="UserNotFoundException">Если пользователь не найден</exception>
    /// <exception cref="FilmNotFoundException">Если фильм не найден</exception>
    public async Task Handle(SetRatingCommand request, CancellationToken cancellationToken)
    {
        // Получаем пользователя по ID из запроса
        var user = await unitOfWork.UserRepository.Value.GetAsync(request.UserId, cancellationToken);

        // Проверяем существование пользователя
        if (user == null) throw new UserNotFoundException(request.UserId);

        // Получаем фильм по ID из запроса
        var film = await unitOfWork.FilmRepository.Value.GetAsync(request.FilmId, cancellationToken);

        // Проверяем существование фильма
        if (film == null) throw new FilmNotFoundException(request.FilmId);

        // Создаем спецификацию для поиска существующей оценки
        // (ищем оценку данного пользователя для данного фильма)
        var spec = new RatingByFilmSpecification(request.FilmId)
            .And(new RatingByUserSpecification(request.UserId));

        // Ищем существующую оценку
        var rating = await unitOfWork.RatingRepository.Value.FirstOrDefaultAsync(spec, cancellationToken);

        // Если оценка не найдена - создаем новую
        if (rating == null)
        {
            // Создаем новую оценку с уникальным ID
            rating = new Rating(Guid.NewGuid(), film, user, request.Score);

            // Добавляем оценку в репозиторий
            await unitOfWork.RatingRepository.Value.AddAsync(rating, cancellationToken);
        }
        else
        {
            // Если оценка уже существует - обновляем ее значение
            rating.Score = request.Score;

            // Обновляем оценку в репозитории
            await unitOfWork.RatingRepository.Value.UpdateAsync(rating, cancellationToken);
        }

        // Сохраняем все изменения в базе данных
        await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
    }
}