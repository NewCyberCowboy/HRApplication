// HRApplication/Repositories/CandidateRepository.cs
using HRApplication.Interfaces;
using HRApplication.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using NpgsqlTypes;

namespace HRApplication.Repositories
{

    public class CandidateRepository : ICandidateRepository
    {
        private readonly string _connectionString;

        public CandidateRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<List<Candidate>> GetAllCandidatesAsync()
        {
            var candidates = new List<Candidate>();

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string query = "SELECT * FROM candidates ORDER BY first_name, last_name";

                    using (var cmd = new NpgsqlCommand(query, connection))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            candidates.Add(new Candidate
                            {
                                CandidateId = Convert.ToInt32(reader["candidate_id"]),
                                FirstName = reader["first_name"].ToString(),
                                LastName = reader["last_name"].ToString(),
                                Age = Convert.ToInt32(reader["age"]),
                                Specialization = reader["specialization"].ToString(),
                                Skills = reader["skills"].ToString(),
                                CreatedAt = Convert.ToDateTime(reader["created_at"])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Пока просто логируем, без MessageBox в репозитории
                Console.WriteLine($"Ошибка при загрузке кандидатов: {ex.Message}");
                throw;
            }

            return candidates;
        }

        public async Task<List<Candidate>> FilterCandidatesAsync(string specialization, int? minAge, int? maxAge, string skills)
        {
            var candidates = new List<Candidate>();

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var sql = @"
                        SELECT candidate_id, first_name, last_name, age, specialization, skills, created_at
                        FROM candidates 
                        WHERE (@specialization IS NULL OR specialization = @specialization)
                          AND (@minAge IS NULL OR age >= @minAge)
                          AND (@maxAge IS NULL OR age <= @maxAge)
                          AND (@skills IS NULL OR skills ILIKE @skills)
                        ORDER BY first_name, last_name";

                    using (var cmd = new NpgsqlCommand(sql, connection))
                    {
                        cmd.Parameters.Add(new NpgsqlParameter("specialization", NpgsqlDbType.Varchar)
                        { Value = specialization ?? (object)DBNull.Value });

                        cmd.Parameters.Add(new NpgsqlParameter("minAge", NpgsqlTypes.NpgsqlDbType.Integer)
                        { Value = minAge ?? (object)DBNull.Value });

                        cmd.Parameters.Add(new NpgsqlParameter("maxAge", NpgsqlTypes.NpgsqlDbType.Integer)
                        { Value = maxAge ?? (object)DBNull.Value });

                        // ИСПРАВЛЕННАЯ СТРОКА - явно указываем тип для skills:
                        cmd.Parameters.Add(new NpgsqlParameter("skills", NpgsqlTypes.NpgsqlDbType.Text)
                        { Value = skills != null ? $"%{skills}%" : (object)DBNull.Value });

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                candidates.Add(new Candidate
                                {
                                    CandidateId = Convert.ToInt32(reader["candidate_id"]),
                                    FirstName = reader["first_name"].ToString(),
                                    LastName = reader["last_name"].ToString(),
                                    Age = Convert.ToInt32(reader["age"]),
                                    Specialization = reader["specialization"].ToString(),
                                    Skills = reader["skills"].ToString(),
                                    CreatedAt = Convert.ToDateTime(reader["created_at"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при фильтрации кандидатов: {ex.Message}");
                throw;
            }

            return candidates;
        }

        // Остальные методы можно добавить позже...
        public Task<Candidate> GetCandidateByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<int> AddCandidateAsync(Candidate candidate)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateCandidateAsync(Candidate candidate)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteCandidateAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}