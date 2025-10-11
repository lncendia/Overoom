using Films.Application.Abstractions.Commands.Profile;
using Films.Application.Abstractions.Exceptions;
using Films.Domain.Repositories;
using MediatR;

namespace Films.Application.Services.CommandHandlers.Profile;

/// <summary>
/// Обработчик команды добавления/удаления фильма из списка просмотра пользователя
/// </summary>
/// <param name="unitOfWork">Единица работы для доступа к репозиториям данных</param>
public class ToggleWatchListCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<ToggleWatchListCommand>
{
    /// <summary>
    /// Переключает состояние фильма в списке "К просмотру" пользователя
    /// (добавляет, если отсутствует, или удаляет, если уже есть в списке)
    /// </summary>
    /// <param name="request">Команда с ID пользователя и ID фильма</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <exception cref="UserNotFoundException">Если пользователь не найден</exception>
    /// <exception cref="FilmNotFoundException">Если фильм не найден</exception>
    public async Task Handle(ToggleWatchListCommand request, CancellationToken cancellationToken)
    {
        // Получаем пользователя по ID из запроса
        var user = await unitOfWork.UserRepository.Value.GetAsync(request.UserId, cancellationToken);

        // Проверяем существование пользователя
        if (user == null) throw new UserNotFoundException(request.UserId);

        // Получаем фильм по ID из запроса
        var film = await unitOfWork.FilmRepository.Value.GetAsync(request.FilmId, cancellationToken);

        // Проверяем существование фильма
        if (film == null) throw new FilmNotFoundException(request.FilmId);

        // Переключаем состояние фильма в списке просмотра пользователя
        // (метод ToggleWatchlist должен сам определять, добавлять или удалять фильм)
        user.ToggleWatchlist(film);

        // Обновляем данные пользователя в репозитории
        await unitOfWork.UserRepository.Value.UpdateAsync(user, cancellationToken);

        // Сохраняем изменения в базе данных
        await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
    }
}