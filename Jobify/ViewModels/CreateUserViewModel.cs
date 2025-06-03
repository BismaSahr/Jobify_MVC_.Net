using System.ComponentModel.DataAnnotations;

namespace Jobify.ViewModels
{
    public class CreateUserViewModel
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [Display(Name = "Role")]
        public string Role { get; set; } // Admin, Employer, JobSeeker
    }
}
