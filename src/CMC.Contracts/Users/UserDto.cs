namespace CMC.Contracts.Users;

public record UserDto(
  Guid Id, string Email, string FirstName, string LastName, bool IsEmailConfirmed, DateTime CreatedAt, DateTime
  ? LastLoginAt);
