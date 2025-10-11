namespace Rooms.Domain.Rooms.Exceptions;

public class ActionNotAllowedException(string action) : Exception("The user has forbidden this action")
{
    public string Action { get; } = action;
}