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
    emailData.Accept(visitor);
    if (visitor.Subject is null || visitor.Body is null) return Task.CompletedTask;

    return SendEmailBySmtpAsync(emailData.Recipient, visitor.Subject, visitor.Body, token);
  }

  /// <summary>
  /// Метод отправляет Email через API MailGun
  /// </summary>
  /// <param name="recipient">Email получателя</param>
  /// <param name="subject">Тема письма</param>
  /// <param name="htmlContent">HTML контент письма</param>
  /// <param name="token">Токен отмены для отслеживания отмены операции.</param>
  private async Task SendEmailBySmtpAsync(string recipient, string subject, string htmlContent,
    CancellationToken token = default)
  {
    try
    {
      var message = new MimeMessage();
      message.From.Add(new MailboxAddress(smtpConfiguration.DisplayedName, smtpConfiguration.Login));
      message.To.Add(new MailboxAddress("Customer", recipient));
      message.Subject = subject;

      message.Body = new BodyBuilder
        {
          HtmlBody = htmlContent
        }
        .ToMessageBody();

      using var client = new MailKit.Net.Smtp.SmtpClient();
      await client.ConnectAsync(smtpConfiguration.Host, smtpConfiguration.Port, true, token);
      await client.AuthenticateAsync(smtpConfiguration.Login, smtpConfiguration.Password, token);
      await client.SendAsync(message, token);
      await client.DisconnectAsync(true, token);
    }
    catch (Exception exception) when (exception is not OperationCanceledException)
    {
      throw new EmailSendException(exception);
    }
  }
}