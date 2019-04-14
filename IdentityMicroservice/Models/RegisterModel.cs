using System.ComponentModel.DataAnnotations;

namespace IdentityMicroservice
{
    public class RegisterModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage ="The {0} must be at least {2} and at max {1} characters long", MinimumLength = 0)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        public string ConfirmPassword { get; set; }

        [Required]
        public string Role { get; set; }
    }
}