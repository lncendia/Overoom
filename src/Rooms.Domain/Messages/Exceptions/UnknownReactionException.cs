namespace Rooms.Domain.Messages.Exceptions;

/// <summary>
/// Исключение, возникающее при попытке поставить реакцию, которой нет среди допустимых.
/// </summary>
/// <param name="reaction">Код реакции</param>
public class UnknownReactionException(string reaction) : Exception($"Reaction '{reaction}' is not supported")
{
  /// <summary>
  /// Код реакции
  /// </summary>
  public string Reaction { get; } = reaction;
}
