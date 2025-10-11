using Common.Domain.Interfaces;
using Films.Domain.Rooms;
using Films.Domain.Rooms.Specifications.Visitor;

namespace Films.Domain.Repositories;

public interface IRoomRepository : IRepository<Room, Guid, IRoomSpecificationVisitor>;