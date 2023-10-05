using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Security.Principal;

namespace Weather_by_location
{
    public partial class Form1 : Form
    {
        readonly WeatherService weatherService = new WeatherService();

        Dictionary<string, string> languages = new Dictionary<string, string>()
        {
            {"Russian", "ru"},
            {"English", "en"},
        };
        readonly string userName = WindowsIdentity.GetCurrent().Name;
        public Form1()
        {
            InitializeComponent();
            Logger.LogInformation($"User: {userName}. The application has been successfully launched.");
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem == null)
            {
                Logger.DisplayError("Выберите язык!!!\nChoose a language!!!");
                Logger.LogError("No language selected.");
                return;
            }
            string language = languages[comboBox1.SelectedItem.ToString()];
            Logger.LogInformation($"User: {userName}. The interface language is selected: {comboBox1.SelectedItem}.");
            try
            {
                if (string.IsNullOrEmpty(textBox1.Text))
                {
                    if (language == "ru")
                        Logger.DisplayError("Введите город!!!");
                    else
                        Logger.DisplayError("Enter the city!!!");
                    Logger.LogError("The city was entered incorrectly.");
                    return;
                }

                string locality = textBox1.Text;
                Logger.LogInformation($"User: {userName}. The user entered the city: {locality}.");

                string geocodingResult = await weatherService.GetGeocodingDataAsync(locality, language);

                if (string.IsNullOrEmpty(geocodingResult))
                {
                    if (language == "ru")
                        Logger.DisplayError("Ошибка при получении данных о местоположении!!!");
                    else
                        Logger.DisplayError("Error receiving location data!!!");
                    Logger.LogError("Error receiving location data.");
                    return;
                }
                webBrowser1.DocumentText = await weatherService.UpdateWeatherUI(weatherService.ProcessGeocodingData(geocodingResult), language);

            }
            catch (FormatException ex)
            {
                Logger.LogError($"Data conversion error: {ex}");
                Logger.DisplayError(language == "ru" ? "Ошибка в формате данных. Пожалуйста, проверьте ответ от сервера." : "Error in the data format. Please check the response from the server.");
                return;
            }
            catch (Exception ex)
            {
                Logger.LogError($"An unknown error has occurred: {ex}");
                Logger.DisplayError(language == "ru" ? "Произошла неизвестная ошибка." : "An unknown error has occurred.");
                return;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            LanguageSettings.HandleLanguageChange(comboBox1, label1, label2, button1);
        }
    }
}
