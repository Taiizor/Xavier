# Changelog

All notable changes to Xavier will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

-   Initial release of Xavier API Defaults Pack
-   `AddXavier()` service collection extension
-   `UseXavier()` application builder extension
-   `MapXavierInfrastructureEndpoints()` endpoint routing extension
-   Error handling with ProblemDetails (enabled by default)
-   Correlation ID middleware with W3C traceparent support
-   HTTP logging with safe defaults (opt-in)
-   Rate limiting with built-in policies (net7+, opt-in)
-   OpenAPI document exposure (net8+, enabled by default in Development)
-   Health endpoints (/health, /ready)
-   HttpClient resilience defaults (net8+, opt-in)
-   OpenTelemetry wiring helpers (opt-in)
-   Multi-targeting: net6.0, net7.0, net8.0, net9.0, net10.0

[Unreleased]: https://github.com/Taiizor/Xavier/compare/v1.0.0...HEAD
