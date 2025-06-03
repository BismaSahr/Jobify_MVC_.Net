using System;

namespace Jobify.Models
{
    public class UserProfileViewModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; }
        public DateTime? CreatedAt { get; set; }

        // Admin details
        public string Department { get; set; }

        // Employer details
        public string CompanyName { get; set; }
        public string Website { get; set; }
        public string Address { get; set; }
        public string Designation { get; set; }

        // Job Seeker details
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }
}
