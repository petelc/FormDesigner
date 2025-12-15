// using FormDesignerAPI.Core.FormAggregate;

// namespace FormDesignerAPI.IntegrationTests.Data;

// public class EfRepositoryFormAdd : BaseEfRepoTestFixture
// {
//     [Fact]
//     public async Task AddsFormAndSetsId()
//     {
//         var testFormNumber = "XXX1000";
//         var testFormTitle = "Test Form";
//         var repository = GetFormRepository();

//         var form = new Form(testFormNumber, testFormTitle);
//         // .UpdateDivision("Test Division")
//         // .SetOwner("Test Owner", string.Empty)
//         // .UpdateVersion("1.0")
//         // .SetConfigurationPath("/path/to/config");

//         await repository.AddAsync(form);

//         var newForm = (await repository.ListAsync())
//                         .FirstOrDefault();

//         newForm.ShouldNotBeNull();
//         testFormNumber.ShouldBe(newForm.FormNumber);
//         newForm.Id.ShouldBeGreaterThan(0);
//     }
// }
