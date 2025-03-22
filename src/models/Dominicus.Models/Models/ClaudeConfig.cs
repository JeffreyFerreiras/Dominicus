namespace Dominicus.Models.Models;

public class ClaudeConfig
{
    public string ApiKey { get; set; }
    public string Model { get; set; }
    public int MaxTokens { get; set; }
    public float Temperature { get; set; }
} 