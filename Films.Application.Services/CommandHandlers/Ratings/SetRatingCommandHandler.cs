using Common.Infrastructure.Repositories;

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
/// <param name="sessionHandlerFactory">Фабрика обработчиков сессий MongoDB</param>
/// <param name="unitOfWork">Единица работы для взаимодействия с репозиториями</param>
public class SetRatingCommandHandler(ISessionHandlerFactory sessionHandlerFactory, IUnitOfWork unitOfWork) : IRequestHandler<SetRatingCommand>
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
    User? user = await unitOfWork.UserRepository.Value.GetAsync(request.UserId, cancellationToken);
    if (user == null) throw new UserNotFoundException(request.UserId);

    Film? film = await unitOfWork.FilmRepository.Value.GetAsync(request.FilmId, cancellationToken);
    if (film == null) throw new FilmNotFoundException(request.FilmId);

    var spec = new DuplicateRatingsSpecification(request.FilmId, request.UserId);
    Rating? rating = await unitOfWork.RatingRepository.Value.FirstOrDefaultAsync(spec, cancellationToken);

    if (rating == null)
    {
      rating = new Rating(Guid.NewGuid(), film, user, request.Score);
      await unitOfWork.RatingRepository.Value.AddAsync(rating, cancellationToken);
    }
    else
    {
      rating.Score = request.Score;
      await unitOfWork.RatingRepository.Value.UpdateAsync(rating, cancellationToken);
    }

    // Оценка и счётчики оценок фильма должны фиксироваться вместе
    await unitOfWork.SaveChangesAsync(sessionHandlerFactory.CreateTransactionHandler(), cancellationToken);
  }
}