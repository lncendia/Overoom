using Common.Domain.Specifications.Abstractions;

namespace Common.Domain.Specifications;

/// <summary>
/// Спецификация, представляющая логическое НЕ для другой спецификации
/// </summary>
/// <typeparam name="T">Тип объекта для спецификации</typeparam>
/// <typeparam name="TVisitor">Тип посетителя спецификации</typeparam>
public class NotSpecification<T, TVisitor>(ISpecification<T, TVisitor> specification) : ISpecification<T, TVisitor>
    where TVisitor : ISpecificationVisitor<TVisitor, T>
{
    /// <summary>
    /// Исходная спецификация
    /// </summary>
    public ISpecification<T, TVisitor> Specification { get; } = specification;

    /// <summary>
    /// Принимает посетителя для обработки спецификации
    /// </summary>
    /// <param name="visitor">Посетитель спецификации</param>
    public void Accept(TVisitor visitor) => visitor.Visit(this);
    
    /// <summary>
    /// Проверяет удовлетворяет ли объект условиям спецификации
    /// </summary>
    /// <param name="obj">Проверяемый объект</param>
    /// <returns>True если объект не удовлетворяет условиям исходной спецификации, иначе False</returns>
    public bool IsSatisfiedBy(T obj) => !Specification.IsSatisfiedBy(obj);
}