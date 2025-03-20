namespace Dominicus.Models.Models;

public class Conversation
{
    public string Question { get; set; } = string.Empty;
    public string EnglishResponse { get; set; } = string.Empty;
    public string DominicanResponse { get; set; } = string.Empty;
    public DateTime? Timestamp { get; set; }
} 