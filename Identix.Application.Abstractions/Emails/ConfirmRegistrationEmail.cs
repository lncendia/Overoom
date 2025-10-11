using Common.Application.EmailService;

namespace Identix.Application.Abstractions.Emails;

/// <summary>
/// Класс, представляющий данные для электронного письма с подтверждением регистрации.
/// </summary>
public class ConfirmRegistrationEmail : EmailData
{
    /// <summary>
    /// Ссылка для подтверждения регистрации.
    /// </summary>
    public required string ConfirmLink { get; init; }

    /// <summary>
    /// Метод, позволяющий посетителю IEmailVisitor посетить текущий объект ConfirmRegistrationEmail и выполнить соответствующие действия.
    /// </summary>
    /// <param name="visitor">Посетитель IEmailVisitor.</param>
    public override void Accept(IEmailVisitor visitor) => visitor.Extended<IExtendedEmailVisitor>().Visit(this);
}