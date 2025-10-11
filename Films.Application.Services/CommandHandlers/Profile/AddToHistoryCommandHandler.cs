using Films.Application.Abstractions.Commands.Profile;
using Films.Application.Abstractions.Exceptions;
using Films.Domain.Repositories;
using MediatR;

namespace Films.Application.Services.CommandHandlers.Profile;

/// <summary>
/// Обработчик команды добавления фильма в историю просмотров пользователя
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с репозиториями</param>
public class AddToHistoryCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<AddToHistoryCommand>
{
    /// <summary>
    /// Обрабатывает команду добавления фильма в историю просмотров
    /// </summary>
    /// <param name="request">Команда с данными (ID пользователя и ID фильма)</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <exception cref="UserNotFoundException">Если пользователь не найден</exception>
    /// <exception cref="FilmNotFoundException">Если фильм не найден</exception>
    public async Task Handle(AddToHistoryCommand request, CancellationToken cancellationToken)
    {
        // Получаем пользователя по ID
        var user = await unitOfWork.UserRepository.Value.GetAsync(request.UserId, cancellationToken);

        // Проверяем существование пользователя
        if (user == null) throw new UserNotFoundException(request.UserId);

        // Получаем фильм по ID
        var film = await unitOfWork.FilmRepository.Value.GetAsync(request.FilmId, cancellationToken);

        // Проверяем существование фильма
        if (film == null) throw new FilmNotFoundException(request.FilmId);

        // Добавляем фильм в историю просмотров пользователя
        user.AddFilmToHistory(film);

        // Обновляем данные пользователя в репозитории
        await unitOfWork.UserRepository.Value.UpdateAsync(user, cancellationToken);

        // Сохраняем изменения в базе данных
        await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
    }
}