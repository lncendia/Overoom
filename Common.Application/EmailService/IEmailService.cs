namespace Common.Application.EmailService;

/// <summary>
/// Интерфейс отправки Email
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Метод отправляет Email
    /// </summary>
    /// <param name="emailData">Объект данных об отправляемом Email</param>
    /// <param name="token">Токен отмены для отслеживания отмены операции.</param>
    Task SendAsync(EmailMessage emailData, CancellationToken token = default);
}