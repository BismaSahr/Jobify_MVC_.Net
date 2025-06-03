using System;
using System.Collections.Generic;
using Jobify.Models;

namespace Jobify.ViewModels
{
    public class JobSeekerDashboardViewModel
    {
        public JobSeeker JobSeekerDetails { get; set; }

        public List<JobApplication> Applications { get; set; } // This line is correct and should be kept

        public int TotalApplications { get; set; }

        // Added back based on your last provided ViewModel, if you intend to use it.
        // The controller logic previously commented it out, so ensure your View also uses it.
        public string LatestApplicationDate { get; set; }

        // --- Properties for Charts (These were missing and are crucial for the dashboard to work) ---

        // Data for Applications Trend Chart (Last 7 Days)
        public List<string> ApplicationsByWeekLabels { get; set; }
        public List<int> ApplicationsByWeekData { get; set; }

        // Data for Applications Trend Chart (Last 30 Days/Monthly)
        public List<string> ApplicationsByMonthLabels { get; set; }
        public List<int> ApplicationsByMonthData { get; set; }

        // Data for Application Status Chart
   
        public List<string> ApplicationStatusLabels { get; set; }
        public List<int> ApplicationStatusData { get; set; }

        // New property for Overall Summary Chart
        public int TotalAvailableJobPosts { get; set; } // To get the total number of jobs available in the system
    }
}