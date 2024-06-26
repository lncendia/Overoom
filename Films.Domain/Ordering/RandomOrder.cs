using Films.Domain.Ordering.Abstractions;

namespace Films.Domain.Ordering;

public class RandomOrder<T, TVisitor> : IOrderBy<T, TVisitor>
    where TVisitor : ISortingVisitor<TVisitor, T>
{
    public IEnumerable<T> Order(IEnumerable<T> items) => items.OrderBy(_ => Guid.NewGuid());

    public IReadOnlyCollection<IEnumerable<T>> Divide(IEnumerable<T> items) =>
        Order(items).Select(x => (IEnumerable<T>)new List<T> { x }).ToArray();

    public void Accept(TVisitor visitor) => visitor.Visit(this);
}