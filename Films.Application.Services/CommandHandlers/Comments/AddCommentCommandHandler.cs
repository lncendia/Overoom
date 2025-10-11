using Films.Application.Abstractions.Commands.Comments;
using Films.Application.Abstractions.Exceptions;
using Films.Domain.Comments;
using Films.Domain.Repositories;
using MediatR;

namespace Films.Application.Services.CommandHandlers.Comments;

/// <summary>
/// Обработчик команды добавления комментария
/// </summary>
/// <param name="unitOfWork">Единица работы для доступа к репозиториям</param>
public class AddCommentCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<AddCommentCommand, Guid>
{
    /// <summary>
    /// Обрабатывает команду добавления нового комментария
    /// </summary>
    /// <param name="request">Команда с данными для создания комментария</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>DTO созданного комментария</returns>
    /// <exception cref="UserNotFoundException">Выбрасывается, если пользователь не найден</exception>
    /// <exception cref="FilmNotFoundException">Выбрасывается, если фильм не найден</exception>
    public async Task<Guid> Handle(AddCommentCommand request, CancellationToken cancellationToken)
    {
        // Получаем пользователя по ID
        var user = await unitOfWork.UserRepository.Value.GetAsync(request.UserId, cancellationToken);
        
        // Проверяем, что пользователь существует
        if (user == null) throw new UserNotFoundException(request.UserId);
        
        // Получаем фильм по ID
        var film = await unitOfWork.FilmRepository.Value.GetAsync(request.FilmId, cancellationToken);
        
        // Проверяем, что фильм существует
        if (film == null) throw new FilmNotFoundException(request.FilmId);
        
        // Создаем новый комментарий
        var comment = new Comment(Guid.NewGuid(), film, user, request.Text);
        
        // Добавляем комментарий в репозиторий
        await unitOfWork.CommentRepository.Value.AddAsync(comment, cancellationToken);
        
        // Сохраняем изменения в БД
        await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
        
        // Возвращаем DTO созданного комментария
        return comment.Id;
    }
}