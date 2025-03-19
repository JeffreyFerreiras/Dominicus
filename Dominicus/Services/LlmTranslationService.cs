using LLama;
using LLama.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Dominicus.Models;
using System.Text;

namespace Dominicus.Services;

public class LlmTranslationService : ITranslationService, IDisposable
{
    private readonly ILogger<LlmTranslationService> _logger;
    private readonly LlmConfig _config;
    private readonly LLamaWeights _weights;
    private readonly LLamaContext _context;

    public LlmTranslationService(
        ILogger<LlmTranslationService> logger,
        IOptions<LlmConfig> config)
    {
        _logger = logger;
        _config = config.Value;

        var baseDir = AppContext.BaseDirectory;
        var modelPath = Path.Combine(baseDir, _config.ModelPath);

        _logger.LogInformation("Base Directory: {BaseDir}", baseDir);
        _logger.LogInformation("Model Path: {ModelPath}", modelPath);
        _logger.LogInformation("Model File Exists: {Exists}", File.Exists(modelPath));

        if (!File.Exists(modelPath))
        {
            throw new FileNotFoundException($"Model file not found at: {modelPath}");
        }

        var parameters = new ModelParams(modelPath)
        {
            ContextSize = (uint)_config.ContextSize,
            GpuLayerCount = 0
        };

        _logger.LogInformation("Loading model with parameters: ContextSize={ContextSize}, GpuLayerCount={GpuLayerCount}", 
            parameters.ContextSize, parameters.GpuLayerCount);

        _weights = LLamaWeights.LoadFromFile(parameters);
        _context = _weights.CreateContext(parameters);
        _logger.LogInformation("Model loaded successfully");
    }

    public async Task<(string englishResponse, string dominicanResponse)> GetTranslatedResponseAsync(string question)
    {
        _logger.LogInformation("Received translation request for question: {Question}", question);

        try
        {
            _logger.LogInformation("Generating English response for: {Question}", question);
            var englishPrompt = $"Answer this question in English: {question}";
            var englishResponse = await GetLlamaResponseAsync(englishPrompt);
            _logger.LogInformation("English response generated: {Response}", englishResponse);

            _logger.LogInformation("Generating Dominican response for: {Question}", question);
            var dominicanPrompt = $"Respond to this question in authentic Dominican Spanish slang, using common Dominican expressions and making it sound natural like a Dominican person would speak: {question}";
            var dominicanResponse = await GetLlamaResponseAsync(dominicanPrompt);
            _logger.LogInformation("Dominican response generated: {Response}", dominicanResponse);

            return (englishResponse, dominicanResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating response for question: {Question}", question);
            throw;
        }
    }

    private async Task<string> GetLlamaResponseAsync(string prompt)
    {
        _logger.LogInformation("Starting LLM inference with prompt: {Prompt}", prompt);
        
        var systemPrompt = "<<SYS>>You are a helpful AI assistant. Provide clear, concise, and accurate responses. When asked for a joke, tell a complete joke with a setup and punchline. When asked for a translation, provide a complete translation that maintains the meaning and style. When asked to respond in Dominican Spanish, use authentic Dominican Spanish slang, expressions, and cultural references. Use common Dominican words like 'tiguere', 'jevi', 'vaina', 'coño', 'dime', 'mamaguebo', 'papi', 'mami', etc. Make the response sound natural and conversational, like a real Dominican person would speak.<</SYS>>";
        
        var fullPrompt = $"{systemPrompt}\n<s>[INST] {prompt} [/INST] Let me help you with that. ";
        
        _logger.LogDebug("Full prompt being sent to model: {FullPrompt}", fullPrompt);

        var inferenceParams = new InferenceParams()
        {
            MaxTokens = _config.MaxTokens,
            AntiPrompts = new[] { "<s>", "</s>", "[INST]", "[/INST]" }
        };

        _logger.LogInformation("Inference parameters: MaxTokens={MaxTokens}, AntiPrompts={AntiPrompts}", 
            inferenceParams.MaxTokens, string.Join(", ", inferenceParams.AntiPrompts));

        var executor = new InteractiveExecutor(_context);
        var response = new StringBuilder();
        var tokenCount = 0;
        
        _logger.LogInformation("Beginning token generation");
        try 
        {
            await foreach (var text in executor.InferAsync(fullPrompt, inferenceParams))
            {
                tokenCount++;
                _logger.LogDebug("Token #{Count}: {Token}", tokenCount, text);
                
                if (!string.IsNullOrWhiteSpace(text))
                {
                    response.Append(text);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token generation after {Count} tokens", tokenCount);
            throw;
        }
        
        var finalResponse = response.ToString().Trim();
        _logger.LogInformation("Completed LLM inference. Tokens generated: {TokenCount}, Response length: {Length}, Response: {Response}", 
            tokenCount, finalResponse.Length, finalResponse);
        return finalResponse;
    }

    public void Dispose()
    {
        _context?.Dispose();
        _weights?.Dispose();
        GC.SuppressFinalize(this);
    }
} 