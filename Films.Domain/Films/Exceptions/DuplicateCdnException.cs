using System;

namespace Films.Domain.Films.Exceptions;

public class DuplicateCdnException() : Exception("There cannot be two CDNs of the same type");