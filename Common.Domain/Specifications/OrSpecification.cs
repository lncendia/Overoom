using Common.Domain.Specifications.Abstractions;

namespace Common.Domain.Specifications;

/// <summary>
/// Спецификация, представляющая логическое ИЛИ между двумя другими спецификациями
/// </summary>
/// <typeparam name="T">Тип объекта для спецификации</typeparam>
/// <typeparam name="TVisitor">Тип посетителя спецификации</typeparam>
public class OrSpecification<T, TVisitor>(ISpecification<T, TVisitor> left, ISpecification<T, TVisitor> right)
    : ISpecification<T, TVisitor>
    where TVisitor : ISpecificationVisitor<TVisitor, T>
{
    /// <summary>
    /// Левая спецификация
    /// </summary>
    public ISpecification<T, TVisitor> Left { get; } = left;
    
    /// <summary>
    /// Правая спецификация
    /// </summary>
    public ISpecification<T, TVisitor> Right { get; } = right;

    /// <summary>
    /// Принимает посетителя для обработки спецификации
    /// </summary>
    /// <param name="visitor">Посетитель спецификации</param>
    public void Accept(TVisitor visitor) => visitor.Visit(this);
    
    /// <summary>
    /// Проверяет удовлетворяет ли объект условиям спецификации
    /// </summary>
    /// <param name="obj">Проверяемый объект</param>
    /// <returns>True если объект удовлетворяет условиям хотя бы одной спецификации, иначе False</returns>
    public bool IsSatisfiedBy(T obj) => Left.IsSatisfiedBy(obj) || Right.IsSatisfiedBy(obj);
}