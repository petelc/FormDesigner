# Appendix B: Troubleshooting Guide

## Traxs.SharedKernel Issues

### Error: "Package 'Traxs.SharedKernel' not found"

**Cause:** Package not available in configured sources

**Solution:**
```bash
# Check your NuGet sources
dotnet nuget list source

# Add GitHub Packages source if missing
dotnet nuget add source \
  --name github \
  --username YOUR_GITHUB_USERNAME \
  --password YOUR_PAT \
  --store-password-in-clear-text \
  "https://nuget.pkg.github.com/petelc/index.json"
```

### Error: "EntityBase<Guid> not found"

**Cause:** Wrong base class used

**Solution:** Use `EntityBase<Guid>` not just `EntityBase`
```csharp
// Wrong
public class Form : EntityBase, IAggregateRoot
// Correct
public class Form : EntityBase<Guid>, IAggregateRoot
[Additional troubleshooting...]
