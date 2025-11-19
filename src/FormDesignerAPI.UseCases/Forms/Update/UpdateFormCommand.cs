using Ardalis.Result;
using FastEndpoints;
using FormDesignerAPI.Core.FormAggregate;
using MediatR;

namespace FormDesignerAPI.UseCases.Forms.Update;

/// <summary>
/// Command to update an existing form.
/// Version parameters are optional - if not provided, current values are retained.
/// </summary>
/// <param name="FormId">The ID of the form to update</param>
/// <param name="FormNumber">New form number</param>
/// <param name="FormTitle">New form title</param>
/// <param name="FormDivision">New form division</param>
/// <param name="OwnerName">New owner name</param>
/// <param name="OwnerEmail">New owner email</param>
/// <param name="VersionMajor">Optional: new major version number</param>
/// <param name="VersionMinor">Optional: new minor version number</param>
/// <param name="VersionPatch">Optional: new patch version number</param>
/// <param name="FormDefinitionPath">Optional: new form definition path</param>
/// <param name="RevisedDate">Date the form was revised</param>
public record UpdateFormCommand(
    Guid FormId,
    string FormNumber,
    string FormTitle,
    string FormDivision,
    string OwnerName,
    string OwnerEmail,
    int? VersionMajor = null,
    int? VersionMinor = null,
    int? VersionPatch = null,
    string? FormDefinitionPath = null,
    DateTime? RevisedDate = null
) : FastEndpoints.ICommand<Result>, IRequest<Result>;
