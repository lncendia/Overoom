using Overoom.Domain.Ordering.Abstractions;

namespace Overoom.Domain.Ordering;

public class RandomOrder<T, TVisitor> : IOrderBy<T, TVisitor>
    where TVisitor : ISortingVisitor<TVisitor, T>
{
    public IEnumerable<T> Order(IEnumerable<T> items) => items.OrderBy(_ => Guid.NewGuid());

    public IList<IEnumerable<T>> Divide(IEnumerable<T> items) =>
        Order(items).Select(x => (IEnumerable<T>)new List<T> { x }).ToList();

    public void Accept(TVisitor visitor) => visitor.Visit(this);
}