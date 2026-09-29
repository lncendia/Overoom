namespace Rooms.Application.Abstractions;

/// <summary>
/// Константы, используемые в приложении
/// </summary>
public static class Constants
{
  /// <summary>
  /// Константы для работы с ScopedDictionary
  /// </summary>
  public static class ScopedDictionary
  {
    /// <summary>
    /// Ключ для хранения идентификатора подключения в словаре
    /// </summary>
    public const string CurrentConnectionIdKey = "ConnectionId";
  }

  /// <summary>
  /// Константы для OpenTelemetry
  /// </summary>
  public static class OpenTelemetry
  {
    /// <summary>
    /// Имя сервиса для трассировки
    /// </summary>
    public const string ServiceName = "rooms";
  }

  /// <summary>
  /// Параметры статистики зрителя для сбора и анализа данных о поведении
  /// </summary>
  public static class ViewerStatisticParameters
  {
    /// <summary>Количество отправленных сообщений в чате</summary>
    public const string MessagesCount = "MessagesCount";

    /// <summary>Количество отправленных звуковых сигналов (бипов)</summary>
    public const string BeepCount = "BeepCount";

    /// <summary>Количество полученных звуковых сигналов (бипов)</summary>
    public const string BeepedCount = "BeeppedCount";

    /// <summary>Количество отправленных "криков"</summary>
    public const string ScreamCount = "ScreamCount";

    /// <summary>Количество полученных "криков"</summary>
    public const string ScreamedCount = "ScreamedCount";

    /// <summary>Количество перемоток видео</summary>
    public const string SeekCount = "SeekCount";

    /// <summary>Количество поставленных пауз</summary>
    public const string PauseCount = "PauseCount";

    /// <summary>Количество смен эпизодов</summary>
    public const string EpisodeChangeCount = "EpisodeChangeCount";

    /// <summary>Количество отключений от комнаты</summary>
    public const string DisconnectCount = "DisconnectCount";
  }

  /// <summary>
  /// Статический класс, содержащий предопределенные теги для зрителей
  /// </summary>
  public static class ViewerTags
  {
    /// <summary>Тег для зрителя, который отстает от ведущего</summary>
    public const string Turtle = "🐢 Тормоз";

    /// <summary>Тег для зрителя с именем не на кириллице</summary>
    public const string ForeignName = "🕵️‍♂️ Иноагент";

    /// <summary>Тег для активного участника чата</summary>
    public const string Chatter = "🗯️ Болтун";

    /// <summary>Тег для чрезвычайно активного участника чата</summary>
    public const string ChatterOverdrive = "🤖 Чат-террорист";

    /// <summary>Тег для зрителя, смотрящего в полноэкранном режиме</summary>
    public const string Fullscreener = "🖥️ Иммерсивщик";

    /// <summary>Тег для зрителя, смотрящего не ту серию</summary>
    public const string WrongEpisode = "🧭 Потеряшка";

    /// <summary>Тег для зрителя, поставившего видео на паузу</summary>
    public const string OnPause = "⏸️ Завис";

    /// <summary>Тег для зрителя, ушедшего вперед по времени просмотра</summary>
    public const string OffSyncLeader = "🏃 Ушёл в отрыв";

    /// <summary>Тег для зрителя без аватарки</summary>
    public const string NoAvatar = "👤 Серый кардинал";

    /// <summary>Тег для зрителя с выключенным звуком</summary>
    public const string Muted = "🙉 Глухонемой";

    /// <summary>Тег для зрителя, часто перематывающего видео</summary>
    public const string Seeker = "⏩ Перемотчик";

    /// <summary>Тег для зрителя, часто ставящего на паузу</summary>
    public const string PauseMaster = "⏱️ Стоп-Хам";

    /// <summary>Тег для зрителя, часто меняющего серии</summary>
    public const string EpisodeHopper = "🔄 Сериеброд";

    /// <summary>Тег для зрителя, который всегда первый в комнате</summary>
    public const string FirstIn = "🎟️ Премиум";

    /// <summary>Тег для зрителя, смотрящего в замедленном режиме</summary>
    public const string SlowWatcher = "🐌 Замедленный";

    /// <summary>Тег для зрителя, смотрящего в ускоренном режиме</summary>
    public const string FastWatcher = "⚡ Скорострел";

    /// <summary>Тег для ведущего комнаты просмотра</summary>
    public const string Host = "🧑‍✈️ Ведущий";

    /// <summary>Тег для зрителя, который любит кидать скримеры</summary>
    public const string Screamer = "👻 Скример";

    /// <summary>Тег для зрителя, который постоянно страдает от скримеров</summary>
    public const string Screamed = "😱 Пугливый";

    /// <summary>Тег для зрителя, который обожает бипать всех подряд</summary>
    public const string Beeper = "📢 Бипер";

    /// <summary>Тег для зрителя, который достал остальных своими бипами</summary>
    public const string BeepVictim = "🙉 Забибиканный";

    /// <summary>Тег для зрителя, который слишком часто выходит из комнаты</summary>
    public const string Leaver = "🚪 Исчезающий";

    /// <summary>
    /// Словарь всех тегов с их описаниями
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string> All = new Dictionary<string, string>
    {
      [Turtle] = "Отстаёт от ведущего более чем на 5 минут",
      [ForeignName] = "Имя написано не кириллицей. Под подозрением.",
      [Chatter] = "Отправил более 50 сообщений. Может стоит посмотреть фильм?",
      [ChatterOverdrive] = "100+ сообщений за сессию. Возможно, это чат-бот.",
      [Fullscreener] = "Смотрит в полноэкранном режиме. Уважение.",
      [WrongEpisode] = "Смотрит не ту серию, что все остальные. Вернись в поток.",
      [OnPause] = "У него пауза, а фильм давно идёт. Напомнить?",
      [OffSyncLeader] = "Смотрит, пока все на паузе. Нетерпеливый!",
      [NoAvatar] = "Нет аватарки. Кто ты, воин?",
      [Muted] = "Громкость видео на нуле. Он, наверное, читает по губам.",
      [Seeker] = "Мотает так, будто ищет сцену после титров. Но титры ещё не начинались.",
      [PauseMaster] = "Ставил видео на паузу более 30 раз. У него свои ритмы.",
      [EpisodeHopper] = "Меняет серии чаще, чем кто-либо. Что он ищет?",
      [FirstIn] = "Всегда первый в комнате. Легенда.",
      [SlowWatcher] = "Смотрит в замедленном. Наслаждается каждым пикселем.",
      [FastWatcher] = "Смотрит в ускоренном. Сериал – это просто сюжетная выжимка.",
      [Host] = "Тот, кто рулит просмотром. Наш капитан.",
      [Screamer] = "Кидает скримеры при каждом удобном случае. Живёт ради чужого испуга.",
      [Screamed] = "Судьба жестока — все скримеры достаются именно ему.",
      [Beeper] = "Бипает чаще, чем дышит. Всегда напомнит о себе.",
      [BeepVictim] = "Постоянно получает бипы. Когда же его оставят в покое?",
      [Leaver] = "Постоянно выходит из комнаты. Кажется, у него непостоянные отношения с интернетом."
    };
  }
}