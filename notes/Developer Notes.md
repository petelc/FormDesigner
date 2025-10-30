# Developer Notes

### Update Form

Currently the update to a form is handled in the UseCases Project using the UpdateFormCommand and the UpdateFormHandler. But the delete form is handled in the Core project as a service. What would it take to handle the update form as a service in the Core project similiar to the delete form service is?

What do I need to do:

**\_\_** Create the IFormUpdateService in the interfaces namespace.
**\_\_** Create the FormUpdateEvent in the Events namespace
**\_\_** Create the FormUpdateService in the Services namespace
**\_\_** Implement the IFormUpdateService interface in the FormUpdateService class
**\_\_** Create the FormUpdatedHandler in the Handlers namespace

In the UseCases Project:
**\_\_\_** Update the handler to call the service from the Core project.

### Registering Update Form Service

~~Currently the FormUpdateService is registered in the Program class of the Web project. I should move it to the Infrastructure Service Extensions.~~

Ok rethinking this, I am going to move the update form service registration to the Services Config.

---

So resolve the login not returning a token I need to do the following:

-[] Create an IdentityTokenClaimService that implements ITokenClaimService (Infrastructure Project)
-[] Create the ITokenClaimService in the Core Project
-[] Create required AuthEnpoints (create folder in Web project?)
-[] AuthenticateEndpoint.AuthenticateRequest
-[] AuthenticateEndpoint.AuthenticateResponse
-[] AuthenticateEndpoint.ClaimValue
-[] AuthenticateEndpoint.UserInfo
-[] AuthenticateEndpoint
-[] Create RoleManagementEndpoints
-[] Create RoleMembershipEndpoints
-[] Create UserManagementEndpoints

---

Getting a list of users

```csharp
// existing using statements here //
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.eShopWeb.PublicApi.Extensions;

namespace Microsoft.eShopWeb.PublicApi.UserManagementEndpoints;

public class UserListEndpoint(UserManager<ApplicationUser> userManager):EndpointWithoutRequest<UserListResponse>
{

    public override void Configure()
    {
        Get("api/users");
        Roles(BlazorShared.Authorization.Constants.Roles.ADMINISTRATORS);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Description(d => d.Produces<UserListResponse>()
        .WithTags("UserManagementEndpoints"));
    }

    public override async Task<UserListResponse> ExecuteAsync(CancellationToken ct)
    {
        await Task.Delay(1000, ct);
        var response = new UserListResponse();
        var users = userManager.Users.ToList();
        foreach ( var user in users)
        {
            response.Users.Add(user.ToUserDto());
        }
        return response;
    }
}
```

so this method of getting the users directly access the user manager instead of using the mediator pattern.
This is similiar to the way the login was implemented. My question is does this follow the clean architecture
concept? And if I want it implemented using the mediator pattern how do I do that since I don't have access to the
application user?

for the form definition json string and handling the database table and also building the C# classes i need to look for the content tag for each child, which have an id so that may help.
