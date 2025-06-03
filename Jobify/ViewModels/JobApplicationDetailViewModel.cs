// File: Jobify.ViewModels/JobApplicationDetailViewModel.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace Jobify.ViewModels
{
    public class JobApplicationDetailViewModel
    {
        public int ApplicationId { get; set; }

        // Job Post Details
        public int JobId { get; set; }
        public string JobTitle { get; set; }
        public string JobCategoryName { get; set; }
        public string JobLocationName { get; set; }
        public DateTime JobCreatedAt { get; set; } // Matches JobPost.CreatedAt

        // Job Seeker Details (from User model linked via JobSeeker)
        public int JobSeekerId { get; set; }
        public string JobSeekerFullName { get; set; }
        public string JobSeekerEmail { get; set; }
        public string JobSeekerPhone { get; set; } // Assuming User.Phone
        public string JobSeekerGender { get; set; } // Assuming JobSeeker has Gender

        // Application Details (data provided in application form)
        public string FullName { get; set; } // Name on application form
        public string Email { get; set; }    // Email on application form
        public string Phone { get; set; }    // Phone on application form
        public string Skills { get; set; }
        public string CoverLetter { get; set; }
        public string ResumeLink { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd hh:mm tt}")]
        public DateTime AppliedAt { get; set; }
    }
}