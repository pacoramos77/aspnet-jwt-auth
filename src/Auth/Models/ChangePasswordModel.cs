using System.ComponentModel.DataAnnotations;

namespace Auth.Models;

public class ChangePasswordModel
{
    [Required(ErrorMessage = "Username is required")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "CurrentPassword is required")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "NewPassword is required")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "ConfirmNewPassword is required")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}