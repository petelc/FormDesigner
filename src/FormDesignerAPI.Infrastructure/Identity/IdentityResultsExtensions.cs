using Ardalis.Result;
using Microsoft.AspNetCore.Identity;

namespace FormDesignerAPI.Infrastructure.Identity;

public static class IdentityResultsExtensions
{
    public static Result ToApplicationResult(this IdentityResult result)
    {

        return result.Succeeded
            ? Result.Success()
            : Result.Error(new ErrorList(result.Errors.Select(e => e.Description)));
    }
}
