using Common.Application.EmailService;

namespace Identix.Application.Abstractions.Emails;

/// <summary>
/// Класс, представляющий данные для электронного письма с подтверждением восстановления пароля.
/// </summary>
public class ConfirmRecoverPasswordEmail : EmailMessage
{
    /// <summary>
    /// Ссылка для подтверждения восстановления пароля.
    /// </summary>
    public required string ConfirmLink { get; init; }

    /// <summary>
    /// Метод, позволяющий посетителю IEmailVisitor посетить текущий объект ConfirmRecoverPasswordEmail и выполнить соответствующие действия.
    /// </summary>
    /// <param name="visitor">Посетитель IEmailVisitor.</param>
    public override void Accept(IEmailVisitor visitor) => visitor.Extended<IExtendedEmailVisitor>().Visit(this);
}