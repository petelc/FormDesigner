using MediatR;
using Microsoft.Extensions.Logging;
using FormDesignerAPI.Core.FormContext.Aggregates;
using FormDesignerAPI.Core.FormContext.Interfaces;
using FormDesignerAPI.Core.FormContext.ValueObjects;
using FormDesignerAPI.UseCases.Interfaces;
using FormDesignerAPI.UseCases.FormContext.Mappers;

namespace FormDesignerAPI.UseCases.Commands.AnalyzeForm;

/// <summary>
/// Handler for AnalyzeFormCommand - analyzes PDF forms and creates Form aggregates
/// </summary>
public class AnalyzeFormCommandHandler : IRequestHandler<AnalyzeFormCommand, AnalyzeFormResult>
{
    private readonly IFormExtractor _formExtractor;
    private readonly IFormRepository _formRepository;
    private readonly FormDefinitionMapper _mapper;
    private readonly ILogger<AnalyzeFormCommandHandler> _logger;

    public AnalyzeFormCommandHandler(
        IFormExtractor formExtractor,
        IFormRepository formRepository,
        FormDefinitionMapper mapper,
        ILogger<AnalyzeFormCommandHandler> logger)
    {
        _formExtractor = formExtractor;
        _formRepository = formRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<AnalyzeFormResult> Handle(
        AnalyzeFormCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Analyzing form: {FileName}", request.FileName);

        // Save PDF to temp location for processing
        var tempPath = Path.GetTempFileName();
        try
        {
            await using (var fileStream = new FileStream(tempPath, FileMode.Create))
            {
                await request.PdfStream.CopyToAsync(fileStream, cancellationToken);
            }

            // Detect form type using Document Intelligence
            var formType = await _formExtractor.DetectFormTypeAsync(tempPath, cancellationToken);
            _logger.LogInformation("Detected form type: {FormType}", formType);

            // Extract structure from PDF
            var extractedStructure = await _formExtractor.ExtractFormStructureAsync(
                tempPath,
                formType,
                cancellationToken);

            _logger.LogInformation(
                "Extracted {FieldCount} fields, {TableCount} tables",
                extractedStructure.Fields.Count,
                extractedStructure.Tables.Count);

            // Map extracted structure to FormDefinition value object
            var definition = _mapper.MapToFormDefinition(extractedStructure);

            // Optionally map tables as repeating fields
            if (extractedStructure.Tables.Any())
            {
                var tableFields = _mapper.MapTablesAsRepeatingFields(extractedStructure.Tables);
                _logger.LogInformation(
                    "Mapped {TableFieldCount} additional fields from tables",
                    tableFields.Count);
            }

            // Create Form aggregate with Import origin
            var fileName = Path.GetFileNameWithoutExtension(request.FileName);
            var origin = OriginMetadata.Import(request.FileName, "system");
            var form = Form.Create(fileName, definition, origin, "system");

            _logger.LogInformation("Created form aggregate: {FormId}", form.Id);

            // Persist to repository
            await _formRepository.AddAsync(form, cancellationToken);
            await _formRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Form analysis complete: {FormId}, {FieldCount} fields",
                form.Id,
                form.FieldCount);

            // Map to result DTO
            return new AnalyzeFormResult
            {
                FormId = form.Id,
                FileName = request.FileName,
                FormType = formType,
                FieldCount = form.FieldCount,
                TableCount = extractedStructure.Tables.Count,
                Fields = definition.Fields.Select(f => new FieldSummaryDto
                {
                    Name = f.Name,
                    Label = f.Label ?? f.Name,
                    Type = f.Type,
                    IsRequired = f.Required,
                    HasOptions = f.Options?.Any() ?? false,
                    HasValidation = !string.IsNullOrEmpty(f.Pattern),
                    MaxLength = f.MaxLength
                }).ToList(),
                Warnings = extractedStructure.Warnings,
                RequiresManualReview = extractedStructure.Warnings.Any(),
                AnalyzedAt = DateTime.UtcNow
            };
        }
        finally
        {
            // Clean up temp file
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }
}