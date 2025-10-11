using System.Reflection;
using Rooms.Domain.Messages.Snapshots;

namespace Rooms.Domain.Messages;

/// <summary>
/// Представляет текстовое сообщение в комнате.
/// </summary>
public partial class Message
{
    /// <summary>
    /// Внутренний конструктор для гидратации из снапшота или БД.
    /// </summary>
    // ReSharper disable once UnusedMember.Local
    private Message(MessageSnapshot snapshot) : base(snapshot.Id)
    {
        RoomId = snapshot.RoomId;
        UserId = snapshot.UserId;
        Text = snapshot.Text;
        SentAt = snapshot.SentAt;
    }

    /// <summary>
    /// Создаёт снапшот текущего состояния агрегата.
    /// </summary>
    internal MessageSnapshot GetSnapshot() => new()
    {
        Id = Id,
        RoomId = RoomId,
        UserId = UserId,
        Text = Text,
        SentAt = SentAt
    };

    /// <summary>
    /// Воссоздаёт агрегат Message из снапшота.
    /// </summary>
    internal static Message FromSnapshot(MessageSnapshot snapshot)
    {
        // Получаем тип Message
        var filmType = typeof(Message);
    
        // Получаем внутренний конструктор, который принимает MessageSnapshot
        var constructor = filmType.GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            [typeof(MessageSnapshot)],
            null);
    
        // Вызываем конструктор и возвращаем результат
        return (Message)constructor!.Invoke([snapshot]);
    }
}
