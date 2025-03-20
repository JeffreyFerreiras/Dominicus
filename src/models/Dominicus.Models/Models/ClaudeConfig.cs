namespace Dominicus.Models.Models;

public class ClaudeConfig
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "claude-3-sonnet-20240229";
    public int MaxTokens { get; set; } = 1024;
    public float Temperature { get; set; } = 0.7f;
} 