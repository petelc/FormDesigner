// Save to: src/FormDesignerAPI.Core/CodeGenerationContext/ValueObjects/ArtifactType.cs

namespace FormDesignerAPI.Core.CodeGenerationContext.ValueObjects;

/// <summary>
/// Types of code artifacts that can be generated
/// </summary>
public enum ArtifactType
{
  CSharpEntity,
  CSharpInterface,
  CSharpRepository,
  CSharpController,
  CSharpDto,
  CSharpAutoMapper,
  CSharpValidation,
  CSharpUnitTests,
  CSharpIntegrationTests,
  SqlCreateTable,
  SqlStoredProcedures,
  ReactComponent,
  ReactValidation,
  GitHubActions,
  AzurePipeline,
  Dockerfile
}