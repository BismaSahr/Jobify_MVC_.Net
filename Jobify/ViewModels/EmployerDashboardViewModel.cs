// Jobify.ViewModels/EmployerDashboardViewModel.cs
using System.Collections.Generic;
using Jobify.Models; // Assuming your Employer model is here
using System.ComponentModel.DataAnnotations; // For any potential display attributes

namespace Jobify.ViewModels
{
    public class EmployerDashboardViewModel
    {
        public Employer EmployerDetails { get; set; }
        public List<JobPostViewModel> PostedJobs { get; set; } // Reusing your existing JobPostViewModel
        public int TotalPostedJobs { get; set; }
        public int TotalApplications { get; set; }
        // You can add more properties here, e.g.,
        // public int TotalApplicationsReceived { get; set; }
        public string LatestApplicationDate { get; set; }
        public List<string> JobPostGraphLabels { get; set; } // e.g., ["Monday", "Tuesday", ...]
        public List<int> JobPostGraphData { get; set; }
    }
}