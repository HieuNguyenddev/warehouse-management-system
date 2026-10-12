using System.ComponentModel.DataAnnotations;
using UserManagementAPI.Common.Dtos;

namespace UserManagementAPI.Modules.Auth.DTOs;

public class LoginRequest
{
    [NotBlank(ErrorMessage = "Email là bắt buộc.")]
    [StringLength(200, ErrorMessage = "Email tối đa 200 ký tự.")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    public string Email { get; set; } = string.Empty;

    [NotBlank(ErrorMessage = "Mật khẩu là bắt buộc.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu từ 8 đến 100 ký tự.")]
    public string Password { get; set; } = string.Empty;
}
