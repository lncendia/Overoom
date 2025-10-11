using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Identix.TagHelpers;

[HtmlTargetElement("svg-icon", Attributes = "src")]
public class SvgTagHelper(IWebHostEnvironment env) : TagHelper
{
    /// <summary>
    /// Путь до SVG (от корня wwwroot, например "~/icons/user.svg").
    /// </summary>
    [HtmlAttributeName("src")]
    public string Src { get; set; } = string.Empty;

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (string.IsNullOrEmpty(Src))
            return;

        output.TagName = null;

        var relativePath = Src.StartsWith("~/") ? Src[2..] : Src;
        var fullPath = Path.Combine(env.WebRootPath, relativePath);

        if (!File.Exists(fullPath))
        {
            output.Content.SetContent($"<!-- SVG not found: {Src} -->");
            return;
        }

        var svgContent = await File.ReadAllTextAsync(fullPath);

        // Находим открывающий тег <svg ...>
        var match = Regex.Match(svgContent, @"<svg\b[^>]*>", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            var originalTag = match.Value;

            // Собираем все атрибуты из <svg-icon>
            var newAttributes = context.AllAttributes
                .Where(a => a.Name != "src")
                .ToDictionary(a => a.Name.ToLower(), a => a.Value?.ToString() ?? "");

            // Обновляем/добавляем атрибуты в <svg ...>
            var updatedTag = originalTag;

            foreach (var attr in newAttributes)
            {
                // Есть ли уже атрибут?
                var attrRegex = new Regex($"""
                                           \s{attr.Key}="[^"]*"
                                           """, RegexOptions.IgnoreCase);
                if (attrRegex.IsMatch(updatedTag))
                {
                    updatedTag = attrRegex.Replace(updatedTag, $" {attr.Key}=\"{attr.Value}\"");
                }
                else
                {
                    updatedTag = updatedTag.TrimEnd('>') + $" {attr.Key}=\"{attr.Value}\">";
                }
            }

            // Подменяем вхождение <svg ...> на обновлённое
            svgContent = svgContent.Replace(originalTag, updatedTag);
        }

        output.Content.SetHtmlContent(svgContent);
    }
}