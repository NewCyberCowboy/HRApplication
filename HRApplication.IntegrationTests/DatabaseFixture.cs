using HRApplication.Repositories;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using Xunit;

namespace HRApplication.IntegrationTests
{
    public class DatabaseFixture : IDisposable
    {
        public string ConnectionString { get; private set; }
        public string TestDatabaseName { get; } = "hr_test";

        public DatabaseFixture()
        {
            ConnectionString = "Host=localhost;Database=hr;Username=postgres;Password=123;Port=5432";
            Console.WriteLine($"🔗 Connection string: {ConnectionString}");
            InitializeTestDatabase();
        }

        private void InitializeTestDatabase()
        {
            try
            {
                using var connection = new Npgsql.NpgsqlConnection(ConnectionString);
                connection.Open();
                Console.WriteLine($"✅ Database connection successful: {ConnectionString}");

                // Проверяем какие таблицы есть в БД
                var checkTablesSql = @"
            SELECT table_name 
            FROM information_schema.tables 
            WHERE table_schema = 'public' 
            AND table_type = 'BASE TABLE'";

                using var checkCmd = new Npgsql.NpgsqlCommand(checkTablesSql, connection);
                using var reader = checkCmd.ExecuteReader();

                Console.WriteLine("Tables in database:");
                while (reader.Read())
                {
                    Console.WriteLine($"  - {reader.GetString(0)}");
                }
                reader.Close();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Database check failed: {ex.Message}");
                throw;
            }
        }
        public void Dispose()
        {
            // Очистка после тестов
        }
    }

    [CollectionDefinition("Database")]
    public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
    {
    }
}