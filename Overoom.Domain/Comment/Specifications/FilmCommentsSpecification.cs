using Overoom.Domain.Comment.Specifications.Visitor;
using Overoom.Domain.Specifications.Abstractions;

namespace Overoom.Domain.Comment.Specifications;

public class FilmCommentsSpecification : ISpecification<Entities.Comment, ICommentSpecificationVisitor>
{
    public FilmCommentsSpecification(Guid filmId) => FilmId = filmId;

    public Guid FilmId { get; }
    public bool IsSatisfiedBy(Entities.Comment item) => item.FilmId == FilmId;

    public void Accept(ICommentSpecificationVisitor visitor) => visitor.Visit(this);
}