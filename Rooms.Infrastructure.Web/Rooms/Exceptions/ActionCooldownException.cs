namespace Rooms.Infrastructure.Web.Rooms.Exceptions;

/// <summary>
/// Исключение, указывающее на то, что действие недоступно из-за времени восстановления
/// </summary>
public class ActionCooldownException(int seconds) : Exception($"The action will be available in {seconds} seconds")
{
    /// <summary>
    /// Количество секунд до доступности действия
    /// </summary>
    public int Seconds { get; } = seconds;
}