namespace Common.Domain.Specifications.Abstractions;

/// <summary>
/// Интерфейс для спецификаций доменной модели
/// </summary>
/// <typeparam name="T">Тип объекта для спецификации</typeparam>
/// <typeparam name="TVisitor">Тип посетителя спецификации</typeparam>
public interface ISpecification <in T, in TVisitor>  where TVisitor : ISpecificationVisitor<TVisitor, T>
{
    /// <summary>
    /// Проверяет удовлетворяет ли объект условиям спецификации
    /// </summary>
    /// <param name="item">Проверяемый объект</param>
    /// <returns>True если объект удовлетворяет условиям, иначе False</returns>
    bool IsSatisfiedBy(T item);
    
    /// <summary>
    /// Принимает посетителя для обработки спецификации
    /// </summary>
    /// <param name="visitor">Посетитель спецификации</param>
    void Accept (TVisitor visitor);
}