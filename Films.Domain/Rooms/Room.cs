using System.Text;
using Common.Domain.Aggregates;
using Films.Domain.Films;
using Films.Domain.Rooms.Events;
using Films.Domain.Rooms.Exceptions;
using Films.Domain.Users;

namespace Films.Domain.Rooms;

/// <summary> 
/// Класс, представляющий комнату для совместного просмотра фильмов.
/// </summary> 
public partial class Room : AggregateRoot
{
    /// <summary>
    /// Максимально допустимое число зрителей в комнате
    /// </summary>
    private const int MaxViewersCount = 10;

    /// <summary>
    /// Инициализирует новый экземпляр класса Room.
    /// </summary>
    /// <param name="id">Уникальный идентификатор комнаты.</param>
    /// <param name="user">Пользователь, создающий комнату.</param>
    /// <param name="film">Фильм для просмотра в комнате.</param>
    /// <param name="isOpen">Флаг, указывающий является ли комната открытой.</param>
    public Room(Guid id, User user, Film film, bool isOpen) : base(id)
    {
        // Если комната закрытая, генерируем секретный код
        if (!isOpen) Code = GenerateRandomCode(5);

        // Устанавливаем идентификатор фильма
        FilmId = film.Id;

        // Добавляем создателя комнаты в список зрителей
        _viewers.Add(user.Id);

        // Устанавливаем владельца комнаты
        OwnerId = user.Id;

        // Устанавливаем дату создания комнаты
        CreatedAt = DateTime.UtcNow;

        // Если нельзя создать комнату с этим фильмом
        if (!film.CanCreateRoom) throw new FilmNotAvailableException(film.Id);

        // Добавляем событие
        AddDomainEvent(new RoomCreatedEvent(this, user, film));
    }

    /// <summary> 
    /// Получает идентификатор фильма, который транслируется в комнате.
    /// </summary> 
    /// <value>Guid идентификатор фильма.</value>
    public Guid FilmId { get; }

    /// <summary>
    /// Получает секретный код для доступа в комнату.
    /// </summary>
    /// <value>5-символьный код или null, если комната открытая.</value>
    public string? Code { get; }

    /// <summary>
    /// Получает дату и время создания комнаты.
    /// </summary>
    /// <value>Дата и время в UTC.</value>
    public DateTime CreatedAt { get; }

    /// <summary>
    /// Владелец комнаты.
    /// </summary>
    public Guid OwnerId { get; }

    /// <summary>
    /// Список идентификаторов пользователей, находящихся в комнате.
    /// </summary>
    private readonly HashSet<Guid> _viewers = [];

    /// <summary>
    /// Список идентификаторов заблокированных пользователей.
    /// </summary>
    private readonly HashSet<Guid> _bannedUsers = [];

    /// <summary> 
    /// Получает коллекцию идентификаторов пользователей в комнате.
    /// </summary> 
    /// <value>Доступная только для чтения коллекция Guid.</value>
    public IReadOnlySet<Guid> Viewers => _viewers;

    /// <summary> 
    /// Получает коллекцию идентификаторов заблокированных пользователей.
    /// </summary> 
    /// <value>Доступная только для чтения коллекция Guid.</value>
    public IReadOnlySet<Guid> BannedUsers => _bannedUsers;

    /// <summary>
    /// Подключает пользователя к комнате.
    /// </summary>
    /// <param name="user">Пользователь для подключения.</param>
    /// <param name="code">Код доступа (требуется для закрытых комнат).</param>
    /// <exception cref="NotImplementedException">Пользователь уже находится в комнате.</exception>
    /// <exception cref="InvalidCodeException">Неверный код доступа.</exception>
    /// <exception cref="UserBannedInRoomException">Пользователь заблокирован в этой комнате.</exception>
    /// <exception cref="RoomIsFullException">В комнате достигнут лимит пользователей (10).</exception>
    public void Join(User user, string? code)
    {
        // Проверка, что пользователь уже не находится в комнате
        if (_viewers.Contains(user.Id)) throw new NotImplementedException();

        // Если комната закрытая, проверяем код доступа
        if (Code != null)
        {
            if (!string.Equals(Code, code, StringComparison.CurrentCultureIgnoreCase)) throw new InvalidCodeException(Id);
        }

        // Проверка, что пользователь не заблокирован
        if (_bannedUsers.Contains(user.Id)) throw new UserBannedInRoomException(user.Id, Id);

        // Проверка, что в комнате есть свободные места
        if (_viewers.Count >= MaxViewersCount) throw new RoomIsFullException(Id);

        // Добавляем пользователя в список зрителей
        _viewers.Add(user.Id);

        // Добавляем событие
        AddDomainEvent(new ViewerJoinedEvent(this, user));
    }

    /// <summary>
    /// Отключает пользователя от комнаты.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя для отключения.</param>
    public void Leave(Guid userId)
    {
        // Если это владелец
        if (userId == OwnerId) throw new ActionNotAllowedException(Id, nameof(Leave));
        
        _viewers.Remove(userId);

        AddDomainEvent(new ViewerLeavedEvent(this, userId));
    }

    /// <summary>
    /// Блокирует пользователя в комнате.
    /// </summary>
    /// <param name="initiatorId">Инициатор действия.</param>
    /// <param name="userId">Идентификатор пользователя для блокировки.</param>
    /// <remarks>
    /// Заблокированный пользователь будет удален из комнаты и не сможет подключиться снова.
    /// </remarks>
    public void Kick(Guid initiatorId, Guid userId)
    {
        // Если это не владелец
        if (initiatorId != OwnerId || initiatorId == userId) throw new ActionNotAllowedException(Id, nameof(Kick));

        // Добавляем пользователя в список заблокированных
        _bannedUsers.Add(userId);

        // Удаляем пользователя из списка зрителей
        _viewers.Remove(userId);

        AddDomainEvent(new ViewerKickedEvent(this, userId));
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="userId"></param>
    /// <exception cref="ActionNotAllowedException"></exception>
    public void CanDelete(Guid userId)
    {
        if (userId != OwnerId) throw new ActionNotAllowedException(Id, nameof(CanDelete));
    }

    /// <summary>
    /// Генерирует случайный код указанной длины.
    /// </summary>
    /// <param name="length">Длина генерируемого кода.</param>
    /// <returns>Строка, состоящая из случайных символов A-Z и 0-9.</returns>
    private static string GenerateRandomCode(int length)
    {
        // Строка с допустимыми символами для кода
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        // StringBuilder для построения кода
        var sb = new StringBuilder();

        // Создаем генератор случайных чисел
        var random = new Random();

        // Генерируем код заданной длины
        for (var i = 0; i < length; i++)
        {
            // Выбираем случайный индекс символа
            var index = random.Next(chars.Length);

            // Добавляем выбранный символ к коду
            sb.Append(chars[index]);
        }

        // Возвращаем сгенерированный код
        return sb.ToString();
    }
}