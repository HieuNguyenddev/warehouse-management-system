using System.ComponentModel.DataAnnotations;
using UserManagementAPI.Common.Dtos;

namespace UserManagementAPI.Modules.Roles.DTOs;

public class UpsertRoleRequest
{
    [NotBlank(ErrorMessage = "Tên vai trò là bắt buộc.")]
    [StringLength(150, MinimumLength = 1, ErrorMessage = "Tên vai trò tối đa 150 ký tự.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Mô tả tối đa 500 ký tự.")]
    public string? Description { get; set; }
}
