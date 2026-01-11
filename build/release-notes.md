# Xavier Release Notes

## Release Process

1. Update `CHANGELOG.md` with all changes
2. Run tests: `dotnet test`
3. Create a tag: `git tag v<version>`
4. Push: `git push origin v<version>`
5. GitHub Actions will automatically build, test, and publish to NuGet

## Pre-release Checklist

- [ ] All tests pass
- [ ] CHANGELOG.md is updated
- [ ] README.md is updated if needed
- [ ] Version number follows SemVer
- [ ] All TFMs build successfully (net6.0, net7.0, net8.0, net9.0, net10.0)

## Post-release Verification

- [ ] Package appears on NuGet.org
- [ ] README.NUGET.md renders correctly
- [ ] GitHub Release is created with notes
- [ ] Symbol package (.snupkg) is uploaded

## Version History

See [CHANGELOG.md](../CHANGELOG.md) for detailed version history.
