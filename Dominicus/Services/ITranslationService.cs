namespace Dominicus.Services;

public interface ITranslationService
{
    Task<(string englishResponse, string dominicanResponse)> GetTranslatedResponseAsync(string question);
} 