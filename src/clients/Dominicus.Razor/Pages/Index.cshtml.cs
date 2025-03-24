using Dominicus.Core.Abstractions;
using Dominicus.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dominicus.Razor.Pages;

public class IndexModel : PageModel
{
    private readonly ITranslationService _translationService;
    private readonly ILogger<IndexModel> _logger;
    private const int MaxHistoryItems = 50;

    public IndexModel(ITranslationService translationService, ILogger<IndexModel> logger)
    {
        _translationService = translationService;
        _logger = logger;
    }

    [BindProperty]
    public string Question { get; set; } = string.Empty;

    public List<Conversation> ConversationHistory { get; private set; } = new();

    public List<string> SuggestedQuestions { get; } = new()
    {
        "How are you today?",
        "What's the weather like?",
        "Tell me a joke",
        "What's your favorite Dominican food?",
        "Teach me some Dominican slang",
        "What's your favorite Dominican music?"
    };

    public void OnGet()
    {
        var history = HttpContext.Session.Get<List<Conversation>>("ConversationHistory");
        if (history != null)
        {
            ConversationHistory = history;
            _logger.LogInformation($"Loaded {ConversationHistory.Count} conversation items from session");
        }
        else
        {
            _logger.LogWarning("No conversation history found in session");
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Question))
        {
            return Page();
        }

        try
        {
            var (english, dominican) = await _translationService.GetTranslatedResponseAsync(Question);
            
            var conversation = new Conversation
            {
                Question = Question,
                EnglishResponse = english,
                DominicanResponse = dominican,
                Timestamp = DateTime.UtcNow
            };

            ConversationHistory = HttpContext.Session.Get<List<Conversation>>("ConversationHistory") ?? new List<Conversation>();
            ConversationHistory.Add(conversation);
            _logger.LogInformation($"Added new conversation. Total count: {ConversationHistory.Count}");

            // Keep only the most recent conversations
            if (ConversationHistory.Count > MaxHistoryItems)
            {
                ConversationHistory = ConversationHistory.Skip(ConversationHistory.Count - MaxHistoryItems).ToList();
            }

            HttpContext.Session.Set("ConversationHistory", ConversationHistory);
            _logger.LogInformation("Saved conversation history to session");
            Question = string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing question");
            ModelState.AddModelError("", "Sorry, something went wrong processing your question.");
        }

        return Page();
    }
}

// Session extension methods
public static class SessionExtensions
{
    public static void Set<T>(this ISession session, string key, T value)
    {
        session.SetString(key, System.Text.Json.JsonSerializer.Serialize(value));
    }

    public static T? Get<T>(this ISession session, string key)
    {
        var value = session.GetString(key);
        return value == null ? default : System.Text.Json.JsonSerializer.Deserialize<T>(value);
    }
}
