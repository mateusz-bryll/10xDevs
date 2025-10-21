using StronglyTypedIds;

namespace TaskFlow.Modules.Users;

[StronglyTypedId(Template.String, "string-efcore")]
public partial struct UserId { }

public sealed record User(UserId Id, string Email, string Name, string Picture);