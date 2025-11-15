using Identix.Infrastructure.Web.Settings.InputModels;

namespace Identix.Infrastructure.Web.Settings.ViewModels;

/// <summary>
/// Модель представления для страницы настроек.
/// </summary>
public class SettingsViewModel : CloseSessionsInputModel
{
    /// <summary>
    /// Внешние поставщики
    /// </summary>
    public required IEnumerable<ExternalProvider> ExternalProviders { get; init; }

    /// <summary>
    /// Флаг, определяющий, установленный ли у пользователя пароль
    /// </summary>
    public required bool HasPassword { get; init; }

    /// <summary>
    /// Флаг, определяющий, включена ли 2fa аутентификация
    /// </summary>
    public required bool TwoFactorEnabled { get; init; }

    /// <summary>
    /// Почта пользователя
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Имя пользователя
    /// </summary>
    public required string UserName { get; init; }

    /// <summary>
    /// URL аватара
    /// </summary>
    public string? Thumbnail { get; init; }

    /// <summary>
    /// Сообщение для пользователя
    /// </summary>
    public required string? Message { get; init; }
}