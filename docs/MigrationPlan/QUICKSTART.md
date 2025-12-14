# Quick Start Guide - FormContext Implementation

## What We've Built So Far

✅ **Database layer** ready for FormContext
✅ **Mock Document Intelligence** service (no Azure needed yet!)
✅ **FormDefinitionMapper** to convert extracted fields
✅ **Scriban templates** already exist for code generation
✅ **Azure setup guide** when you're ready

## What's Next - 3 Main Tasks

### Task 1: Create Use Cases (Highest Priority)
We need to create the CQRS handlers that your Web endpoints will call.

**Start here**: Create these files in `src/FormDesignerAPI.UseCases/FormContext/`

#### A. Create Form From Definition
```
FormContext/
  Create/
    CreateFormCommand.cs
    CreateFormHandler.cs
```

#### B. Get Form
```
FormContext/
  Get/
    GetFormQuery.cs
    GetFormHandler.cs
```

#### C. List Forms
```
FormContext/
  List/
    ListFormsQuery.cs
    ListFormsHandler.cs
```

### Task 2: Re-enable PDF Analysis
**File**: `UseCases/Commands/AnalyzeForm/AnalyzeFormCommandHandler.cs.disabled`

Rename it to `.cs` and update it to:
1. Use FormContext.Form (not FormAggregate)
2. Call MockFormExtractorService
3. Call FormDefinitionMapper
4. Save Form with FormRevision

### Task 3: Wire Up Services
**File**: `Infrastructure/InfrastructureServiceExtensions.cs`

Add service registrations:
```csharp
// Document Intelligence
services.AddScoped<IFormExtractor, MockFormExtractorService>();
services.AddScoped<FormDefinitionMapper>();
```

## Quick Test Without Azure

Once the above is done, you can test the full workflow:

1. **Upload a PDF** with filename containing "contact" or "application"
2. **MockFormExtractorService** will simulate extraction
3. **FormDefinitionMapper** converts to FormDefinition
4. **Form aggregate** is created and saved
5. **Code generation** can create C#, React, SQL files

All without needing Azure credentials!

## When You're Ready for Azure

1. Follow `AZURE_SETUP_GUIDE.md`
2. Get your endpoint and key
3. Update `appsettings.json`
4. Create `AzureFormExtractorService.cs` (I can help with this)
5. Switch registration from Mock to Azure service

## File Structure We're Building

```
UseCases/
  FormContext/
    Create/
      CreateFormCommand.cs        ← Need to create
      CreateFormHandler.cs        ← Need to create
    Get/
      GetFormQuery.cs             ← Need to create
      GetFormHandler.cs           ← Need to create
    List/
      ListFormsQuery.cs           ← Need to create
      ListFormsHandler.cs         ← Need to create
  Commands/
    AnalyzeForm/
      AnalyzeFormCommandHandler.cs.disabled  ← Need to re-enable & update

Infrastructure/
  DocumentIntelligence/
    MockFormExtractorService.cs   ✅ Done
    FormDefinitionMapper.cs       ✅ Done
    AzureFormExtractorService.cs  ← Create when ready for Azure

Web/
  FormContext/                    ← Will create endpoints here later
```

## Code Snippets to Get You Started

### Example: CreateFormCommand.cs
```csharp
using FormDesignerAPI.Core.FormContext.Aggregates;
using FormDesignerAPI.Core.FormContext.ValueObjects;

namespace FormDesignerAPI.UseCases.FormContext.Create;

public record CreateFormCommand(
    string Name,
    FormDefinition Definition,
    string CreatedBy
) : IRequest<Result<Guid>>;
```

### Example: CreateFormHandler.cs
```csharp
using FormDesignerAPI.Core.FormContext.Aggregates;
using FormDesignerAPI.Core.FormContext.Interfaces;
using FormDesignerAPI.Core.FormContext.ValueObjects;

namespace FormDesignerAPI.UseCases.FormContext.Create;

public class CreateFormHandler : IRequestHandler<CreateFormCommand, Result<Guid>>
{
    private readonly IFormRepository _repository;

    public CreateFormHandler(IFormRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<Guid>> Handle(
        CreateFormCommand request,
        CancellationToken cancellationToken)
    {
        var origin = OriginMetadata.Manual(request.CreatedBy);

        var form = Form.Create(
            request.Name,
            request.Definition,
            origin,
            request.CreatedBy);

        await _repository.AddAsync(form, cancellationToken);

        return Result.Success(form.Id);
    }
}
```

## Testing Strategy

### Step 1: Unit Tests
Test the use case handlers with in-memory repository

### Step 2: Integration Tests
Test PDF upload → extraction → storage

### Step 3: Manual Testing
1. Start the Web API
2. Upload a PDF named "contact_form.pdf"
3. Mock service will extract contact form fields
4. Check database for created Form
5. Generate code from the form
6. Download ZIP with C#, React, SQL files

## Common Issues & Solutions

### "Form table doesn't exist"
→ You need to apply the migration (we created it, but didn't apply yet)

### "IFormExtractor not registered"
→ Add service registration in InfrastructureServiceExtensions.cs

### "FormRepository not found"
→ Need to implement IFormRepository (use EfRepository pattern)

## Next Session Plan

When you're ready to continue, we can:

1. ✅ **Create all FormContext use cases** (20-30 min)
2. ✅ **Re-enable AnalyzeFormCommandHandler** (10 min)
3. ✅ **Wire up services in DI** (5 min)
4. ✅ **Create Web endpoints** (20 min)
5. ✅ **Test PDF upload workflow** (10 min)
6. ✅ **Implement code generation service** (15 min)
7. ✅ **Test full workflow** (15 min)

Total: ~1.5-2 hours to get everything working!

## Questions?

- Want me to create the use cases now?
- Should we apply the database migration?
- Ready to test with the mock service?
- Want to set up Azure first?

Just let me know what you'd like to tackle next! 🚀
