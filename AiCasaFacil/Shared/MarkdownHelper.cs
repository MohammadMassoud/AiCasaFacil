// AiCasaFacil/Shared/MarkdownHelper.cs
using Markdig;

namespace AiCasaFacil.Shared;

public static class MarkdownHelper
{
    private static readonly MarkdownPipeline _pipeline =
        new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();

    public static string ToHtml(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
            return string.Empty;

        return Markdown.ToHtml(markdown, _pipeline);
    }
}