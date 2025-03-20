using Dominicus.Core.Abstractions.Services;
using Dominicus.Models.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dominicus.Pages;

public class IndexModel : PageModel
{
    private readonly ITranslationService _translationService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ITranslationService translationService, ILogger<IndexModel> logger)
    {
        _translationService = translationService;
        _logger = logger;
    }

    [BindProperty]
    public string Question { get; set; } = string.Empty;

    public Conversation? CurrentConversation { get; private set; }

    public List<string> SuggestedQuestions { get; } = new()
    {
        "How are you today?",
        "What's the weather like?",
        "Tell me a joke",
        "What's your favorite food?"
    };

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Question))
        {
            return Page();
        }

        try
        {
            var (english, dominican) = await _translationService.GetTranslatedResponseAsync(Question);
            
            CurrentConversation = new Conversation
            {
                Question = Question,
                EnglishResponse = english,
                DominicanResponse = dominican
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing question");
            ModelState.AddModelError("", "Sorry, something went wrong processing your question.");
        }

        return Page();
    }
}
