namespace NexaLearn.DTOs;

public record TokenDto(string Token, DateTime ExpiraEn, string UserId, string Email, IReadOnlyList<string> Roles);
