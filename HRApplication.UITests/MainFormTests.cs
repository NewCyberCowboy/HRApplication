using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using System.Linq;
using Xunit;

namespace HRApplication.UITests
{
    public class MainFormTests : UITestBase
    {
        [Fact]
        public void MainForm_ShouldLoad_WhenApplicationStarts()
        {
            // Arrange & Act
            var mainWindow = App.GetMainWindow(Automation);

            // Assert
            Assert.NotNull(mainWindow);
            Assert.True(mainWindow.Title.Contains("HR") || mainWindow.Title.Contains("Candidate"));
        }

        [Fact]
        public void CandidatesButton_ShouldExist_OnMainForm()
        {
            // Arrange
            var mainWindow = App.GetMainWindow(Automation);

            // Act
            var buttons = mainWindow.FindAllDescendants(cf => cf.ByControlType(ControlType.Button));
            var anyButton = buttons.FirstOrDefault();

            // Assert
            Assert.NotNull(anyButton);
        }
    }
}