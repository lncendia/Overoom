using Common.Application.EmailService;
using Identix.Application.Abstractions.Emails;
using MassTransit;

namespace Identix.Infrastructure.Bus;

/// <summary>
/// Обработчик события SendEmail
/// </summary>
/// <param name="sender">Сервис отправки электронного письма</param>
public class SendEmailConsumer(IEmailService sender) : IConsumer<SendEmail>
{
    /// <summary>
    /// Метод обработчик 
    /// </summary>
    /// <param name="context">Контекст сообщения</param>
    public async Task Consume(ConsumeContext<SendEmail> context)
    {
        // Отправляем письмо
        await sender.SendAsync(context.Message.Message, context.CancellationToken);
    }
}