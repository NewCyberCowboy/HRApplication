using System;

namespace HRApplication.Models
{
    public class Vacancy
    {
        public int VacancyId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string RequiredSkills { get; set; }
        public int MinAge { get; set; }
        public int MaxAge { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
