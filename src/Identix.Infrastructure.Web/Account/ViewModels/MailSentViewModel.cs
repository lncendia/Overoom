namespace Identix.Infrastructure.Web.Account.ViewModels;

/// <summary>
/// ViewModel страницы отправки письма
/// </summary>
/// <param name="Message">Текст страницы</param>
/// <param name="ReturnUrl">URL возврата</param>
public record MailSentViewModel(string Message, string ReturnUrl);