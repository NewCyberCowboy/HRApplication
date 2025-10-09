using FlaUI.Core;
using FlaUI.UIA3;
using System;
using System.IO;
using System.Threading;

namespace HRApplication.UITests
{
    public class UITestBase : IDisposable
    {
        protected FlaUI.Core.Application App { get; private set; }
        protected UIA3Automation Automation { get; private set; }

        public UITestBase()
        {
            
            var appPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "..", "..", "..", "..",
                "HRApplication", "bin", "Debug",
                "HRApplication.exe");

            Console.WriteLine($"🔍 Looking for: {appPath}");
            Console.WriteLine($"📁 File exists: {File.Exists(appPath)}");

            if (!File.Exists(appPath))
            {
                throw new FileNotFoundException($"Application not found at: {appPath}");
            }

            // Запускаем приложение
            App = FlaUI.Core.Application.Launch(appPath);
            Automation = new UIA3Automation();

            // Ждем загрузки приложения
            Thread.Sleep(3000);

            // Проверяем что окно загрузилось
            var mainWindow = App.GetMainWindow(Automation);
            Console.WriteLine($"✅ Main window loaded: {mainWindow?.Title}");
        }

        public void Dispose()
        {
            Automation?.Dispose();

            // Закрываем приложение
            try
            {
                App?.Close();
            }
            catch
            {
                // Если не закрывается - убиваем процесс
                App?.Kill();
            }
        }
    }
}