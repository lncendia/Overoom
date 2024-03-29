﻿using System.Reflection;
using System.Runtime.Serialization;
using Overoom.Domain.Abstractions;
using Overoom.Domain.Comments.Entities;
using Overoom.Infrastructure.Storage.Mappers.Abstractions;
using Overoom.Infrastructure.Storage.Mappers.StaticMethods;
using Overoom.Infrastructure.Storage.Models.Comment;

namespace Overoom.Infrastructure.Storage.Mappers.AggregateMappers;

internal class CommentMapper : IAggregateMapperUnit<Comment, CommentModel>
{
    private static readonly Type CommentType = typeof(Comment);

    private static readonly FieldInfo CreatedAt =
        CommentType.GetField("<CreatedAt>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!;

    private static readonly FieldInfo UserId =
        CommentType.GetField("<UserId>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!;

    private static readonly FieldInfo FilmId =
        CommentType.GetField("<FilmId>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!;

    private static readonly FieldInfo Text =
        CommentType.GetField("<Text>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!;

    public Comment Map(CommentModel model)
    {
        var comment = (Comment)FormatterServices.GetUninitializedObject(CommentType);
        IdFields.AggregateId.SetValue(comment, model.Id);
        CreatedAt.SetValue(comment, model.CreatedAt);
        UserId.SetValue(comment, model.UserId);
        FilmId.SetValue(comment, model.FilmId);
        Text.SetValue(comment, model.Text);
        IdFields.DomainEvents.SetValue(comment, new List<IDomainEvent>());
        return comment;
    }
}