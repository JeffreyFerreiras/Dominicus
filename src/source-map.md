# Dominicus Source Map

## Project Overview
Dominicus is a .NET solution that implements a Razor-based web application using Domain-Driven Design (DDD) principles.

## Solution Structure
- `Dominicus.sln` - Main solution file
- `src/` - Source code directory
  - `clients/` - Client applications
    - `Dominicus.Razor/` - Razor web application (Presentation Layer)
  - `services/` - Service layer projects
    - `Dominicus.Core/` - Core domain implementation
    - `Dominicus.Core.Abstractions/` - Domain interfaces and abstractions
    - `Dominicus.Infrastructure/` - Infrastructure implementations
  - `models/` - Shared models and DTOs
    - `Dominicus.Models/` - Shared models and DTOs

## Domain-Driven Design Architecture

### Presentation Layer (UI)
- `Dominicus.Razor/` - Razor web application
  - `Pages/` - Razor pages and components
  - `wwwroot/` - Static web assets
  - `Program.cs` - Application entry point
  - `appsettings.json` - Configuration files

### Domain Layer
- `Dominicus.Core/` - Core domain implementation
  - `Services/` - Domain service implementations
    - `LlmTranslationService.cs` - LLM-based translation service
  - Dependencies:
    - `Dominicus.Core.Abstractions`
    - `Dominicus.Models`
    - `LLama` - LLM integration
    - `Microsoft.Extensions.Logging`
    - `Microsoft.Extensions.Options`

### Domain Abstractions Layer
- `Dominicus.Core.Abstractions/` - Domain interfaces and abstractions
  - `Services/` - Service interfaces
    - `ITranslationService.cs` - Translation service interface
  - `Models/` - Domain models and configurations
    - `LlmConfig.cs` - LLM configuration model

### Infrastructure Layer
- `Dominicus.Infrastructure/` - Technical concerns
  - External service integrations
  - Data access implementations
  - Logging and monitoring
  - Security implementations

### Shared Layer
- `Dominicus.Models/` - Common components
  - Shared DTOs
  - Common utilities
  - Constants and enums
  - Shared interfaces

## Development Guidelines

### Project Organization
- Domain logic is centralized in the Core project
- Abstractions are separated into Core.Abstractions
- Infrastructure concerns are isolated in the Infrastructure project
- Shared code is placed in the Models project

### Dependencies
- Core project depends on Core.Abstractions and Models
- Infrastructure project depends on Core and Models
- Core.Abstractions has no external dependencies
- Models project has no external dependencies

### Configuration Management
- Domain-specific configuration in Core.Abstractions
- Infrastructure configuration in Infrastructure
- Application configuration in Razor project

### Build and Deployment
- Each project generates its own assembly
- Dependencies are managed through project references
- Build outputs are in respective `bin` and `obj` directories

## Future Considerations
- Implement CQRS pattern in Core layer
- Add event sourcing capabilities
- Consider microservices architecture for scalability
- Implement API Gateway for service communication
- Add comprehensive logging and monitoring
