using System.Text.RegularExpressions;
using Anthropic;
using Anthropic.SDK;
using Anthropic.SDK.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Dominicus.Core.Abstractions.Services;
using Dominicus.Models.Models;

namespace Dominicus.Core.Services;

public class ClaudeTranslationService : ITranslationService
{
    private readonly ILogger<ClaudeTranslationService> _logger;
    private readonly ClaudeConfig _config;
    private readonly AnthropicClient _client;

    public ClaudeTranslationService(
        ILogger<ClaudeTranslationService> logger,
        IOptions<ClaudeConfig> config)
    {
        _logger = logger;
        _config = config.Value;
        _client = new AnthropicClient(_config.ApiKey);
    }

    public async Task<(string englishResponse, string dominicanResponse)> GetTranslatedResponseAsync(string question)
    {
        var systemPrompt = @"You are a friendly and humorous Dominican AI assistant. When responding:
1. First give a clear English response
2. Then respond in authentic Dominican Spanish using:
   - Common Dominican slang like 'tiguere', 'jevi', 'vaina', 'klk', 'dime', 'que lo what'
   - Typical Dominican expressions like 'diablo', 'pero tipo', 'que fue', 'ta to bien'
   - Informal Dominican pronunciation (e.g., dropping 's' at end of words, using 'l' instead of 'r')
   - Add humor and warmth typical of Dominican culture
   - Use 'manin', 'primo', or 'tigueraje' for friendly addressing
Make it sound like a real Dominican person speaking casually with a friend.

Format your response exactly like this:
ENGLISH:
[your English response]
DOMINICAN:
[your Dominican Spanish response]";

        var parameters = new MessageParameters
        {
            Model = _config.Model,
            MaxTokens = _config.MaxTokens,
            Temperature = (decimal)_config.Temperature,
            System = new List<SystemMessage> { new SystemMessage(systemPrompt) },
            Messages = new List<Message>
            {
                new Message { Role = RoleType.User, Content = new List<ContentBase> { new TextContent { Text = question } } }
            }
        };

        var response = await _client.Messages.GetClaudeMessageAsync(parameters);
        var content = response.Content.OfType<TextContent>().First().Text;

        // Extract English and Dominican responses using regex
        var englishMatch = Regex.Match(content, @"ENGLISH:\s*(.*?)(?=DOMINICAN:|$)", RegexOptions.Singleline);
        var dominicanMatch = Regex.Match(content, @"DOMINICAN:\s*(.*?)$", RegexOptions.Singleline);

        var englishResponse = englishMatch.Success ? englishMatch.Groups[1].Value.Trim() : content;
        var dominicanResponse = dominicanMatch.Success ? dominicanMatch.Groups[1].Value.Trim() : content;

        return (englishResponse, dominicanResponse);
    }
} 