namespace Rooms.Domain.Messages;

/// <summary>
/// Допустимые реакции на сообщения.
/// </summary>
/// <remarks>
/// Хранятся кодами, а не эмодзи: код стабилен, безопасен как имя поля документа и не зависит от отображения.
/// </remarks>
public static class MessageReactions
{
  public const string Like = "like";
  public const string Love = "love";
  public const string Laugh = "laugh";
  public const string Wow = "wow";
  public const string Sad = "sad";
  public const string Fire = "fire";

  /// <summary>
  /// Все допустимые коды реакций.
  /// </summary>
  public static IReadOnlySet<string> All { get; } = new HashSet<string> { Like, Love, Laugh, Wow, Sad, Fire };
}
