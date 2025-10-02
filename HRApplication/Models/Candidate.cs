using System;

namespace HRApplication.Models
{
    public class Candidate
    {
        public int CandidateId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public string Specialization { get; set; }
        public string Skills { get; set; }
        public DateTime CreatedAt { get; set; }

        public string FullName => $"{FirstName} {LastName}";
    }
}
