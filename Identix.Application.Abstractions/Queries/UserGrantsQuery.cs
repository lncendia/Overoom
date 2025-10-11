using System.Globalization;
using MediatR;

namespace Identix.Application.Abstractions.Queries;

/// <summary>
/// Запрос для получения списка грантов (разрешений) пользователя с поддержкой локализации
/// </summary>
public class UserGrantsQuery : IRequest<IReadOnlyList<GrantDto>>
{
    /// <summary>
    /// Уникальный идентификатор пользователя
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Культура (язык) для локализации отображаемых имен и описаний
    /// </summary>
    public required CultureInfo Culture { get; init; }
}

/// <summary>
/// DTO для представления информации о гранте пользователя
/// </summary>
public class GrantDto
{
    /// <summary>
    /// Уникальный идентификатор авторизации (гранта)
    /// </summary>
    public required string Id { get; init; }
    
    /// <summary>
    /// Идентификатор клиентского приложения, которому выдан грант
    /// </summary>
    public required string ApplicationId { get; init; }

    /// <summary>
    /// Отображаемое имя клиентского приложения
    /// </summary>
    public required string ClientName { get; init; }

    /// <summary>
    /// URL веб-сайта клиентского приложения
    /// </summary>
    public string? ClientUrl { get; init; }

    /// <summary>
    /// URL логотипа клиентского приложения
    /// </summary>
    public string? ClientLogoUrl { get; init; }

    /// <summary>
    /// Описание гранта или приложения
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Дата и время создания гранта
    /// </summary>
    public required DateTime Created { get; init; }

    /// <summary>
    /// Коллекция scope'ов (разрешений), входящих в данный грант
    /// </summary>
    public required IEnumerable<GrantScopeDto> Scopes { get; init; }
}

/// <summary>
/// DTO для представления отдельного scope (разрешения) в рамках гранта
/// </summary>
public class GrantScopeDto
{
    /// <summary>
    /// Признак того, что scope относится к идентификационным данным (identity scope)
    /// </summary>
    public required bool IdentityScope { get; init; }

    /// <summary>
    /// Локализованное отображаемое имя scope
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Локализованное описание scope
    /// </summary>
    public string? Description { get; init; }
}