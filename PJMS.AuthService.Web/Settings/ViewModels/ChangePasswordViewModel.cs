

using PJMS.AuthService.Web.Settings.InputModels;

namespace PJMS.AuthService.Web.Settings.ViewModels;

/// <summary>
/// Модель представления для изменения пароля.
/// </summary>
public class ChangePasswordViewModel : ChangePasswordInputModel
{
    /// <summary>
    /// Определяет, нужно ли отображать поле для ввода старого пароля.
    /// </summary>
    public required bool ShowOldPassword { get; init; }
}