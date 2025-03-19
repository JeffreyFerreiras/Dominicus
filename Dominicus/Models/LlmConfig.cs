namespace Dominicus.Models;

public class LlmConfig
{
    public string ModelPath { get; set; } = Path.Combine(AppContext.BaseDirectory, "LLM", "llama-2-7b-chat.Q4_K_M.gguf");
    public int ContextSize { get; set; } = 2048;
    public int MaxTokens { get; set; } = 512;
    public float Temperature { get; set; } = 0.7f;
    public float TopP { get; set; } = 0.9f;
    public float RepeatPenalty { get; set; } = 1.1f;
} 