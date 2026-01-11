# Contributing to Xavier

Thank you for your interest in contributing to Xavier! This document provides guidelines and information for contributors.

## Getting Started

1. Fork the repository
2. Clone your fork: `git clone https://github.com/YOUR-USERNAME/Xavier.git`
3. Create a feature branch: `git checkout -b feature/your-feature`
4. Make your changes
5. Run tests: `dotnet test`
6. Commit your changes: `git commit -m "feat: add your feature"`
7. Push to your fork: `git push origin feature/your-feature`
8. Open a Pull Request

## Development Setup

### Prerequisites

-   .NET SDK 10.0 or later (will roll forward to latest)
-   Git

### Building

```bash
dotnet build Xavier.sln
```

### Running Tests

```bash
dotnet test Xavier.sln
```

## Coding Standards

-   Follow existing code style (enforced by `.editorconfig`)
-   Enable nullable reference types
-   Add XML documentation for public APIs
-   Write unit tests for new functionality
-   Keep commits atomic and well-described

## Commit Messages

We follow [Conventional Commits](https://www.conventionalcommits.org/):

-   `feat:` New feature
-   `fix:` Bug fix
-   `docs:` Documentation changes
-   `refactor:` Code refactoring
-   `test:` Adding or updating tests
-   `chore:` Maintenance tasks

## Pull Request Process

1. Update documentation if needed
2. Add tests for new functionality
3. Ensure all tests pass
4. Update CHANGELOG.md if applicable
5. Request review from maintainers

## Questions?

Open a [Discussion](https://github.com/Taiizor/Xavier/discussions) or [Issue](https://github.com/Taiizor/Xavier/issues).

## License

By contributing, you agree that your contributions will be licensed under the MIT License.
