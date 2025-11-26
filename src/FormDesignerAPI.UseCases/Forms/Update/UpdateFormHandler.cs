using Ardalis.Result;
using FastEndpoints;
using FormDesignerAPI.Core.FormAggregate;
using FormDesignerAPI.Core.Interfaces;
using MediatR;

namespace FormDesignerAPI.UseCases.Forms.Update;

/// <summary>
/// Handler for updating an existing form.
/// Supports optional version parameters - if not provided, current version is retained.
/// </summary>
public class UpdateFormHandler : FastEndpoints.ICommandHandler<UpdateFormCommand, Result>, IRequestHandler<UpdateFormCommand, Result>
{
  private readonly IFormUpdateService _formUpdateService;
  private readonly IRepository<Form> _formRepository;

  public UpdateFormHandler(IFormUpdateService formUpdateService, IRepository<Form> formRepository)
  {
    _formUpdateService = formUpdateService;
    _formRepository = formRepository;
  }

  public async Task<Result> ExecuteAsync(UpdateFormCommand request, CancellationToken cancellationToken)
  {
    return await Handle(request, cancellationToken);
  }

  public async Task<Result> Handle(UpdateFormCommand request, CancellationToken cancellationToken)
  {
    // Retrieve the current form to get existing version if not updating it
    var existingForm = await _formRepository.GetByIdAsync(request.FormId, cancellationToken);
    if (existingForm == null)
    {
      return Result.NotFound();
    }

    // Determine the version to use:
    // If version parameters are provided, create a new version
    // Otherwise, keep the current version
    Core.FormAggregate.Revision? versionToUse = null;

    if (request.VersionMajor.HasValue && request.VersionMinor.HasValue && request.VersionPatch.HasValue)
    {
      // Create a new version with provided parameters
      var formDefinitionPath = request.FormDefinitionPath ?? "default.json";
      var formDefinition = new FormDefinition(formDefinitionPath);
      versionToUse = Core.FormAggregate.Revision.Create(
        request.VersionMajor.Value,
        request.VersionMinor.Value,
        request.VersionPatch.Value,
        formDefinition
      );
    }
    else if (request.FormDefinitionPath != null)
    {
      // Update the form definition path of the current version
      var currentRevision = existingForm.GetCurrentRevision();
      if (currentRevision != null)
      {
        currentRevision.UpdateRevision(
          currentRevision.Major,
          currentRevision.Minor,
          currentRevision.Patch,
          new FormDefinition(request.FormDefinitionPath)
        );
        versionToUse = currentRevision;
      }
    }
    else
    {
      // Keep the current version
      versionToUse = existingForm.GetCurrentRevision();
    }

    // If no version exists and none was provided, create a default version
    if (versionToUse == null)
    {
      var formDefinition = new FormDefinition("default.json");
      versionToUse = Core.FormAggregate.Revision.Create(1, 0, 0, formDefinition);
    }

    var formUpdateDto = new FormUpdateDto(
      request.FormId,
      request.FormNumber,
      request.FormTitle,
      request.FormDivision,
      request.OwnerName,
      request.OwnerEmail,
      versionToUse,
      request.RevisedDate ?? DateTime.UtcNow
    );

    return await _formUpdateService.UpdateFormAsync(request.FormId, formUpdateDto, cancellationToken);
  }
}

