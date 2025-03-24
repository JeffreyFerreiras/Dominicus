namespace Dominicus.Core.Abstractions;

public interface ITranslationService
{
    Task<(string englishResponse, string dominicanResponse)> GetTranslatedResponseAsync(string question);
} 