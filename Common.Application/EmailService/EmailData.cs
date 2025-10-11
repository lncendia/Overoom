namespace Common.Application.EmailService;

/// <summary>
/// Объект данных об отправляемом Email
/// </summary>
public abstract class EmailData
{
    /// <summary>
    /// Email адрес получателя
    /// </summary>
    public required string Recipient { get; init; }

    /// <summary>
    /// Абстрактный метод, который позволяет посетителю IEmailVisitor посетить объект и выполнить соответствующие действия.
    /// </summary>
    /// <param name="visitor">Посетитель IEmailVisitor.</param>
    public abstract void Accept(IEmailVisitor visitor);
}