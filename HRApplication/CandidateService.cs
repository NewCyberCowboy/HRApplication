using HRApplication.Interfaces;
using HRApplication.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HRApplication.Services
{
    public class CandidateService : ICandidateService
    {
        private readonly ICandidateRepository _candidateRepository;

        public CandidateService(ICandidateRepository candidateRepository)
        {
            _candidateRepository = candidateRepository;
        }

        public async Task<List<Candidate>> GetAllCandidatesAsync()
        {
            return await _candidateRepository.GetAllCandidatesAsync();
        }

        public async Task<List<Candidate>> FilterCandidatesAsync(string specialization, int? minAge, int? maxAge, string skills)
        {
            // Бизнес-логика валидации
            if (minAge.HasValue && minAge < 18)
                throw new System.ArgumentException("Минимальный возраст не может быть меньше 18 лет");

            if (maxAge.HasValue && maxAge > 70)
                throw new System.ArgumentException("Максимальный возраст не может быть больше 70 лет");

            return await _candidateRepository.FilterCandidatesAsync(specialization, minAge, maxAge, skills);
        }

        public Task<bool> ValidateCandidateAsync(Candidate candidate)
        {
            if (string.IsNullOrWhiteSpace(candidate.FirstName))
                return Task.FromResult(false);

            if (string.IsNullOrWhiteSpace(candidate.LastName))
                return Task.FromResult(false);

            if (candidate.Age < 18 || candidate.Age > 70)
                return Task.FromResult(false);

            return Task.FromResult(true);
        }
    }
}