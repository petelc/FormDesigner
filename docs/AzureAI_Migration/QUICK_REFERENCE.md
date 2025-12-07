# Quick Reference Card

## NuGet Packages to Install

```bash
cd src/FormDesignerAPI.Infrastructure
dotnet add package Azure.AI.FormRecognizer --version 4.1.0
dotnet add package Azure.AI.OpenAI --version 1.0.0-beta.12
dotnet add package itext7 --version 8.0.2
```

## Configuration (appsettings.json)

```json
{
  "Azure": {
    "AI": {
      "DocumentIntelligence": {
        "Endpoint": "https://YOUR-RESOURCE.cognitiveservices.azure.com/",
        "ApiKey": "YOUR-KEY"
      },
      "OpenAI": {
        "Endpoint": "https://YOUR-RESOURCE.openai.azure.com/",
        "ApiKey": "YOUR-KEY",
        "DeploymentName": "gpt-4"
      },
      "FormGeneration": {
        "OutputDirectory": "C:\\GeneratedCode",
        "MaxConcurrentProcessing": 5
      }
    }
  }
}
```

## Key Files to Create

### Core Layer
- `Enums/PdfFormType.cs`
- `Entities/FormField.cs`
- `Entities/FormTable.cs`
- `Entities/FormStructure.cs`
- `Entities/GeneratedCode.cs`
- `Interfaces/IFormExtractor.cs`
- `Interfaces/ICodeGenerator.cs`
- `Interfaces/IFormToCodePipeline.cs`

### Use Cases Layer
- `DTOs/FormAnalysisResult.cs`
- `DTOs/CodeGenerationResult.cs`
- `DTOs/BatchProcessingResult.cs`
- `FormCodeGeneration/AnalyzeFormUseCase.cs`
- `FormCodeGeneration/GenerateCodeUseCase.cs`
- `FormCodeGeneration/BatchProcessUseCase.cs`

### Infrastructure Layer
- `Configuration/AzureAIOptions.cs`
- `AI/AzureDocumentIntelligence/FormTypeDetector.cs`
- `AI/AzureDocumentIntelligence/EnhancedFormExtractor.cs`
- `AI/AzureOpenAI/CodeGenerator.cs`
- `AI/Pipeline/EnhancedFormToCodePipeline.cs`
- `Configuration/InfrastructureServiceExtensions.cs`

### Web Layer
- `Controllers/FormCodeGenerationController.cs`
- `Extensions/WebServiceExtensions.cs`

## API Endpoints

- `POST /api/formcodegeneration/analyze` - Analyze a PDF form
- `POST /api/formcodegeneration/generate` - Generate code from form
- `POST /api/formcodegeneration/batch` - Batch process multiple forms

## Build Commands

```bash
# Build specific project
cd src/FormDesignerAPI.Core && dotnet build
cd src/FormDesignerAPI.UseCases && dotnet build
cd src/FormDesignerAPI.Infrastructure && dotnet build
cd src/FormDesignerAPI.Web && dotnet build

# Build entire solution
dotnet build

# Run the application
cd src/FormDesignerAPI.Web && dotnet run
```

## Testing

```bash
# Test analyze endpoint
curl -X POST "https://localhost:5001/api/formcodegeneration/analyze" \
  -H "Content-Type: multipart/form-data" \
  -F "pdfFile=@test-form.pdf"

# Test generate endpoint
curl -X POST "https://localhost:5001/api/formcodegeneration/generate" \
  -H "Content-Type: multipart/form-data" \
  -F "pdfFile=@test-form.pdf" \
  -F "formName=TestForm"
```

## Common Issues

1. **401 Unauthorized** → Check Azure API keys
2. **Package not found** → Run `dotnet restore`
3. **Build errors** → Check project references
4. **JSON deserialization error** → Verify Azure OpenAI response format
