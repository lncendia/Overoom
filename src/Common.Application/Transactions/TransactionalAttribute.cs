namespace Common.Application.Transactions;

/// <summary>
/// Помечает точку входа (действие контроллера, метод хаба), которая должна выполняться в одной транзакции MongoDB.
/// </summary>
/// <remarks>
/// Транзакция стартует до вызова обработчика и фиксируется после его успешного завершения. Все чтения и сохранения
/// репозиториев, а также сообщения, опубликованные через outbox, попадают в неё. Обработчики команд о транзакции
/// не знают: границу задаёт вызывающая сторона.
/// </remarks>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class TransactionalAttribute : Attribute;
