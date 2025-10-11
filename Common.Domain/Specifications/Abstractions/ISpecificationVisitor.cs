namespace Common.Domain.Specifications.Abstractions;

/// <summary>
/// Интерфейс посетителя для спецификаций
/// </summary>
/// <typeparam name="TVisitor">Тип посетителя</typeparam>
/// <typeparam name="T">Тип объекта для спецификации</typeparam>
public interface ISpecificationVisitor<TVisitor, T>  where TVisitor : ISpecificationVisitor<TVisitor, T>
{
    /// <summary>
    /// Обрабатывает спецификацию И
    /// </summary>
    /// <param name="spec">Спецификация И</param>
    void Visit(AndSpecification<T, TVisitor> spec);
    
    /// <summary>
    /// Обрабатывает спецификацию ИЛИ
    /// </summary>
    /// <param name="spec">Спецификация ИЛИ</param>
    void Visit(OrSpecification<T, TVisitor> spec);
    
    /// <summary>
    /// Обрабатывает спецификацию НЕ
    /// </summary>
    /// <param name="spec">Спецификация НЕ</param>
    void Visit(NotSpecification<T, TVisitor> spec);
}