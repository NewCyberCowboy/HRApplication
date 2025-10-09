using HRApplication.Interfaces;
using HRApplication.Models;
using HRApplication.Services;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace HRApplication.UnitTests
{
    public class CandidateServiceTests
    {
        private readonly Mock<ICandidateRepository> _mockRepo;
        private readonly CandidateService _candidateService;

        public CandidateServiceTests()
        {
            _mockRepo = new Mock<ICandidateRepository>();
            _candidateService = new CandidateService(_mockRepo.Object);
        }

        [Fact]
        public async Task GetAllCandidatesAsync_ShouldReturnCandidates_WhenRepositoryReturnsData()
        {
            // Arrange
            var expectedCandidates = new List<Candidate>
            {
                new Candidate { CandidateId = 1, FirstName = "John", LastName = "Doe", Age = 25 },
                new Candidate { CandidateId = 2, FirstName = "Jane", LastName = "Smith", Age = 30 }
            };

            _mockRepo.Setup(repo => repo.GetAllCandidatesAsync())
                    .ReturnsAsync(expectedCandidates);

            // Act
            var result = await _candidateService.GetAllCandidatesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("John", result[0].FirstName);
            _mockRepo.Verify(repo => repo.GetAllCandidatesAsync(), Times.Once);
        }

        [Fact]
        public async Task FilterCandidatesAsync_ShouldCallRepositoryWithCorrectParameters()
        {
            // Arrange
            var expectedCandidates = new List<Candidate>
            {
                new Candidate { CandidateId = 1, FirstName = "John", Age = 25, Specialization = "Developer" }
            };

            _mockRepo.Setup(repo => repo.FilterCandidatesAsync("Developer", 20, 30, "C#"))
                    .ReturnsAsync(expectedCandidates);

            // Act
            var result = await _candidateService.FilterCandidatesAsync("Developer", 20, 30, "C#");

            // Assert
            Assert.Single(result);
            _mockRepo.Verify(repo => repo.FilterCandidatesAsync("Developer", 20, 30, "C#"), Times.Once);
        }

        [Fact]
        public async Task FilterCandidatesAsync_ShouldHandleNullParameters()
        {
            // Arrange
            var expectedCandidates = new List<Candidate>
            {
                new Candidate { CandidateId = 1, FirstName = "John" }
            };

            _mockRepo.Setup(repo => repo.FilterCandidatesAsync(null, null, null, null))
                    .ReturnsAsync(expectedCandidates);

            // Act
            var result = await _candidateService.FilterCandidatesAsync(null, null, null, null);

            // Assert
            Assert.Single(result);
            _mockRepo.Verify(repo => repo.FilterCandidatesAsync(null, null, null, null), Times.Once);
        }

        [Fact]
        public async Task GetAllCandidatesAsync_ShouldReturnEmptyList_WhenRepositoryReturnsNoData()
        {
            // Arrange
            var emptyList = new List<Candidate>();
            _mockRepo.Setup(repo => repo.GetAllCandidatesAsync())
                    .ReturnsAsync(emptyList);

            // Act
            var result = await _candidateService.GetAllCandidatesAsync();

            // Assert
            Assert.Empty(result);
            _mockRepo.Verify(repo => repo.GetAllCandidatesAsync(), Times.Once);
        }
    }
}