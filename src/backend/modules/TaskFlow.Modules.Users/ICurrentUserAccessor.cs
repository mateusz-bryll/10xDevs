namespace TaskFlow.Modules.Users;

public interface ICurrentUserAccessor
{
    User GetCurrentUser();
}

internal sealed class DevelopmentCurrentUserAccessor : ICurrentUserAccessor
{
    public User GetCurrentUser()
    {
        return new User(UserId.Parse("auth0|0000000000000000"), "example@domain.local", "Example User", "https://place-hold.it/250");
    }
}