using System.Linq.Expressions;
using Common.Domain.Specifications;
using Common.Domain.Specifications.Abstractions;

namespace Common.Infrastructure.Repositories.Visitors;

/// <inheritdoc cref="ISpecificationVisitor{TVisitor,T}"/>
/// <summary>
/// Реализация посетителя спецификации.
/// </summary>
public abstract class BaseSpecificationVisitor<TEntity, TVisitor, TItem>
    where TVisitor : ISpecificationVisitor<TVisitor, TItem>
{
    /// <summary>
    /// Выражение для запроса к ef.
    /// </summary>
    public Expression<Func<TEntity, bool>>? Expr { get; protected set; }

    /// <summary>
    /// Конвертирует спецификацию в Expression.
    /// </summary>
    /// <param name="spec">Спецификация</param>
    protected abstract Expression<Func<TEntity, bool>> ConvertSpecToExpression(ISpecification<TItem, TVisitor> spec);

    /// <inheritdoc cref="ISpecificationVisitor{TVisitor,T}"/>
    /// <summary>
    /// Посещает объект с условием "И".
    /// </summary>
    public void Visit(AndSpecification<TItem, TVisitor> spec)
    {
        // Преобразование левой спецификации (условия) в выражение.
        // Метод ConvertSpecToExpression принимает правило (или спецификацию),
        // указанное в левой части, и преобразует его в Expression<Func<TEntity, bool>>.
        var leftExpr = ConvertSpecToExpression(spec.Left);
        
        // Аналогично, правая спецификация преобразуется в выражение.
        // Это позволяет нам работать с условиями в виде логических выражений.+
        var rightExpr = ConvertSpecToExpression(spec.Right);

        // Создаем общий параметр для объекта TEntity, который будет
        // использоваться в объединении выражений. Это необходимо, так как
        // логические выражения требуют общего пространства параметров.
        var param = Expression.Parameter(typeof(TEntity), "x");

        // Подменяем параметр в левой части выражения на общий параметр (param).
        // ReplaceParameterVisitor — это вспомогательный объект, позволяющий заменить
        // параметры в теле выражения.
        var leftBody = new ReplaceParameterVisitor(leftExpr.Parameters.Single(), param).Visit(leftExpr.Body);

        // Аналогично, заменяем параметры в правой части на общий параметр.
        var rightBody = new ReplaceParameterVisitor(rightExpr.Parameters.Single(), param).Visit(rightExpr.Body);

        // Создаем новое логическое выражение, объединяя обе части с помощью оператора "И" (AndAlso).
        // AndAlso — это логическое И, выражающее "выполняется оба условия".
        var exprBody = Expression.AndAlso(leftBody, rightBody);

        // Создаем лямбда-выражение типа Expression<Func<TEntity, bool>>.
        // Lambda связывает логическое выражение (exprBody) с параметром (param), создавая финальное условие.
        Expr = Expression.Lambda<Func<TEntity, bool>>(exprBody, param);
    }

    /// <inheritdoc cref="ISpecificationVisitor{TVisitor,T}"/>
    /// <summary>
    /// Посещает объект с условием "ИЛИ".
    /// </summary>
    public void Visit(OrSpecification<TItem, TVisitor> spec)
    {
        // Преобразование левой спецификации (условия) в выражение.
        // Здесь левая часть правила преобразуется в Expression<Func<TEntity, bool>>.
        var leftExpr = ConvertSpecToExpression(spec.Left);

        // Преобразование правой спецификации (условия) в выражение.
        // Это аналогично предыдущему процессу, но обрабатываем правую часть логики.
        var rightExpr = ConvertSpecToExpression(spec.Right);

        // Создаем общий параметр для объекта TEntity, чтобы обеспечить использование
        // одного параметра (переменной) в обеих частях выражения.
        var param = Expression.Parameter(typeof(TEntity), "x");

        // Заменяем оригинальный параметр в левой части на общий параметр (param).
        // Это необходимо, чтобы оба выражения могли быть объединены в одно логическое выражение.
        var leftBody = new ReplaceParameterVisitor(leftExpr.Parameters.Single(), param).Visit(leftExpr.Body);

        // Заменяем параметр в правой части выражения на общий параметр.
        var rightBody = new ReplaceParameterVisitor(rightExpr.Parameters.Single(), param).Visit(rightExpr.Body);

        // Создаем логическое выражение с оператором "ИЛИ" (Or).
        // Or возвращает true, если выполняется хотя бы одно из указанных условий.
        var exprBody = Expression.Or(leftBody, rightBody);

        // Создаем лямбда-выражение, связывающее объединенное выражение (exprBody) с параметром (param).
        // Это финальная форма логического правила.
        Expr = Expression.Lambda<Func<TEntity, bool>>(exprBody, param);
    }

    /// <inheritdoc cref="ISpecificationVisitor{TVisitor,T}"/>
    /// <summary>
    /// Посещает объект с условием "НЕ".
    /// </summary>
    public void Visit(NotSpecification<TItem, TVisitor> spec)
    {
        // Преобразование спецификации в выражение
        var specExpr = ConvertSpecToExpression(spec.Specification);

        // Создание тела выражения с оператором Not
        var exprBody = Expression.Not(specExpr.Body);

        // Создание лямбда выражения для типа TEntity
        Expr = Expression.Lambda<Func<TEntity, bool>>(exprBody, specExpr.Parameters.Single());
    }
}