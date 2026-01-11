# Xavier Versioning Policy

Xavier follows [Semantic Versioning 2.0.0](https://semver.org/).

## Version Format

`MAJOR.MINOR.PATCH[-PRERELEASE]`

-   **MAJOR**: Breaking changes to public API
-   **MINOR**: New features, backward-compatible
-   **PATCH**: Bug fixes, backward-compatible

## Pre-release Versions

-   `alpha`: Early development, unstable
-   `beta`: Feature complete, testing
-   `rc`: Release candidate

Example: `1.0.0-beta.1`

## Release Process

1. Update `CHANGELOG.md`
2. Create and push a tag: `git tag v1.0.0 && git push origin v1.0.0`
3. GitHub Actions will build, test, and publish to NuGet

## Supported Versions

| Version | Status | .NET Support   |
| ------- | ------ | -------------- |
| 1.x     | Active | net6.0-net10.0 |
