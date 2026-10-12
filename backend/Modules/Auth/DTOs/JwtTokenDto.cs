namespace UserManagementAPI.Modules.Auth.DTOs;

public class JwtTokenDto
{
    public required string Token { get; set; }

    public required DateTime ExpiresAt { get; set; }

    public string TokenType { get; set; } = "Bearer";
}
