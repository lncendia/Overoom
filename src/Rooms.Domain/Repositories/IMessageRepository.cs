using Common.Domain.Interfaces;
using Rooms.Domain.Messages;
using Rooms.Domain.Messages.Specifications.Visitor;

namespace Rooms.Domain.Repositories;

public interface IMessageRepository : IRepository<Message, Guid, IMessageSpecificationVisitor>;