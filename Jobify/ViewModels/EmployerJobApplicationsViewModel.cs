// File: Jobify.ViewModels/EmployerJobApplicationsViewModel.cs
using System.Collections.Generic;
using System.Linq;
using Jobify.Models; // Ensure this is correctly referenced

namespace Jobify.ViewModels
{
    // ViewModel to represent a single job post with its applications
    // This is the type that is "grouped" in the controller
    public class JobApplicationsGroupViewModel
    {
        public int JobId { get; set; }
        public string JobTitle { get; set; }
        public string CategoryName { get; set; }
        public string LocationName { get; set; }
        public System.DateTime? CreatedAt { get; set; } // Changed to nullable DateTime?

        public int TotalApplications { get; set; }
        public List<JobApplicationDetailViewModel> Applications { get; set; }
    }

    // Main ViewModel for the employer's applications dashboard
    // This is the top-level model passed to the EmployerDashboard view
    public class EmployerJobApplicationsViewModel
    {
        // These are the properties that were causing ambiguity.
        // Ensure they are defined ONLY ONCE here.
        public string EmployerName { get; set; }
        public List<JobApplicationsGroupViewModel> GroupedJobApplications { get; set; }
    }
}