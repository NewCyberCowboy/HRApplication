using HRApplication.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HRApplication.Interfaces
{
    public interface ICandidateRepository
    {
        Task<List<Candidate>> GetAllCandidatesAsync();
        Task<List<Candidate>> FilterCandidatesAsync(string specialization, int? minAge, int? maxAge, string skills);
        Task<Candidate> GetCandidateByIdAsync(int id);
        Task<int> AddCandidateAsync(Candidate candidate);
        Task<bool> UpdateCandidateAsync(Candidate candidate);
        Task<bool> DeleteCandidateAsync(int id);
    }
}