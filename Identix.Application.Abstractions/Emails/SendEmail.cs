using Common.Application.EmailService;

namespace Identix.Application.Abstractions.Emails;

/// <summary>
/// Команда отправки электронного письма через систему сообщений
/// </summary>
public class SendEmail
{
    /// <summary>
    /// Содержимое email сообщения
    /// </summary>
    public required EmailMessage Message { get; set; }
}