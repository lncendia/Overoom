using Common.Domain.Interfaces;
using Rooms.Domain.Rooms;
using Rooms.Domain.Rooms.Specifications.Visitor;

namespace Rooms.Domain.Repositories;

public interface IRoomRepository : IRepository<Room, Guid, IRoomSpecificationVisitor>;