using MediatR;
using OpenIddict.Abstractions;
using Identix.Application.Abstractions.Commands.OpenId;

namespace Identix.Application.Services.Commands.OpenId;

/// <summary>
/// Обработчик команды для отзыва гранта пользователя.
/// </summary>
/// <param name="authorizationManager">Менеджер авторизаций OpenIddict.</param>
public class RevokeGrantHandler(IOpenIddictAuthorizationManager authorizationManager) 
    : IRequestHandler<RevokeGrantCommand>
{
    /// <summary>
    /// Обрабатывает команду отзыва гранта.
    /// </summary>
    /// <param name="request">Команда отзыва, содержащая Id гранта и Id пользователя.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <exception cref="InvalidOperationException">Выбрасывается, если авторизация не найдена.</exception>
    /// <exception cref="UnauthorizedAccessException">Выбрасывается, если авторизация не принадлежит пользователю.</exception>
    public async Task Handle(RevokeGrantCommand request, CancellationToken cancellationToken)
    {
        // Ищем авторизацию по Id
        var authorization = await authorizationManager.FindByIdAsync(request.GrantId, cancellationToken);
        if (authorization is null)
        {
            throw new InvalidOperationException("Authorization not found");
        }

        // Проверяем, что авторизация принадлежит текущему пользователю
        var subject = await authorizationManager.GetSubjectAsync(authorization, cancellationToken);
        if (!Guid.TryParse(subject, out var subjectGuid) || subjectGuid != request.UserId)
        {
            throw new UnauthorizedAccessException("Authorization does not belong to the user");
        }

        // Отзываем авторизацию (помечаем как недействительную)
        await authorizationManager.TryRevokeAsync(authorization, cancellationToken);
    }
}
