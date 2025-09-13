using FormDesignerAPI.Core.FormAggregate;
using FormDesignerAPI.Core.Services;

namespace FormDesignerAPI.UnitTests.Core.Services;

public class DeleteFormService_DeleteForm
{
    private readonly IRepository<Form> _repository = Substitute.For<IRepository<Form>>();
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly ILogger<FormDeletedService> _logger = Substitute.For<ILogger<FormDeletedService>>();

    private readonly FormDeletedService _service;

    public DeleteFormService_DeleteForm()
    {
        _service = new FormDeletedService(_repository, _mediator, _logger);
    }

    [Fact]
    public async Task ReturnsNotFoundGivenCantFindForm()
    {
        var result = await _service.DeleteFormAsync(0);

        result.Status.ShouldBe(Ardalis.Result.ResultStatus.NotFound);
    }
}
