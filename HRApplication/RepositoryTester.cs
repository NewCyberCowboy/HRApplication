
using HRApplication.Repositories;
using System;
using System.Windows.Forms;

public class RepositoryTester
{
    public static async void TestNewRepository()
    {
        try
        {
           
            var connectionString = "Host=localhost;Port=5432;Database=hr_management;Username=postgres;Password=123";

            var repository = new CandidateRepository(connectionString);
            var candidates = await repository.GetAllCandidatesAsync();

            MessageBox.Show($"Новый репозиторий загрузил {candidates.Count} кандидатов", "Тест",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка тестирования: {ex.Message}", "Ошибка",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}