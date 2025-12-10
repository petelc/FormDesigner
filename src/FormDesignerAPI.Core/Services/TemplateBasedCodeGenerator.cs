// using FormDesignerAPI.Core.Common;

// namespace FormDesignerAPI.Core.Services;

// namespace CodeGeneration.Core.Domain.Services;

// public class TemplateBasedCodeGenerator
// {
//     private readonly ClaudeApiClient _claudeClient;
//     private readonly Dictionary<string, ICodeTemplate> _templates;

//     public TemplateBasedCodeGenerator(ClaudeApiClient claudeClient)
//     {
//         _claudeClient = claudeClient;
//         _templates = new Dictionary<string, ICodeTemplate>
//         {
//             // C# Templates
//             { "interface", new InterfaceTemplate() },
//             { "repository", new RepositoryTemplate() },
//             { "controller", new ControllerTemplate() },
//             { "business", new BusinessClassTemplate() },

//             // SQL Templates
//             { "create-table", new CreateTableTemplate() },
//             { "stored-procs", new StoredProceduresTemplate() },

//             // React Templates
//             { "react-form", new FormComponentTemplate() }
//         };
//     }

//     public async Task<List<GeneratedArtifact>> GenerateAll(
//         FormDefinition formDef,
//         GenerationOptions options,
//         string pdfPath)
//     {
//         var artifacts = new List<GeneratedArtifact>();
//         var context = CreateTemplateContext(formDef, options);

//         // Generate C# files
//         if (options.IncludeCSharpModels)
//         {
//             artifacts.Add(await GenerateFromTemplate("business", formDef, context, pdfPath));
//             artifacts.Add(await GenerateFromTemplate("interface", formDef, context, pdfPath));
//             artifacts.Add(await GenerateFromTemplate("repository", formDef, context, pdfPath));
//             artifacts.Add(await GenerateFromTemplate("controller", formDef, context, pdfPath));
//         }

//         // Generate SQL files
//         if (options.IncludeSqlSchema)
//         {
//             artifacts.Add(await GenerateFromTemplate("create-table", formDef, context, pdfPath));
//             artifacts.Add(await GenerateFromTemplate("stored-procs", formDef, context, pdfPath));
//         }

//         // Generate React components
//         if (options.IncludeReactComponents)
//         {
//             artifacts.Add(await GenerateFromTemplate("react-form", formDef, context, pdfPath));
//         }

//         return artifacts;
//     }

//     private async Task<GeneratedArtifact> GenerateFromTemplate(
//         string templateKey,
//         FormDefinition formDef,
//         TemplateContext context,
//         string pdfPath)
//     {
//         var template = _templates[templateKey];

//         // Generate prompt from template
//         var prompt = template.GeneratePrompt(formDef, context);

//         // Call Claude API with PDF
//         var generatedCode = await _claudeClient.GenerateCodeFromPdf(pdfPath, prompt);

//         // Post-process the generated code
//         generatedCode = template.PostProcess(generatedCode, context);

//         // Determine file path
//         var fileName = GetFileName(templateKey, formDef.Name, template.FileExtension);

//         return new GeneratedArtifact(
//             MapTemplateToArtifactType(templateKey),
//             fileName,
//             generatedCode
//         );
//     }

//     private TemplateContext CreateTemplateContext(
//         FormDefinition formDef,
//         GenerationOptions options)
//     {
//         return new TemplateContext
//         {
//             Namespace = options.Namespace ?? "YourApp",
//             ProjectName = options.ProjectName ?? "YourProject",
//             AdditionalImports = options.AdditionalImports ?? new(),
//             Variables = new Dictionary<string, object>
//             {
//                 { "FormName", formDef.Name },
//                 { "GeneratedDate", DateTime.UtcNow },
//                 { "Author", options.Author ?? "Code Generator" }
//             }
//         };
//     }

//     private string GetFileName(string templateKey, string formName, string extension)
//     {
//         return templateKey switch
//         {
//             "business" => $"Entities/{formName}{extension}",
//             "interface" => $"Interfaces/I{formName}Repository{extension}",
//             "repository" => $"Repositories/{formName}Repository{extension}",
//             "controller" => $"Controllers/{formName}Controller{extension}",
//             "create-table" => $"SQL/Tables/{formName}_Create{extension}",
//             "stored-procs" => $"SQL/StoredProcedures/{formName}_CRUD{extension}",
//             "react-form" => $"Components/{formName}Form{extension}",
//             _ => $"{formName}{extension}"
//         };
//     }

//     private ArtifactType MapTemplateToArtifactType(string templateKey)
//     {
//         return templateKey switch
//         {
//             "business" => ArtifactType.CSharpEntity,
//             "interface" => ArtifactType.CSharpInterface,
//             "repository" => ArtifactType.CSharpRepository,
//             "controller" => ArtifactType.CSharpController,
//             "create-table" => ArtifactType.SqlCreateTable,
//             "stored-procs" => ArtifactType.SqlStoredProcedures,
//             "react-form" => ArtifactType.ReactComponent,
//             _ => ArtifactType.Unknown
//         };
//     }
// }

