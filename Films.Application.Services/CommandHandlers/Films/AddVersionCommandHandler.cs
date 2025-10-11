using Films.Application.Abstractions.Commands.Films;
using Films.Application.Abstractions.Exceptions;
using Films.Domain.Repositories;
using MediatR;

namespace Films.Application.Services.CommandHandlers.Films;

/// <summary>
/// Обработчик команды добавления новой версии фильма или серии
/// </summary>
/// <param name="unitOfWork">Единица работы для взаимодействия с базой данных</param>
public class AddVersionCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<AddVersionCommand>
{
    /// <summary>
    /// Обрабатывает команду добавления новой версии фильма или серии
    /// </summary>
    /// <param name="request">Команда с данными для добавления версии</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Задача, представляющая асинхронную операцию</returns>
    /// <exception cref="FilmNotFoundException">Если фильм с указанным ID не найден</exception>
    public async Task Handle(AddVersionCommand request, CancellationToken cancellationToken)
    {
        // Получаем фильм по ID из репозитория
        var film = await unitOfWork.FilmRepository.Value.GetAsync(request.FilmId, cancellationToken);

        // Проверяем существование фильма
        if (film == null) throw new FilmNotFoundException(request.FilmId);
        
        // Добавляем новую версию к фильму
        film.AddVersion(
            version: request.Version,
            seasonNumber: request.Season,
            episodeNumber: request.Episode
        );

        // Сохраняем изменения в репозитории
        await unitOfWork.FilmRepository.Value.UpdateAsync(film, cancellationToken);

        // Фиксируем изменения в базе данных
        await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
    }
}