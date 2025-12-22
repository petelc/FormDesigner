using MediatR;

namespace FormDesignerAPI.UseCases.Commands.AnalyzeProject;

/// <summary>
/// Command to analyze a project
/// </summary>
public record AnalyzeProjectCommand(
    Guid ProjectId,
    string UserId
) : IRequest<AnalyzeProjectResult>;
