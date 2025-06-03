using System;

namespace Jobify.Models
{
    public class UserEditProfileViewModel
    {
        // Common User properties
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        // Role (to decide which table to update)
        public string Role { get; set; }

        // Admin fields
        public string Department { get; set; }

        // JobSeeker fields
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }

        // Employer fields
        public string CompanyName { get; set; }
        public string Website { get; set; }
        public string Address { get; set; }
        public string Designation { get; set; }
    }
}
