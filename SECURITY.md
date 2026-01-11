# Security Policy

## Supported Versions

| Version | Supported          |
| ------- | ------------------ |
| 1.x     | :white_check_mark: |

## Reporting a Vulnerability

If you discover a security vulnerability in Xavier, please report it responsibly:

1. **Do NOT open a public issue**
2. Email security concerns to the maintainers privately
3. Provide detailed information about the vulnerability
4. Allow reasonable time for a fix before public disclosure

## Security Considerations

Xavier is designed with security in mind:

### HTTP Logging

-   Authorization and Cookie headers are excluded by default
-   Request/response bodies are not logged by default
-   Sensitive data must be explicitly allowed

### Exception Details

-   Stack traces and exception details are hidden in production
-   Only generic error messages are returned to clients
-   Enable `IncludeExceptionDetails` only in development

### OpenAPI

-   OpenAPI endpoints are exposed only in Development by default
-   Enable `ExposeInProduction` explicitly if needed
-   Consider authentication for OpenAPI endpoints in production

### Rate Limiting

-   Enable rate limiting to prevent abuse
-   Configure appropriate limits for your use case

## Best Practices

1. Always use HTTPS in production
2. Review logged data for PII compliance
3. Configure appropriate rate limits
4. Keep Xavier and dependencies updated
