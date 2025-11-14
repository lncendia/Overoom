using Common.Application.EmailService;

namespace Identix.Application.Abstractions.Emails;

/// <summary>
/// Класс, представляющий данные для электронного письма с подтверждением сброса 2фа.
/// </summary>
public class TwoFactorCodeEmail : EmailMessage
{
    /// <summary>
    /// Код подтверждения 2фа.
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    /// Метод, позволяющий посетителю IEmailVisitor посетить текущий объект ConfirmResetTwoFactorEmail и выполнить соответствующие действия.
    /// </summary>
    /// <param name="visitor">Посетитель IEmailVisitor.</param>
    public override void Accept(IEmailVisitor visitor) => visitor.Extended<IExtendedEmailVisitor>().Visit(this);
}