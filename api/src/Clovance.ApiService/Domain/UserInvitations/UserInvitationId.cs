namespace Clovance.ApiService.Domain.UserInvitations;

public readonly record struct UserInvitationId(Guid Value)
{
    public static UserInvitationId New()
    {
        return new UserInvitationId(Guid.NewGuid());
    }
    public static UserInvitationId Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("User invitation id cannot be empty.", nameof(value));
        }
        return new UserInvitationId(value);
    }
    public override string ToString()
    {
        return Value.ToString();
    }
}
