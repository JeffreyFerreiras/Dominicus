using Dominicus.Core;
using Dominicus.Core.Abstractions;
using Dominicus.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Dominicus.Core.Tests;

[TestFixture]
public class TranslationServiceTests
{
    private IOptions<ClaudeConfig> _options;
    private ILogger<ClaudeTranslationService> _logger;
    private ClaudeConfig _config;

    [SetUp]
    public void Setup()
    {
        _config = new ClaudeConfig
        {
            ApiKey = "test-api-key",
            Model = "claude-3-haiku-20240307",
            MaxTokens = 1000,
            Temperature = 0.7f
        };

        _options = Substitute.For<IOptions<ClaudeConfig>>();
        _options.Value.Returns(_config);
        _logger = Substitute.For<ILogger<ClaudeTranslationService>>();
    }

    [Test]
    public void ClaudeTranslationService_Constructor_ThrowsArgumentNull_WhenDependenciesAreNull()
    {
        // Assert
        Assert.Throws<ArgumentNullException>(() => new ClaudeTranslationService(null!, _options));
        Assert.Throws<ArgumentNullException>(() => new ClaudeTranslationService(_logger, null!));
    }

    [Test]
    public void ClaudeTranslationService_Constructor_InitializesService_WhenDependenciesAreValid()
    {
        // Act - this should not throw
        var service = new ClaudeTranslationService(
            _logger,
            _options);

        // Assert
        Assert.That(service, Is.Not.Null);
    }

    [TestCase("")]
    [TestCase("   ")]
    public void GetTranslatedResponseAsync_ThrowsArgumentException_WhenQuestionIsEmpty(string question)
    {
        // Arrange
        var service = new ClaudeTranslationService(
            _logger,
            _options);

        // Act & Assert
        var exception = Assert.ThrowsAsync<ArgumentException>(async () => 
            await service.GetTranslatedResponseAsync(question));
        
        Assert.That(exception, Is.Not.Null);
    }
    
    [Test]
    public void GetTranslatedResponseAsync_ThrowsArgumentException_WhenQuestionIsNull()
    {
        // Arrange
        var service = new ClaudeTranslationService(
            _logger,
            _options);

        // Act & Assert
        var exception = Assert.ThrowsAsync<ArgumentException>(async () => 
            await service.GetTranslatedResponseAsync(null!));
            
        Assert.That(exception, Is.Not.Null);
    }
} 