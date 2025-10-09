using System;
using System.Windows.Forms;
using HRApplication;
using HRApplication.Interfaces;
using HRApplication.Repositories;
using HRApplication.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HRApplication
{
    internal static class Program
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ModernConnectionForm());
        }

        // Новый метод для конфигурации сервисов (будем использовать позже)
        public static void ConfigureServices()
        {
            var services = new ServiceCollection();

            // Регистрируем репозитории
            services.AddScoped<ICandidateRepository>(provider =>
                new CandidateRepository(ModernConnectionForm.CurrentConnectionString));

            // Регистрируем сервисы
            services.AddScoped<ICandidateService, CandidateService>();

            // Регистрируем формы
            services.AddTransient<ModernMainForm>();

            ServiceProvider = services.BuildServiceProvider();
        }
    }
}