using HRApplication.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HRApplication.Services
{
    public interface ICandidateService
    {
        Task<List<Candidate>> GetAllCandidatesAsync();
        Task<List<Candidate>> FilterCandidatesAsync(string specialization, int? minAge, int? maxAge, string skills);
        Task<bool> ValidateCandidateAsync(Candidate candidate);
    }
}