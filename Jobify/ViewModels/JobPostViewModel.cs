using System;
using System.ComponentModel.DataAnnotations;
namespace Jobify.ViewModels
{
    public class JobPostViewModel
    {
        public int JobId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string CategoryName { get; set; }
        public string LocationName { get; set; }
        public string SalaryRange { get; set; }
        // Make these nullable
        public DateTime? Deadline { get; set; }
        public DateTime? CreatedAt { get; set; }

    }
}
