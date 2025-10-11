using System.Linq.Expressions;

namespace Common.Infrastructure.Repositories.Visitors;

/// <summary>
/// Класс ReplaceParameterVisitor выполняет замену одного параметра дерева выражений на другой.
/// Наследуется от ExpressionVisitor для реализации обхода узлов дерева выражений.
/// </summary>
/// <param name="oldParam">Старый параметр, который требуется заменить в выражении.</param>
/// <param name="newParam">Новый параметр, который будет подставлен вместо старого.</param>
public class ReplaceParameterVisitor(ParameterExpression oldParam, ParameterExpression newParam) : ExpressionVisitor
{
    /// <summary>
    /// Переопределенный метод VisitParameter, который вызывается для обработки узлов-параметров дерева выражений.
    /// Проверяет, совпадает ли текущий узел с указанным старым параметром, и выполняет замену на новый параметр.
    /// </summary>
    /// <param name="node">Узел параметра (ParameterExpression) текущего дерева выражений.</param>
    /// <returns>Возвращает либо новый параметр, если произошла замена, либо узел без изменений.</returns>
    protected override Expression VisitParameter(ParameterExpression node)
    {
        // Проверяем, совпадает ли текущий узел с заданным старым параметром (_oldParam).
        // Если совпадает, возвращаем новый параметр (_newParam).
        // Если не совпадает, вызываем базовую реализацию VisitParameter, чтобы продолжить обход дерева.
        return node == oldParam ? newParam : base.VisitParameter(node);
    }
}