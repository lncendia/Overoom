namespace Room.Domain.Rooms.FilmRooms.Exceptions;

public class CdnNameLengthException()
    : Exception("The CDN name length must be between 1 and 30 characters");