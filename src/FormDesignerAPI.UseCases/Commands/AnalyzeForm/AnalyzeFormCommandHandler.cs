using MediatR;
using Microsoft.Extensions.Logging;
using FormDesignerAPI.Core.FormContext.Aggregates;
using FormDesignerAPI.Core.FormContext.Interfaces;
using FormDesignerAPI.Core.FormContext.ValueObjects;
using FormDesignerAPI.UseCases.Interfaces;

namespace FormDesignerAPI.UseCases.Commands.AnalyzeForm;

/// <summary>
/// Handler for AnalyzeFormCommand
/// </summary>
public class AnalyzeFormCommandHandler : IRequestHandler<AnalyzeFormCommand, AnalyzeFormResult>
{
    private readonly IFormExtractor _formExtractor;
    private readonly IFormRepository _formRepository;
    private readonly ILogger<AnalyzeFormCommandHandler> _logger;

    public AnalyzeFormCommandHandler(
        IFormExtractor formExtractor,
        IFormRepository formRepository,
        ILogger<AnalyzeFormCommandHandler> logger)
    {
        _formExtractor = formExtractor;
        _formRepository = formRepository;
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

            // Detect form type
            var formTypeValue = await _formExtractor.DetectFormTypeAsync(tempPath, cancellationToken);
            var formType = FormType.FromString(formTypeValue);

            _logger.LogInformation("Detected form type: {FormType}", formType.Value);

            // Create Form aggregate
            var form = new Form(request.FileName, formType);

            // Extract structure
            var extractedStructure = await _formExtractor.ExtractFormStructureAsync(
                tempPath,
                formType,
                cancellationToken);

            // Add fields to aggregate
            foreach (var fieldData in extractedStructure.Fields)
            {
                var fieldType = FieldType.FromString(fieldData.Type);
                var field = new FormField(fieldData.Name, fieldData.Label, fieldType, fieldData.IsRequired);

                if (fieldData.MaxLength.HasValue)
                    field.SetMaxLength(fieldData.MaxLength.Value);

                if (!string.IsNullOrEmpty(fieldData.DefaultValue))
                    field.SetDefaultValue(fieldData.DefaultValue);

                if (fieldData.Options != null)
                {
                    foreach (var option in fieldData.Options)
                        field.AddOption(option);
                }

                if (!string.IsNullOrEmpty(fieldData.ValidationPattern))
                {
                    var validationRule = new ValidationRule(
                        fieldData.ValidationPattern,
                        "Validation failed",
                        fieldData.IsRequired);
                    field.SetValidationRule(validationRule);
                }

                form.AddField(field);
            }

            // Add tables to aggregate
            foreach (var tableData in extractedStructure.Tables)
            {
                var table = new FormTable(tableData.RowCount, tableData.ColumnCount);

                foreach (var cellData in tableData.Cells)
                {
                    var cell = new TableCell(
                        cellData.Content,
                        cellData.RowIndex,
                        cellData.ColumnIndex,
                        cellData.IsHeader);
                    table.AddCell(cell);
                }

                form.AddTable(table);
            }

            // Add warnings
            foreach (var warning in extractedStructure.Warnings)
            {
                form.AddWarning(warning);
            }

            // Mark as analyzed (raises domain event)
            form.MarkAsAnalyzed();

            // Persist to repository
            await _formRepository.AddAsync(form, cancellationToken);

            _logger.LogInformation(
                "Form analysis complete: {FormId}, {FieldCount} fields, {TableCount} tables",
                form.Id,
                form.Fields.Count,
                form.Tables.Count);

            // Map to result DTO
            return new AnalyzeFormResult
            {
                FormId = form.Id,
                FileName = form.FileName,
                FormType = form.FormType.Value,
                FieldCount = form.Fields.Count,
                TableCount = form.Tables.Count,
                Fields = form.Fields.Select(f => new FieldSummaryDto
                {
                    Name = f.Name,
                    Label = f.Label,
                    Type = f.FieldType.Value,
                    IsRequired = f.IsRequired,
                    HasOptions = f.Options.Any(),
                    HasValidation = f.ValidationRule != null,
                    MaxLength = f.MaxLength
                }).ToList(),
                Warnings = form.Warnings.ToList(),
                RequiresManualReview = form.RequiresManualReview(),
                AnalyzedAt = form.AnalyzedAt!.Value
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