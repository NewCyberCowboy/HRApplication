using System;

namespace HRApplication.Models
{
    public class JobApplication
    {
        public int ApplicationId { get; set; }
        public int CandidateId { get; set; }
        public int VacancyId { get; set; }
        public DateTime AppliedAt { get; set; }
        public string Status { get; set; }

        // Navigation properties
        public string CandidateName { get; set; }
        public string VacancyTitle { get; set; }
    }
}
