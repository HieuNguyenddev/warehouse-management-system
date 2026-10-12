namespace UserManagementAPI.Modules.Auth.DTOs;

public class LoginResponse : JwtTokenDto
{
    public required UserSession User { get; set; }
}
