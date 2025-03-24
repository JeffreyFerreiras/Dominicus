# Dominicus JavaScript Tests

This directory contains unit tests for the JavaScript components of the Dominicus chat application.

## Setup

The tests use Jest as the testing framework with JSDOM for browser API simulation. To set up the testing environment:

1. Navigate to the Razor project directory:
   ```bash
   cd src/clients/Dominicus.Razor
   ```

2. Install the required npm packages:
   ```bash
   npm install
   ```

## Running Tests

To run the tests once:
```bash
npm test
```

To run tests in watch mode (automatically re-run when files change):
```bash
npm run test:watch
```

## Test Structure

The tests are organized to match the structure of the JavaScript classes in the application:

- `ChatUIController` tests - UI manipulation and DOM interaction tests
- `MessageService` tests - API communication tests
- `ChatEventHandler` tests - Event handling and form submission tests
- `Utils` tests - Utility functions tests

Each test follows the Arrange-Act-Assert pattern for clarity and maintainability.

## Mocking

The tests use Jest's mocking capabilities to:
- Mock the DOM using JSDOM
- Mock the fetch API for testing API calls
- Mock dependencies through dependency injection

## Coverage

To run tests with coverage reporting:
```bash
npm test -- --coverage
```

This will generate a coverage report in the `coverage` directory. 