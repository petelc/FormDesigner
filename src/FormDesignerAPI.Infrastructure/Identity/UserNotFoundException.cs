using System;

namespace FormDesignerAPI.Infrastructure.Identity;

public class UserNotFoundException : Exception
{
    public UserNotFoundException(string userName)
        : base($"User with username '{userName}' not found.") { }

}
