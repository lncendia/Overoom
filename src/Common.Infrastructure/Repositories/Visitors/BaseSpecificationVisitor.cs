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
    Expression<Func<TEntity, bool>> leftExpr = ConvertSpecToExpression(spec.Left);
    Expression<Func<TEntity, bool>> rightExpr = ConvertSpecToExpression(spec.Right);
    ParameterExpression param = Expression.Parameter(typeof(TEntity), "x");
    Expression leftBody = new ReplaceParameterVisitor(leftExpr.Parameters.Single(), param).Visit(leftExpr.Body);
    Expression rightBody = new ReplaceParameterVisitor(rightExpr.Parameters.Single(), param).Visit(rightExpr.Body);
    BinaryExpression exprBody = Expression.AndAlso(leftBody, rightBody);
    Expr = Expression.Lambda<Func<TEntity, bool>>(exprBody, param);
  }

  /// <inheritdoc cref="ISpecificationVisitor{TVisitor,T}"/>
  /// <summary>
  /// Посещает объект с условием "ИЛИ".
  /// </summary>
  public void Visit(OrSpecification<TItem, TVisitor> spec)
  {
    Expression<Func<TEntity, bool>> leftExpr = ConvertSpecToExpression(spec.Left);
    Expression<Func<TEntity, bool>> rightExpr = ConvertSpecToExpression(spec.Right);
    ParameterExpression param = Expression.Parameter(typeof(TEntity), "x");
    Expression leftBody = new ReplaceParameterVisitor(leftExpr.Parameters.Single(), param).Visit(leftExpr.Body);
    Expression rightBody = new ReplaceParameterVisitor(rightExpr.Parameters.Single(), param).Visit(rightExpr.Body);
    BinaryExpression exprBody = Expression.Or(leftBody, rightBody);
    Expr = Expression.Lambda<Func<TEntity, bool>>(exprBody, param);
  }

  /// <inheritdoc cref="ISpecificationVisitor{TVisitor,T}"/>
  /// <summary>
  /// Посещает объект с условием "НЕ".
  /// </summary>
  public void Visit(NotSpecification<TItem, TVisitor> spec)
  {
    Expression<Func<TEntity, bool>> specExpr = ConvertSpecToExpression(spec.Specification);
    UnaryExpression exprBody = Expression.Not(specExpr.Body);
    Expr = Expression.Lambda<Func<TEntity, bool>>(exprBody, specExpr.Parameters.Single());
  }
}