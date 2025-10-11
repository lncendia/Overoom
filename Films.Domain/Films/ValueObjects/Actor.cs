using System.Diagnostics.CodeAnalysis;
using Common.Domain.Extensions;

namespace Films.Domain.Films.ValueObjects;

/// <summary>
/// Класс, представляющий информацию об актере.
/// </summary>
public record Actor
{
    private const int MaxPersonLength = 100;
    private const int MaxActorsDescriptionLength = 200;

    private readonly string _name;
    private readonly string? _role;

    /// <summary>
    /// Имя актера.
    /// </summary>
    public required string Name
    {
        get => _name;
        [MemberNotNull(nameof(_name))]
        init
        {
            value.ValidateLength(nameof(Name), MaxPersonLength);
            _name = value;
        }
    }

    /// <summary>
    /// Описание актера.
    /// </summary>
    public string? Role
    {
        get => _role;
        init
        {
            value?.ValidateLength(nameof(Role), MaxActorsDescriptionLength);
            _role = value;
        }
    }
}