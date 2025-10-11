namespace Films.Domain.Films.Exceptions;

/// <summary>
/// Исключение, которое вызывается, когда коллекция тегов пуста.
/// </summary>
public class EmptyTagsCollectionException : Exception
{
    /// <summary>
    /// Название коллекции тегов, которая оказалась пустой.
    /// </summary>
    public string CollectionName { get; }

    /// <summary>
    /// Конструктор исключения.
    /// </summary>
    /// <param name="collectionName">Название коллекции тегов.</param>
    public EmptyTagsCollectionException(string collectionName) 
        : base($"The tag collection '{collectionName}' cannot be empty.")
    {
        CollectionName = collectionName;
    }
}