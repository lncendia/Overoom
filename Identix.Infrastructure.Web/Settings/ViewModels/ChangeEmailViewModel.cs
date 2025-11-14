using Identix.Infrastructure.Web.Settings.InputModels;

namespace Identix.Infrastructure.Web.Settings.ViewModels;

/// <summary>
/// Модель представления для изменения почты.
/// </summary>
public class ChangeEmailViewModel : RequestChangeEmailInputModel
{
    /// <summary>
    /// Определяет, нужно ли отображать поле для ввода пароля.
    /// </summary>
    public required bool ShowPassword { get; init; }
}