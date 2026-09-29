using System.Text.RegularExpressions;
using FluentValidation;
using Uploader.Infrastructure.Web.Queue.InputModels;

namespace Uploader.Infrastructure.Web.Queue.Validators;

/// <summary>
/// Fluent-валидатор для <see cref="QueueInputModel"/>
/// </summary>
public partial class QueueValidator : AbstractValidator<QueueInputModel>
{
  /// <summary>
  /// Инициализирует новый экземпляр валидатора для добавления комментария
  /// </summary>
  public QueueValidator()
  {
    RuleFor(x => x.MagnetUri)
      .NotEmpty().WithMessage("Magnet URI обязателен для заполнения")
      .MaximumLength(2000).WithMessage("Magnet URI не может быть длиннее 2000 символов")
      .Must(BeValidMagnetLink).WithMessage("Некорректный Magnet URI формат")
      .Must(ContainInfoHash).WithMessage("Magnet URI должен содержать info hash (xt параметр)");

    RuleFor(x => x.FileName)
      .MaximumLength(1000).WithMessage("Имя файла не может быть длиннее 1000 символов");

    RuleFor(x => x.FilmTitle)
      .MaximumLength(500).WithMessage("Название фильма не может быть длиннее 500 символов");

    RuleFor(x => x.FilmId)
      .NotEmpty().WithMessage("Идентификатор фильма обязателен");

    RuleFor(x => x.Resolution)
      .IsInEnum().WithMessage("Указано недопустимое разрешение видео");

    RuleFor(x => x.Version)
      .NotEmpty().WithMessage("Версия обязательна для заполнения")
      .MaximumLength(100).WithMessage("Версия не может быть длиннее 100 символов");

    RuleFor(x => x.Season)
      .GreaterThan(0)
      .When(x => x.Season.HasValue)
      .WithMessage("Номер сезона должен быть положительным числом");

    RuleFor(x => x.Episode)
      .GreaterThan(0)
      .When(x => x.Episode.HasValue)
      .WithMessage("Номер серии должен быть положительным числом");

    When(x => x.Season.HasValue || x.Episode.HasValue, () =>
    {
      RuleFor(x => x.Season)
        .NotNull()
        .WithMessage("При указании серии должен быть указан сезон");

      RuleFor(x => x.Episode)
        .NotNull()
        .WithMessage("При указании сезона должна быть указана серия");
    });
  }

  private static bool BeValidMagnetLink(string magnetUri)
  {
    return MagnetUri().IsMatch(magnetUri);
  }

  private static bool ContainInfoHash(string? magnetUri)
  {
    if (string.IsNullOrWhiteSpace(magnetUri))
      return false;

    return magnetUri.Contains("xt=urn:btih:", StringComparison.OrdinalIgnoreCase) ||
           magnetUri.Contains("xt=urn:ed2k:", StringComparison.OrdinalIgnoreCase) ||
           magnetUri.Contains("xt=urn:sha1:", StringComparison.OrdinalIgnoreCase);
  }

  [GeneratedRegex(@"^magnet:\?xt=urn:btih:[a-fA-F0-9]{40,64}(&[a-z0-9]+=[^&]*)*$", RegexOptions.IgnoreCase)]
  private static partial Regex MagnetUri();
}