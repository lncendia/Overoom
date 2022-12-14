using System.Reflection;
using System.Runtime.Serialization;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Overoom.Domain.Abstractions.Repositories;
using Overoom.Infrastructure.PersistentStorage.Context;
using Overoom.Infrastructure.PersistentStorage.Models.Rooms;
using Overoom.Infrastructure.PersistentStorage.Visitors.Sorting;
using Overoom.Infrastructure.PersistentStorage.Visitors.Specifications;
using Overoom.Domain.Ordering.Abstractions;
using Overoom.Domain.Rooms.BaseRoom.Entities;
using Overoom.Domain.Rooms.BaseRoom.ValueObject;
using Overoom.Domain.Rooms.YoutubeRoom;
using Overoom.Domain.Rooms.YoutubeRoom.Entities;
using Overoom.Domain.Rooms.YoutubeRoom.Ordering.Visitor;
using Overoom.Domain.Rooms.YoutubeRoom.Specifications.Visitor;
using Overoom.Domain.Specifications.Abstractions;

namespace Overoom.Infrastructure.PersistentStorage.Repositories;

public class YoutubeRoomRepository : IYoutubeRoomRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public YoutubeRoomRepository(ApplicationDbContext context)
    {
        _context = context;
        _mapper = GetMapper();
    }

    private YoutubeRoom GetMap(YoutubeRoomModel model)
    {
        var room = new YoutubeRoom("https://youtu.be/" + model.VideoIds.First().VideoId, "someName", "someAvatar", model.AddAccess);
        _mapper.Map(model, room);
        var type = room.GetType();
        var btype = type.BaseType!;

        var viewersList = model.Viewers.Select(GetMap).OrderBy(viewer => viewer.Name).ToList();

        btype.GetField("<Id>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(room, model.Id);

        var viewers =
            (btype.GetField("ViewersList", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(room) as
                List<Viewer>)!;
        viewers.Clear();
        viewers.AddRange(viewersList);

        type.GetField("<Owner>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(room,
            viewersList.First(viewer => viewer.Id == model.OwnerId));

        btype.GetField("<LastActivity>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(room,
            model.LastActivity);

        var messages =
            (btype.GetField("MessagesList", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(room) as
                List<Message>)!;
        messages.AddRange(model.Messages.Select(GetMap).OrderBy(message => message.CreatedAt));

        var ids =
            (type.GetField("_ids", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(room) as List<string>)!;
        ids.AddRange(model.VideoIds.Skip(1).Select(idModel => idModel.VideoId));

        type.GetField("<AddAccess>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(room,
            model.AddAccess);

        return room;
    }

    private YoutubeViewer GetMap(YoutubeViewerModel model)
    {
        var viewer = _mapper.Map<YoutubeViewer>(model);
        var x = viewer.GetType().BaseType!;
        x.GetField("<Id>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(viewer, model.Id);
        return viewer;
    }

    private Message GetMap(MessageModel model)
    {
        var message = _mapper.Map<Message>(model);
        var x = message.GetType();
        x.GetField("<CreatedAt>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(message,
            model.CreatedAt);
        return message;
    }

    private YoutubeRoomModel UpdateMap(YoutubeRoom room, YoutubeRoomModel model)
    {
        var oldViewers = new List<YoutubeViewerModel>();
        var newViewers = new List<YoutubeViewerModel>();
        var viewers = room.Viewers;
        foreach (var viewer in viewers)
        {
            var viewerModel = model.Viewers.FirstOrDefault(v => v.Id == viewer.Id);
            if (viewerModel != null)
                oldViewers.Add(_mapper.Map(viewer, viewerModel));
            else
                newViewers.Add(_mapper.Map<YoutubeViewerModel>(viewer));
        }

        var oldMessages = new List<MessageModel>();
        var newMessages = new List<MessageModel>();
        var messages = room.Messages;
        foreach (var message in messages)
        {
            var messageModel = model.Messages.FirstOrDefault(v =>
                v.Text == message.Text && v.ViewerId == message.ViewerId && v.CreatedAt == message.CreatedAt);
            if (messageModel != null) oldMessages.Add(_mapper.Map(message, messageModel));
            else newMessages.Add(_mapper.Map<MessageModel>(message));
        }

        var oldIds = new List<VideoIdModel>();
        var newIds = new List<VideoIdModel>();
        var ids = room.VideoIds;
        foreach (var id in ids)
        {
            var viewerModel = model.VideoIds.FirstOrDefault(v => v.VideoId == id);
            if (viewerModel != null) oldIds.Add(viewerModel);
            else newIds.Add(new VideoIdModel {VideoId = id});
        }

        model.Viewers.RemoveAll(x => !oldViewers.Contains(x));
        model.Messages.RemoveAll(x => !oldMessages.Contains(x));
        model.VideoIds.RemoveAll(x => !oldIds.Contains(x));

        _mapper.Map(room, model);

        model.Viewers.AddRange(newViewers);
        model.Messages.AddRange(newMessages);
        model.VideoIds.AddRange(newIds);
        return model;
    }

    public async Task AddAsync(YoutubeRoom entity)
    {
        var room = new YoutubeRoomModel();
        UpdateMap(entity, room);
        await _context.AddAsync(room);
    }

    public async Task AddRangeAsync(IList<YoutubeRoom> entities)
    {
        var rooms = entities.Select(x => UpdateMap(x, new YoutubeRoomModel())).ToList();
        await _context.AddRangeAsync(rooms);
    }

    public async Task UpdateAsync(YoutubeRoom entity)
    {
        var model = await _context.YoutubeRooms.Include(x => x.Messages).Include(x => x.Viewers)
            .Include(x => x.VideoIds).FirstAsync(x => x.Id == entity.Id);
        UpdateMap(entity, model);
    }

    public async Task UpdateRangeAsync(IList<YoutubeRoom> entities)
    {
        var ids = entities.Select(room => room.Id);
        var rooms = await _context.YoutubeRooms.Include(x => x.Messages).Include(x => x.Viewers)
            .Include(x => x.VideoIds).Where(room => ids.Contains(room.Id)).ToListAsync();
        foreach (var entity in entities)
            UpdateMap(entity, rooms.First(youtubeRoomModel => youtubeRoomModel.Id == entity.Id));
    }

    public Task DeleteAsync(YoutubeRoom entity)
    {
        _context.Remove(_context.YoutubeRooms.First(room => room.Id == entity.Id));
        return Task.CompletedTask;
    }

    public Task DeleteRangeAsync(IEnumerable<YoutubeRoom> entities)
    {
        var ids = entities.Select(room => room.Id);
        _context.RemoveRange(_context.YoutubeRooms.Where(room => ids.Contains(room.Id)));
        return Task.CompletedTask;
    }

    public async Task<YoutubeRoom?> GetAsync(Guid id)
    {
        var room = await _context.YoutubeRooms.Include(x => x.Messages).Include(x => x.Viewers)
            .Include(x => x.VideoIds).FirstOrDefaultAsync(youtubeRoomModel => youtubeRoomModel.Id == id);
        return room == null ? null : GetMap(room);
    }

    public async Task<IList<YoutubeRoom>> FindAsync(
        ISpecification<YoutubeRoom, IYoutubeRoomSpecificationVisitor>? specification,
        IOrderBy<YoutubeRoom, IYoutubeRoomSortingVisitor>? orderBy = null, int? skip = null,
        int? take = null)
    {
        var query = _context.YoutubeRooms.Include(x => x.VideoIds).Include(x => x.Messages).Include(x => x.Viewers)
            .AsQueryable();
        if (specification != null)
        {
            var visitor = new YoutubeRoomVisitor();
            specification.Accept(visitor);
            if (visitor.Expr != null) query = query.Where(visitor.Expr);
        }

        if (orderBy != null)
        {
            var visitor = new YoutubeRoomSortingVisitor();
            orderBy.Accept(visitor);
            var firstQuery = visitor.SortItems.First();
            var orderedQuery = firstQuery.IsDescending
                ? query.OrderByDescending(firstQuery.Expr)
                : query.OrderBy(firstQuery.Expr);
            query = visitor.SortItems.Skip(1)
                .Aggregate(orderedQuery, (current, sort) => sort.IsDescending
                    ? current.ThenByDescending(sort.Expr)
                    : current.ThenBy(sort.Expr));
        }

        if (skip.HasValue) query = query.Skip(skip.Value);
        if (take.HasValue) query = query.Take(take.Value);

        return (await query.ToListAsync()).Select(GetMap).ToList();
    }

    public Task<int> CountAsync(ISpecification<YoutubeRoom, IYoutubeRoomSpecificationVisitor>? specification)
    {
        var query = _context.YoutubeRooms.Include(x => x.Messages).Include(x => x.Viewers).AsQueryable();
        if (specification == null) return query.CountAsync();
        var visitor = new YoutubeRoomVisitor();
        specification.Accept(visitor);
        if (visitor.Expr != null) query = query.Where(visitor.Expr);

        return query.CountAsync();
    }

    private static IMapper GetMapper() => new Mapper(new MapperConfiguration(expr =>
    {
        expr.CreateMap<YoutubeRoomModel, YoutubeRoom>().ForMember(x => x.Messages, opt => opt.Ignore())
            .ForMember(x => x.Viewers, opt => opt.Ignore())
            .ForMember(x => x.VideoIds, opt => opt.Ignore());

        expr.CreateMap<YoutubeRoom, YoutubeRoomModel>().ForMember(x => x.Messages, opt => opt.Ignore())
            .ForMember(x => x.Viewers, opt => opt.Ignore())
            .ForMember(x => x.VideoIds, opt => opt.Ignore());

        expr.CreateMap<YoutubeViewerModel, YoutubeViewer>().ReverseMap();
        expr.CreateMap<MessageModel, Message>().ReverseMap();
    }));
}