using FormDesignerAPI.Core.FormAggregate;
using FormDesignerAPI.Core.Services;

namespace FormDesignerAPI.UnitTests.Core.Services;

public class UpdateFormService_UpdateForm
{
    private readonly IRepository<Form> _repository = Substitute.For<IRepository<Form>>();
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly ILogger<FormUpdateService> _logger = Substitute.For<ILogger<FormUpdateService>>();

    private readonly FormUpdateService _service;

    public UpdateFormService_UpdateForm()
    {
        _service = new FormUpdateService(_repository, _mediator, _logger);
    }

    [Fact]
    public async Task UpdateForm_ValidRequest_UpdatesForm()
    {
        // Arrange
        var formId = 1;
        var formUpdateDto = new FormUpdateDto
        (
            formId,
            "New Form Number",
            "New Form Title",
            "New Division",
            "New Owner",
            "New Version",
            DateTime.UtcNow,
            "New Configuration Path"
        );

        var existingForm = new Form
        (
            "Old Form Number",
            "Old Form Title",
            "Old Division",
            new Owner("Old Owner", "oldowner@example.com"),
            "Old Version",
            DateTime.UtcNow.AddMonths(-1),
            DateTime.UtcNow,
            "Old Configuration Path"
        );

        _repository.GetByIdAsync(formId).Returns(existingForm);
        var result = await _service.UpdateFormAsync(formId, formUpdateDto, CancellationToken.None);
        result.Status.ShouldBe(Ardalis.Result.ResultStatus.Ok);
    }

}
