// In ViewModels/JobApplicationViewModel.cs

using System;
using System.ComponentModel.DataAnnotations;
using Jobify.Models;

namespace Jobify.ViewModels
{
    public class JobApplicationViewModel
    {
        public int JobId { get; set; }
        public string JobTitle { get; set; }
        public string CompanyName { get; set; }
        public string CategoryName { get; set; }
        public string Location { get; set; }

        public int JobSeekerId { get; set; }
        public string JobSeekerFullName { get; set; }
        public string JobSeekerEmail { get; set; }
        public string JobSeekerPhone { get; set; }

        public string Skills { get; set; }
        public string CoverLetter { get; set; }
        public string ResumeLink { get; set; }

        public DateTime? AppliedAt { get; set; }
    }

}