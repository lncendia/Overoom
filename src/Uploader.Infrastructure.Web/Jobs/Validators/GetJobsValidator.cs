using FluentValidation;
using Uploader.Infrastructure.Web.Jobs.InputModels;

namespace Uploader.Infrastructure.Web.Jobs.Validators;

/// <summary>
/// Fluent-валидатор для <see cref="GetJobsInputModel"/>
/// </summary>
public class GetJobsValidator : AbstractValidator<GetJobsInputModel>
{
  /// <summary>
  /// Инициализирует новый экземпляр валидатора параметров пагинации
  /// </summary>
  public GetJobsValidator()
  {
    RuleFor(x => x.Skip)
      .GreaterThanOrEqualTo(0).WithMessage("Количество пропускаемых задач не может быть отрицательным");

    RuleFor(x => x.Take)
      .InclusiveBetween(1, 100).WithMessage("Количество задач должно быть от 1 до 100");
  }
}
