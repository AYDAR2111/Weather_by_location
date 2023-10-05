using Serilog;
using System.Windows.Forms;

namespace Weather_by_location
{
    public static class Logger
    {
        private static readonly ILogger logger_Info = new LoggerConfiguration()
            .MinimumLevel.Information() // Установка уровня логирования
            .WriteTo.File("Info.log")
            .CreateLogger();

        private static readonly ILogger logger_Error = new LoggerConfiguration()
            .MinimumLevel.Error() // Установка уровня логирования
            .WriteTo.File("Error.log")
            .CreateLogger();

        public static void LogInformation(string informationmessage)
        {
            logger_Info.Information($"{informationmessage}");
        }

        public static void LogError(string errorMessage)
        {
            logger_Error.Error($"{errorMessage}");
        }

        public static void DisplayError(string errorMessage)
        {
            MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
