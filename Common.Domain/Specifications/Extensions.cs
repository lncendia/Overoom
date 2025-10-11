using Common.Domain.Specifications.Abstractions;

namespace Common.Domain.Specifications;

/// <summary>
/// Статический класс, содержащий методы расширения для работы со спецификациями.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Создает новую спецификацию, которая является логическим И двух спецификаций.
    /// </summary>
    /// <typeparam name="T">Тип данных, к которому применяется спецификация.</typeparam>
    /// <typeparam name="TV">Тип посетителя спецификации.</typeparam>
    /// <param name="spec1">Первая спецификация.</param>
    /// <param name="spec2">Вторая спецификация.</param>
    /// <returns>Новая спецификация, представляющая логическое И двух спецификаций.</returns>
    public static ISpecification<T, TV> And<T, TV>(this ISpecification<T, TV> spec1, ISpecification<T, TV> spec2)
        where TV : ISpecificationVisitor<TV, T>
    {
        return new AndSpecification<T, TV>(spec1, spec2);
    }

    /// <summary>
    /// Создает новую спецификацию, которая является логическим ИЛИ двух спецификаций.
    /// </summary>
    /// <typeparam name="T">Тип данных, к которому применяется спецификация.</typeparam>
    /// <typeparam name="TV">Тип посетителя спецификации.</typeparam>
    /// <param name="spec1">Первая спецификация.</param>
    /// <param name="spec2">Вторая спецификация.</param>
    /// <returns>Новая спецификация, представляющая логическое ИЛИ двух спецификаций.</returns>
    public static ISpecification<T, TV> Or<T, TV>(this ISpecification<T, TV> spec1, ISpecification<T, TV> spec2)
        where TV : ISpecificationVisitor<TV, T>
    {
        return new OrSpecification<T, TV>(spec1, spec2);
    }

    /// <summary>
    /// Создает новую спецификацию, которая является логическим НЕ одной спецификации.
    /// </summary>
    /// <typeparam name="T">Тип данных, к которому применяется спецификация.</typeparam>
    /// <typeparam name="TV">Тип посетителя спецификации.</typeparam>
    /// <param name="spec">Спецификация, к которой применяется логическое НЕ.</param>
    /// <returns>Новая спецификация, представляющая логическое НЕ одной спецификации.</returns>
    public static ISpecification<T, TV> Not<T, TV>(this ISpecification<T, TV> spec)
        where TV : ISpecificationVisitor<TV, T>
    {
        return new NotSpecification<T, TV>(spec);
    }
}