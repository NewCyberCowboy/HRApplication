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

        public bool Connect(string host, string port, string database, string username, string password)
        {
            try
            {
                string safePassword = string.IsNullOrEmpty(password) ? "" : password;
                string connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={safePassword};Timeout=30";

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

        public List<Candidate> GetCandidates()
        {
            var candidates = new List<Candidate>();

            try
            {
                string query = "SELECT * FROM candidates ORDER BY first_name, last_name";

                using (var cmd = new NpgsqlCommand(query, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
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
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке кандидатов: {ex.Message}");
            }

            return candidates;
        }

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
                            VacancyId = Convert.ToInt32(reader["vacancy_id"]),
                            Title = reader["title"].ToString(),
                            Description = reader["description"].ToString(),
                            RequiredSkills = reader["required_skills"].ToString(),
                            MinAge = Convert.ToInt32(reader["min_age"]),
                            MaxAge = Convert.ToInt32(reader["max_age"]),
                            CreatedAt = Convert.ToDateTime(reader["created_at"])
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
                                ApplicationId = Convert.ToInt32(reader["application_id"]),
                                CandidateId = Convert.ToInt32(reader["candidate_id"]),
                                VacancyId = Convert.ToInt32(reader["vacancy_id"]),
                                AppliedAt = Convert.ToDateTime(reader["applied_at"]),
                                Status = reader["status"].ToString(),
                                VacancyTitle = reader["vacancy_title"].ToString()
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

        // ДОБАВЛЕННЫЙ МЕТОД GetAllApplications
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
                            ApplicationId = Convert.ToInt32(reader["application_id"]),
                            CandidateId = Convert.ToInt32(reader["candidate_id"]),
                            VacancyId = Convert.ToInt32(reader["vacancy_id"]),
                            AppliedAt = Convert.ToDateTime(reader["applied_at"]),
                            Status = reader["status"].ToString(),
                            CandidateName = $"{reader["first_name"]} {reader["last_name"]}",
                            VacancyTitle = reader["vacancy_title"].ToString()
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