using Overoom.Domain.Comments.Ordering.Visitor;
using Overoom.Domain.Ordering.Abstractions;

namespace Overoom.Domain.Comments.Ordering;

public class OrderByDate : IOrderBy<Comment, ICommentSortingVisitor>
{
    public IEnumerable<Comment> Order(IEnumerable<Comment> items) => items.OrderBy(x => x.CreatedAt);

    public IList<IEnumerable<Comment>> Divide(IEnumerable<Comment> items) =>
        Order(items).GroupBy(x => x.CreatedAt).Select(x => x.AsEnumerable()).ToList();

    public void Accept(ICommentSortingVisitor visitor) => visitor.Visit(this);
}