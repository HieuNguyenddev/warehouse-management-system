using System.ComponentModel.DataAnnotations;
using UserManagementAPI.Common.Dtos;

namespace UserManagementAPI.Modules.Users.DTOs;

public class UserWriteRequest
{
    [NotBlank(ErrorMessage = "Họ tên là bắt buộc.")]
    [StringLength(150, MinimumLength = 1, ErrorMessage = "Họ tên tối đa 150 ký tự.")]
    public string FullName { get; set; } = string.Empty;

    [NotBlank(ErrorMessage = "Email là bắt buộc.")]
    [StringLength(200, ErrorMessage = "Email tối đa 200 ký tự.")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    public string Email { get; set; } = string.Empty;

    [OptionalPhone(ErrorMessage = "Số điện thoại không hợp lệ.")]
    public string? Phone { get; set; }

    public Guid? RoleId { get; set; }

    public bool IsActive { get; set; }
}

public sealed class CreateUserRequest : UserWriteRequest
{
    [NotBlank(ErrorMessage = "Mật khẩu là bắt buộc.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu từ 8 đến 100 ký tự.")]
    public string Password { get; set; } = string.Empty;
}

public sealed class UpdateUserRequest : UserWriteRequest
{
    [OptionalPassword(ErrorMessage = "Mật khẩu từ 8 đến 100 ký tự.")]
    public string? Password { get; set; }
}
