namespace Identix.Infrastructure.Web.Registration.ViewModels;

/// <summary>
/// ViewModel страницы подтверждения почты
/// </summary>
/// <param name="ReturnUrl">URL возврата</param>
public record ConfirmEmailViewModel(string ReturnUrl);