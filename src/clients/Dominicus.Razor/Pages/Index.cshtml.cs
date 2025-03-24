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
        _logger.LogInformation($"Form submitted with question: {Question}");
        
        if (string.IsNullOrWhiteSpace(Question))
        {
            _logger.LogWarning("Empty question submitted");
            ModelState.AddModelError("Question", "Please enter a question");
            return Page();
        }

        try
        {
            // Load existing conversation history first
            ConversationHistory = HttpContext.Session.Get<List<Conversation>>("ConversationHistory") ?? new List<Conversation>();
            
            _logger.LogInformation($"Getting translation for: {Question}");
            var (english, dominican) = await _translationService.GetTranslatedResponseAsync(Question);
            
            // Log with null check
            if (english != null)
            {
                _logger.LogInformation($"Received response - English: {english.Substring(0, Math.Min(50, english.Length))}...");
            }
            else
            {
                _logger.LogWarning("Received null English response");
            }
            
            var conversation = new Conversation
            {
                Question = Question,
                EnglishResponse = english ?? "No response received",
                DominicanResponse = dominican ?? "No response received",
                Timestamp = DateTime.UtcNow
            };

            ConversationHistory.Add(conversation);
            _logger.LogInformation($"Added new conversation. Total count: {ConversationHistory.Count}");

            // Keep only the most recent conversations
            if (ConversationHistory.Count > MaxHistoryItems)
            {
                ConversationHistory = ConversationHistory.Skip(ConversationHistory.Count - MaxHistoryItems).ToList();
            }

            // Ensure HTML escaped values are captured correctly
            HttpContext.Session.Set("ConversationHistory", ConversationHistory);
            _logger.LogInformation("Saved conversation history to session");
            
            // Important: Clear the question field AFTER the model has been processed
            ModelState.Clear();
            Question = string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing question: {Question}", Question);
            ModelState.AddModelError("", "Sorry, something went wrong processing your question.");
        }

        return Page();
    }
    
    public async Task<IActionResult> OnPostAskAsync([FromBody] QuestionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
        {
            return BadRequest(new { success = false, error = "Question cannot be empty" });
        }

        try
        {
            // Load existing conversation history
            ConversationHistory = HttpContext.Session.Get<List<Conversation>>("ConversationHistory") ?? new List<Conversation>();
            
            _logger.LogInformation($"API request with question: {request.Question}");
            var (english, dominican) = await _translationService.GetTranslatedResponseAsync(request.Question);
            
            var conversation = new Conversation
            {
                Question = request.Question,
                EnglishResponse = english ?? "No response received",
                DominicanResponse = dominican ?? "No response received",
                Timestamp = DateTime.UtcNow
            };

            ConversationHistory.Add(conversation);
            
            // Keep only the most recent conversations
            if (ConversationHistory.Count > MaxHistoryItems)
            {
                ConversationHistory = ConversationHistory.Skip(ConversationHistory.Count - MaxHistoryItems).ToList();
            }

            HttpContext.Session.Set("ConversationHistory", ConversationHistory);
            
            return new JsonResult(new { 
                success = true, 
                response = new {
                    question = conversation.Question,
                    englishResponse = conversation.EnglishResponse,
                    dominicanResponse = conversation.DominicanResponse,
                    timestamp = conversation.Timestamp
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing API question: {Question}", request.Question);
            return StatusCode(500, new { success = false, error = "An error occurred processing your request" });
        }
    }
}

public class QuestionRequest
{
    public string Question { get; set; } = string.Empty;
}
