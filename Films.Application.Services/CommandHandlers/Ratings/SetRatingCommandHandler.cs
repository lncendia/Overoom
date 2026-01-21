using Films.Application.Abstractions.Commands.Ratings;
using Films.Application.Abstractions.Exceptions;
using Films.Domain.Films;
using Films.Domain.Ratings;
using Films.Domain.Ratings.Specifications;
using Films.Domain.Repositories;
using Films.Domain.Users;

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
    User? user = await unitOfWork.UserRepository.Value.GetAsync(request.UserId, cancellationToken);

    // Проверяем существование пользователя
    if (user == null) throw new UserNotFoundException(request.UserId);

    // Получаем фильм по ID из запроса
    Film? film = await unitOfWork.FilmRepository.Value.GetAsync(request.FilmId, cancellationToken);

    // Проверяем существование фильма
    if (film == null) throw new FilmNotFoundException(request.FilmId);

    // Создаем спецификацию для поиска существующей оценки
    var spec = new DuplicateRatingsSpecification(request.FilmId, request.UserId);

    // Ищем существующую оценку
    Rating? rating = await unitOfWork.RatingRepository.Value.FirstOrDefaultAsync(spec, cancellationToken);

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