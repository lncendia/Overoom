using Common.Domain.Interfaces;
using Films.Domain.Playlists;
using Films.Domain.Playlists.Specifications.Visitor;

namespace Films.Domain.Repositories;

public interface IPlaylistRepository : IRepository<Playlist, Guid, IPlaylistSpecificationVisitor>;