using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using HRApplication.Models;
using System.Linq;

namespace HRApplication.Database
{
    public class DatabaseHelper
    {
        private NpgsqlConnection connection;
        private string connectionString;

        public bool Connect(string host, string port, string database, string username, string password)
        {
            try
            {
                connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password}";
                connection = new NpgsqlConnection(connectionString);
                connection.Open();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public void Disconnect()
        {
            connection?.Close();
        }

        // Кандидаты
        public List<Candidate> GetCandidates(string specialization = null, string skills = null,
            int? minAge = null, int? maxAge = null, int? vacancyId = null)
        {
            var candidates = new List<Candidate>();

            try
            {
                string query = @"
                    SELECT DISTINCT c.* 
                    FROM candidates c
                    LEFT JOIN applications a ON c.candidate_id = a.candidate_id
                    WHERE 1=1";

                var parameters = new List<NpgsqlParameter>();

                if (!string.IsNullOrEmpty(specialization))
                {
                    query += " AND c.specialization ILIKE @specialization";
                    parameters.Add(new NpgsqlParameter("@specialization", $"%{specialization}%"));
                }

                if (!string.IsNullOrEmpty(skills))
                {
                    query += " AND c.skills ILIKE @skills";
                    parameters.Add(new NpgsqlParameter("@skills", $"%{skills}%"));
                }

                if (minAge.HasValue)
                {
                    query += " AND c.age >= @minAge";
                    parameters.Add(new NpgsqlParameter("@minAge", minAge.Value));
                }

                if (maxAge.HasValue)
                {
                    query += " AND c.age <= @maxAge";
                    parameters.Add(new NpgsqlParameter("@maxAge", maxAge.Value));
                }

                if (vacancyId.HasValue)
                {
                    query += " AND a.vacancy_id = @vacancyId";
                    parameters.Add(new NpgsqlParameter("@vacancyId", vacancyId.Value));
                }

                query += " ORDER BY c.first_name, c.last_name";

                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddRange(parameters.ToArray());

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            candidates.Add(new Candidate
                            {
                                CandidateId = reader.GetInt32(reader.GetOrdinal("candidate_id")),
                                FirstName = reader.GetString(reader.GetOrdinal("first_name")),
                                LastName = reader.GetString(reader.GetOrdinal("last_name")),
                                Age = reader.GetInt32(reader.GetOrdinal("age")),
                                Specialization = reader.GetString(reader.GetOrdinal("specialization")),
                                Skills = reader.GetString(reader.GetOrdinal("skills")),
                                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"))
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке кандидатов: {ex.Message}");
            }

            return candidates;
        }

        // Вакансии
        public List<Vacancy> GetVacancies()
        {
            var vacancies = new List<Vacancy>();

            try
            {
                string query = "SELECT * FROM vacancies ORDER BY title";

                using (var cmd = new NpgsqlCommand(query, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        vacancies.Add(new Vacancy
                        {
                            VacancyId = reader.GetInt32(reader.GetOrdinal("vacancy_id")),
                            Title = reader.GetString(reader.GetOrdinal("title")),
                            Description = reader.GetString(reader.GetOrdinal("description")),
                            RequiredSkills = reader.GetString(reader.GetOrdinal("required_skills")),
                            MinAge = reader.GetInt32(reader.GetOrdinal("min_age")),
                            MaxAge = reader.GetInt32(reader.GetOrdinal("max_age")),
                            CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"))
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке вакансий: {ex.Message}");
            }

            return vacancies;
        }

        // Отклики для кандидата
        public List<JobApplication> GetApplicationsByCandidate(int candidateId)
        {
            var applications = new List<JobApplication>();

            try
            {
                string query = @"
                    SELECT a.*, v.title as vacancy_title
                    FROM applications a
                    JOIN vacancies v ON a.vacancy_id = v.vacancy_id
                    WHERE a.candidate_id = @candidateId
                    ORDER BY a.applied_at DESC";

                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@candidateId", candidateId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            applications.Add(new JobApplication
                            {
                                ApplicationId = reader.GetInt32(reader.GetOrdinal("application_id")),
                                CandidateId = reader.GetInt32(reader.GetOrdinal("candidate_id")),
                                VacancyId = reader.GetInt32(reader.GetOrdinal("vacancy_id")),
                                AppliedAt = reader.GetDateTime(reader.GetOrdinal("applied_at")),
                                Status = reader.GetString(reader.GetOrdinal("status")),
                                VacancyTitle = reader.GetString(reader.GetOrdinal("vacancy_title"))
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке откликов: {ex.Message}");
            }

            return applications;
        }

        // Все отклики
        public List<JobApplication> GetAllApplications()
        {
            var applications = new List<JobApplication>();

            try
            {
                string query = @"
                    SELECT a.*, c.first_name, c.last_name, v.title as vacancy_title
                    FROM applications a
                    JOIN candidates c ON a.candidate_id = c.candidate_id
                    JOIN vacancies v ON a.vacancy_id = v.vacancy_id
                    ORDER BY a.applied_at DESC";

                using (var cmd = new NpgsqlCommand(query, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        applications.Add(new JobApplication
                        {
                            ApplicationId = reader.GetInt32(reader.GetOrdinal("application_id")),
                            CandidateId = reader.GetInt32(reader.GetOrdinal("candidate_id")),
                            VacancyId = reader.GetInt32(reader.GetOrdinal("vacancy_id")),
                            AppliedAt = reader.GetDateTime(reader.GetOrdinal("applied_at")),
                            Status = reader.GetString(reader.GetOrdinal("status")),
                            CandidateName = $"{reader.GetString(reader.GetOrdinal("first_name"))} {reader.GetString(reader.GetOrdinal("last_name"))}",
                            VacancyTitle = reader.GetString(reader.GetOrdinal("vacancy_title"))
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке откликов: {ex.Message}");
            }

            return applications;
        }

        public bool IsConnected()
        {
            return connection?.State == ConnectionState.Open;
        }
    }
}