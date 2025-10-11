namespace Common.Application.EmailService;

/// <summary>
/// Расширения для работы с посетителями Email
/// Предоставляет удобные методы для работы с различными типами email-сообщений
/// </summary>
public static class EmailExtensions
{
    /// <summary>
    /// Приводит посетителя к указанному типу
    /// Позволяет получить доступ к специфическим свойствам конкретного типа email
    /// </summary>
    /// <typeparam name="T">Тип, к которому приводится посетитель</typeparam>
    /// <param name="visitor">Экземпляр посетителя</param>
    /// <returns>Приведенный к типу T посетитель</returns>
    public static T Extended<T>(this IEmailVisitor visitor)
    {
        return (T)visitor;
    }
}