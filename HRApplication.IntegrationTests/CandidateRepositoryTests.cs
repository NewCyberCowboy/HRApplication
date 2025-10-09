using HRApplication.Models;
using HRApplication.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using NpgsqlTypes;

namespace HRApplication.IntegrationTests
{
    [Collection("Database")]
    public class CandidateRepositoryTests
    {
        private readonly DatabaseFixture _fixture;

        public CandidateRepositoryTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task GetAllCandidatesAsync_ShouldReturnCandidates_WhenDatabaseHasData()
        {
            // Arrange
            var repository = new CandidateRepository(_fixture.ConnectionString);

            // Act
            var result = await repository.GetAllCandidatesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<Candidate>>(result);

            // Если в БД есть данные - проверяем структуру объектов
            if (result.Any())
            {
                var firstCandidate = result.First();
                Assert.NotNull(firstCandidate.FirstName);
                Assert.NotNull(firstCandidate.LastName);
                Assert.True(firstCandidate.Age > 0);
            }
        }

        [Fact]
        public async Task FilterCandidatesAsync_ShouldReturnFilteredResults_WithSpecialization()
        {
            // Arrange
            var repository = new CandidateRepository(_fixture.ConnectionString);

            // Act
            var result = await repository.FilterCandidatesAsync("Developer", null, null, null);

            // Assert
            Assert.NotNull(result);
            // Проверяем что все возвращенные кандидаты имеют нужную специализацию
            Assert.All(result, candidate =>
                Assert.Equal("Developer", candidate.Specialization));
        }

        [Fact]
        public async Task FilterCandidatesAsync_ShouldReturnEmpty_WhenNoMatches()
        {
            // Arrange
            var repository = new CandidateRepository(_fixture.ConnectionString);

            // Act - ищем несуществующую специализацию
            var result = await repository.FilterCandidatesAsync("NonExistentSpecialization", null, null, null);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task FilterCandidatesAsync_ShouldHandleNullParameters()
        {
            // Arrange
            var repository = new CandidateRepository(_fixture.ConnectionString);

            // Act - все параметры null = получить всех кандидатов
            var result = await repository.FilterCandidatesAsync(null, null, null, null);

            // Assert
            Assert.NotNull(result);
            // Должен вернуть хотя бы пустой список, не исключение
        }
    }
}