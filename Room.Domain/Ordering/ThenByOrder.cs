using Room.Domain.Ordering.Abstractions;

namespace Room.Domain.Ordering;

public class ThenByOrder<T, TVisitor>(IOrderBy<T, TVisitor> left, IOrderBy<T, TVisitor> right) : IOrderBy<T, TVisitor>
    where TVisitor : ISortingVisitor<TVisitor, T>
{
    public IOrderBy<T, TVisitor> Left { get; } = left;
    public IOrderBy<T, TVisitor> Right { get; } = right;

    public IEnumerable<T> Order(IEnumerable<T> items)
    {
        var list = Left.Divide(items);
        return list.SelectMany(Right.Order);
    }

    public IReadOnlyCollection<IEnumerable<T>> Divide(IEnumerable<T> items)
    {
        var list = Left.Divide(items);
        return list.Select(Right.Order).ToArray();
    }

    public void Accept(TVisitor visitor) => visitor.Visit(this);
}