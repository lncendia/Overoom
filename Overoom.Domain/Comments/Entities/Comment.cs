﻿using Overoom.Domain.Abstractions;
using Overoom.Domain.Comments.Exceptions;

namespace Overoom.Domain.Comments.Entities;

public class Comment : AggregateRoot
{
    private static readonly string[] BadWords =
    {
        "бля", "хуй", "хуя", "хуе", "пизд", "пидор", "педик", "письк", "жопа", "писюн", "сука", "вагина", "влагалище",
        "сучий", "ебал", "ебу", "ебан", "ёбан", "ебат", "ебну", "ёбну", "ебит", "уеб", "уёб", "сперм", "залуп", "анал", "анус", "сучи",
        "гандон", "гнида", "говн", "сосать", "соси", "жопе", "пёзд", "пезд", "трах"
    };

    public Comment(Guid filmId, Guid userId, string text)
    {
        FilmId = filmId;
        UserId = userId;
        if (string.IsNullOrEmpty(text) || text.Length > 1000) throw new TextLengthException();
        foreach (var badWord in BadWords)
        {
            var index = text.IndexOf(badWord, StringComparison.OrdinalIgnoreCase);
            while (index != -1)
            {
                text = text.Remove(index, badWord.Length).Insert(index, new string('*', badWord.Length));
                index = text.IndexOf(badWord, index, StringComparison.OrdinalIgnoreCase);
            }
        }

        Text = text;
    }

    public Guid FilmId { get; }
    public Guid UserId { get; }

    public string Text { get; }

    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}