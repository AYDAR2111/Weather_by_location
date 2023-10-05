using System;
using System.Windows.Forms;

namespace Weather_by_location
{
    public class LanguageSettings
    {
        // При выборе русского языка
        private static void SetRussianLanguage(Label label1, Label label12, Button button1)
        {
            try
            {
                label1.Text = Properties.Russian.Label1Text;
                label12.Text = Properties.Russian.Label2Text;
                button1.Text = Properties.Russian.ButtonCaption;
                MessageBox.Show("Язык интерфейса изменен на русский.", "Уведомление", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Logger.LogError($"An unknown error has occurred: {ex}");
            }
        }

        // При выборе английского языка
        private static void SetEnglishLanguage(Label label1, Label label12, Button button1)
        {
            try
            {
                label1.Text = Properties.English.Label1Text;
                label12.Text = Properties.English.Label2Text;
                button1.Text = Properties.English.ButtonCaption;
                MessageBox.Show("The interface language has been changed to English.", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Logger.LogError($"An unknown error has occurred: {ex}");
            }
        }
        public static void HandleLanguageChange(ComboBox comboBox, Label label1, Label label12, Button button1)
        {
            try
            {
                string selectedLanguage = comboBox.SelectedItem.ToString();

                if (selectedLanguage == "Russian")
                    SetRussianLanguage(label1, label12, button1);
                else
                    SetEnglishLanguage(label1, label12, button1);
            }
            catch (Exception ex)
            {
                Logger.LogError($"An unknown error has occurred: {ex}");
            }
        }
    }
}
