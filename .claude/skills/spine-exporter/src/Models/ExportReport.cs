using System.Text.Json.Serialization;

namespace SpineExporter.Models;

/// <summary>
/// 导出报告
/// </summary>
public sealed class ExportReport
{
    [JsonPropertyName("timestamp")]
    public required DateTime Timestamp { get; init; }

    [JsonPropertyName("summary")]
    public required ExportSummary Summary { get; init; }

    [JsonPropertyName("details")]
    public required List<ExportDetail> Details { get; init; }
}

public sealed class ExportSummary
{
    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("success")]
    public int Success { get; set; }

    [JsonPropertyName("failed")]
    public int Failed { get; set; }
}

public sealed class ExportDetail
{
    [JsonPropertyName("source")]
    public required string Source { get; init; }

    [JsonPropertyName("status")]
    public required string Status { get; init; }

    [JsonPropertyName("animations")]
    public List<string>? Animations { get; init; }

    [JsonPropertyName("frames")]
    public int? Frames { get; init; }

    [JsonPropertyName("output")]
    public string? Output { get; init; }

    [JsonPropertyName("error")]
    public string? Error { get; init; }
}
