using MediatR;
using OpenIddict.Abstractions;
using Identix.Application.Abstractions.Extensions;
using Identix.Application.Abstractions.Queries;

namespace Identix.Application.Services.Queries;

/// <summary>
/// Обработчик запроса на получение списка грантов (разрешений) пользователя
/// </summary>
public class UserGrantsHandler(
    IOpenIddictApplicationManager applicationManager,
    IOpenIddictAuthorizationManager authorizationManager,
    IOpenIddictScopeManager scopeManager)
    : IRequestHandler<UserGrantsQuery, IReadOnlyList<GrantDto>>
{
    /// <summary>
    /// Обрабатывает запрос на получение грантов пользователя
    /// </summary>
    /// <param name="request">Запрос с параметрами пользователя</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Список DTO объектов с информацией о грантах</returns>
    public async Task<IReadOnlyList<GrantDto>> Handle(UserGrantsQuery request, CancellationToken cancellationToken)
    {
        // Получаем все активные авторизации пользователя по его идентификатору
        // Авторизации представляют собой выданные разрешения для конкретных приложений
        var authorizations = await authorizationManager.FindAsync(
            subject: request.UserId.ToString(),
            client: null,
            status: OpenIddictConstants.Statuses.Valid,
            type: OpenIddictConstants.AuthorizationTypes.Permanent,
            scopes: null,
            cancellationToken: cancellationToken
        ).ToListAsync(cancellationToken);

        // Инициализируем коллекцию для хранения результата
        var grants = new List<GrantDto>();

        // Обрабатываем каждую авторизацию для формирования полной информации о гранте
        foreach (var authorization in authorizations)
        {
            // Создаем и заполняем дескриптор авторизации для получения дополнительных метаданных
            var authDescriptor = new OpenIddictAuthorizationDescriptor();
            await authorizationManager.PopulateAsync(authDescriptor, authorization, cancellationToken);

            // Находим приложение по идентификатору (пропускаем если приложение не найдено)
            var app = await applicationManager.FindByIdAsync(authDescriptor.ApplicationId!, cancellationToken);
            if (app is null) continue;

            // Создаем и заполняем дескриптор приложения для получения метаданных
            var appDescriptor = new OpenIddictApplicationDescriptor();
            await applicationManager.PopulateAsync(appDescriptor, app, cancellationToken);

            // Получаем список scope'ов (разрешений), связанных с данной авторизацией
            var scopeNames = await authorizationManager.GetScopesAsync(authorization, cancellationToken);

            // Коллекция для хранения информации о scope'ах
            var scopes = new List<GrantScopeDto>(scopeNames.Length);

            // Для каждого scope получаем детальную информацию и локализованные названия
            await foreach (var scope in scopeManager.FindByNamesAsync(scopeNames, cancellationToken))
            {
                // Создаем и заполняем дескриптор scope для получения метаданных
                var descriptor = new OpenIddictScopeDescriptor();
                await scopeManager.PopulateAsync(descriptor, scope, cancellationToken);

                // Определяем тип scope (identity scope или resource scope)
                var isIdentity = descriptor.IsIdentityScope();

                // Получаем локализованное отображаемое имя scope
                var displayName = descriptor.GetDisplayName(request.Culture);

                // Получаем локализованное описание scope
                var description = descriptor.GetDescription(request.Culture);

                // Создаем DTO объекта scope с собранной информацией
                scopes.Add(new GrantScopeDto
                {
                    IdentityScope = isIdentity,
                    DisplayName = displayName ?? descriptor.Name ?? string.Empty,
                    Description = description
                });
            }

            // Формируем итоговый DTO гранта со всей собранной информацией
            grants.Add(new GrantDto
            {
                // Уникальный идентификатор авторизации
                Id = await authorizationManager.GetIdAsync(authorization, cancellationToken) ?? string.Empty,

                // Идентификатор клиентского приложения
                ApplicationId = authDescriptor.ApplicationId ?? string.Empty,

                // Отображаемое имя приложения (или clientId если имя не задано)
                ClientName = appDescriptor.DisplayName ?? authDescriptor.ApplicationId!,

                // URL веб-сайта приложения
                ClientUrl = appDescriptor.GetClientUri(),

                // URL логотипа приложения
                ClientLogoUrl = appDescriptor.GetLogoUri(),

                // Описание гранта
                Description = authDescriptor.GetDescription(),

                // Дата создания авторизации (приводится к UTC)
                Created = authDescriptor.CreationDate?.ToUniversalTime().Date ?? DateTime.MinValue,

                // Список scope'ов (разрешений) входящих в данный грант
                Scopes = scopes
            });
        }

        return grants;
    }
}