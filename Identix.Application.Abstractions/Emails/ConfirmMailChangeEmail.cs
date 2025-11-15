using Common.Application.EmailService;

namespace Identix.Application.Abstractions.Emails;

/// <summary>
/// Класс, представляющий данные для электронного письма с подтверждением изменения электронной почты.
/// </summary>
public class ConfirmMailChangeEmail : EmailMessage
{
    /// <summary>
    /// Ссылка для подтверждения изменения электронной почты.
    /// </summary>
    public required string ConfirmLink { get; init; }

    /// <summary>
    /// Метод, позволяющий посетителю IEmailVisitor посетить текущий объект ConfirmMailChangeEmail и выполнить соответствующие действия.
    /// </summary>
    /// <param name="visitor">Посетитель IEmailVisitor.</param>
    public override void Accept(IEmailVisitor visitor) => visitor.Extended<IExtendedEmailVisitor>().Visit(this);
}