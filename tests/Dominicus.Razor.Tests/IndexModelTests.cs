using Dominicus.Core.Abstractions;
using Dominicus.Models;
using Dominicus.Razor.Pages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dominicus.Razor.Tests;

[TestFixture]
public class IndexModelTests
{
    private ITranslationService _translationService;
    private ILogger<IndexModel> _logger;
    private HttpContext _httpContext;
    private ISession _session;
    
    [SetUp]
    public void Setup()
    {
        _translationService = Substitute.For<ITranslationService>();
        _logger = Substitute.For<ILogger<IndexModel>>();
        _httpContext = Substitute.For<HttpContext>();
        _session = Substitute.For<ISession>();
        _httpContext.Session.Returns(_session);
    }
    
    [Test]
    public void SuggestedQuestions_ReturnsNonEmptyList()
    {
        // Arrange
        var indexModel = new IndexModel(_translationService, _logger);
        
        // Act/Assert
        Assert.That(indexModel.SuggestedQuestions, Is.Not.Empty);
    }
    
    [Test]
    public async Task OnPostAsync_ReturnsPage_WhenQuestionIsEmpty()
    {
        // Arrange
        var indexModel = new IndexModel(_translationService, _logger)
        {
            Question = string.Empty,
            PageContext = new PageContext { HttpContext = _httpContext }
        };
        
        // Act
        var result = await indexModel.OnPostAsync();
        
        // Assert
        Assert.That(result, Is.TypeOf<PageResult>());
        await _translationService.DidNotReceive().GetTranslatedResponseAsync(Arg.Any<string>());
    }
    
    [Test]
    public async Task OnPostAsync_CallsTranslationService_WhenQuestionIsValid()
    {
        // Arrange
        _translationService
            .GetTranslatedResponseAsync(Arg.Any<string>())
            .Returns(("English response", "Dominican response"));
        
        var indexModel = new IndexModel(_translationService, _logger)
        {
            Question = "Valid question",
            PageContext = new PageContext { HttpContext = _httpContext }
        };
        
        // Act
        var result = await indexModel.OnPostAsync();
        
        // Assert
        Assert.That(result, Is.TypeOf<PageResult>());
        await _translationService.Received(1).GetTranslatedResponseAsync("Valid question");
    }
    
    [Test]
    public async Task OnPostAsync_HandlesNullResponses_GracefullyWithFallback()
    {
        // Arrange
        _translationService
            .GetTranslatedResponseAsync(Arg.Any<string>())
            .Returns(("", ""));
        
        var indexModel = new IndexModel(_translationService, _logger)
        {
            Question = "Valid question",
            PageContext = new PageContext { HttpContext = _httpContext }
        };
        
        // Act
        var result = await indexModel.OnPostAsync();
        
        // Assert
        Assert.That(result, Is.TypeOf<PageResult>());
        await _translationService.Received(1).GetTranslatedResponseAsync("Valid question");
        
        // Verify data was saved with fallback values
        _session.Received().Set(
            Arg.Is<string>("ConversationHistory"), 
            Arg.Any<byte[]>());
    }
} 