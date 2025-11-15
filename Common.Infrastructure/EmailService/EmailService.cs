using Common.Application.EmailService;
using Common.Application.Exceptions;
using MimeKit;

namespace Common.Infrastructure.EmailService;

/// <inheritdoc />
/// <summary>
/// Реализация интерфейс отправки Email
/// </summary>
/// <param name="smtpConfiguration">Конфигурация SMTP</param>
/// <param name="visitor">Посетитель сообщения</param>
public class EmailService(SmtpConfiguration smtpConfiguration, IEmailVisitor visitor) : IEmailService
{
    /// <inheritdoc />
    /// <summary>
    /// Метод отправляет Email
    /// </summary>
    /// <param name="emailData">Объект данных об отправляемом Email</param>
    /// <param name="token">Токен отмены для отслеживания отмены операции.</param>
    public Task SendAsync(EmailMessage emailData, CancellationToken token = default)
    {
        // Посещаем письмо
        emailData.Accept(visitor);

        // Если визитор по какой-то причине не установил свойства - не продолжаем
        if (visitor.Subject is null || visitor.Body is null) return Task.CompletedTask;

        // Отправляем Email, контент берем из посетителя
        return SendEmailBySmtpAsync(emailData.Recipient, visitor.Subject, visitor.Body, token);
    }

    /// <summary>
    /// Метод отправляет Email через API MailGun
    /// </summary>
    /// <param name="recipient">Email получателя</param>
    /// <param name="subject">Тема письма</param>
    /// <param name="htmlContent">HTML контент письма</param>
    /// <param name="token">Токен отмены для отслеживания отмены операции.</param>
    private async Task SendEmailBySmtpAsync(string recipient, string subject, string htmlContent, CancellationToken token = default)
    {
        try
        {
            // создаем структуру сообщения
            var message = new MimeMessage();

            // отправитель сообщения
            message.From.Add(new MailboxAddress(smtpConfiguration.DisplayedName, smtpConfiguration.Login));

            // адресат сообщения
            message.To.Add(new MailboxAddress("Customer", recipient));

            // тема сообщения
            message.Subject = subject;

            // тело сообщения (так же в формате HTML)
            message.Body = new BodyBuilder
                {
                    HtmlBody = htmlContent
                }
                .ToMessageBody();

            // инициализируем клиент smtp
            using var client = new MailKit.Net.Smtp.SmtpClient();

            // либо используем порт 465
            await client.ConnectAsync(smtpConfiguration.Host, smtpConfiguration.Port, true, token);

            // Аутентифицируемся с помощью логина и пароля
            await client.AuthenticateAsync(smtpConfiguration.Login, smtpConfiguration.Password, token);

            // Асинхронно отправьте указанное сообщение
            await client.SendAsync(message, token);

            // Асинхронно отключить сервис
            await client.DisconnectAsync(true, token);
        }
        catch (Exception exception) when(exception is not OperationCanceledException)
        {
            // Инкапсулируем полученное исключение в EmailException и вызываем его дальше
            throw new EmailSendException(exception);
        }
    }
}