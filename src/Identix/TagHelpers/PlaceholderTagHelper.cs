using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Identix.TagHelpers;

/// <summary>
/// Класс, реализующий тег-помощник для добавления атрибута placeholder.
/// </summary>
[HtmlTargetElement("span", Attributes = PlaceholderAttributeName, TagStructure = TagStructure.WithoutEndTag)]
public class PlaceholderTagHelper : TagHelper
{
  /// <summary>
  /// Имя атрибута placeholder.
  /// </summary>
  private const string PlaceholderAttributeName = "asp-placeholder-for";

  /// <summary>
  /// Выражение, которое будет вычислено относительно текущей модели.
  /// </summary>
  [HtmlAttributeName(PlaceholderAttributeName)]
  public ModelExpression Placeholder { get; set; } = null!;

  /// <summary> 
  /// Метод для обработки элемента HTML в Tag Helper. 
  /// </summary> 
  /// <param name="context">Контекст Tag Helper.</param> 
  /// <param name="output">Выходные данные Tag Helper.</param> 
  public override void Process(TagHelperContext context, TagHelperOutput output)
  {
    base.Process(context, output);
    string placeholder = GetPlaceholder(Placeholder.ModelExplorer);

    if (!output.Attributes.TryGetAttribute("data-placeholder", out _))
    {
      output.Attributes.Add(new TagHelperAttribute("data-placeholder", placeholder));
    }
  }

  /// <summary>
  /// Получает заполнитель для указанного ModelExplorer.
  /// </summary>
  /// <param name="modelExplorer">Модельный исследователь для получения информации о модели.</param>
  /// <returns>Заполнитель для модели.</returns>
  private static string GetPlaceholder(ModelExplorer modelExplorer)
  {
    string? placeholder = modelExplorer.Metadata.Placeholder;

    if (string.IsNullOrWhiteSpace(placeholder))
    {
      placeholder = modelExplorer.Metadata.GetDisplayName();
    }

    return placeholder;
  }
}