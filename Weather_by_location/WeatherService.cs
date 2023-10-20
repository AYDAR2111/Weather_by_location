using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Weather_by_location.Properties;

namespace Weather_by_location
{
    public class WeatherService
    {
        public async Task<string> GetGeocodingDataAsync(string locality, string language)
        {
            using (HttpClient client = new HttpClient())
            {
                string JSON = "";
                try
                {
                    Uri site = new Uri($"https://geocoding-api.open-meteo.com/v1/search?name={locality}&count=1&language={language}&format=json");
                    HttpResponseMessage response = await client.GetAsync(site);

                    if (response.IsSuccessStatusCode)
                        JSON = await response.Content.ReadAsStringAsync();
                    else
                    {
                        Logger.LogError($"HTTP error when requesting to {site}: {response.StatusCode}");
                        Logger.DisplayError(language == "ru" ? $"Ошибка HTTP: {response.StatusCode}" : $"HTTP Error: {response.StatusCode}");
                        return string.Empty;
                    }
                }
                catch (HttpRequestException ex)
                {
                    Logger.LogError($"HTTP request error: {ex}");
                    Logger.DisplayError(language == "ru" ? "Ошибка HTTP при запросе. Пожалуйста, проверьте интернет-соединение." : "HTTP error on request.Please check your internet connection.");
                }
                catch (FormatException ex)
                {
                    Logger.LogError($"Data conversion error: {ex}");
                    Logger.DisplayError(language == "ru" ? "Ошибка в формате данных. Пожалуйста, проверьте ответ от сервера." : "Error in the data format. Please check the response from the server.");
                }
                catch (Exception ex)
                {
                    Logger.LogError($"An unknown error has occurred.: {ex}");
                    Logger.DisplayError(language == "ru" ? "Произошла неизвестная ошибка." : "An unknown error has occurred.");
                }
                return JSON;
            }
        }

        public WeatherData ProcessGeocodingData(string geocodingResult)
        {
            // Разбор JSON данных о местоположении и получение информации о местоположении
            JObject geocoding = JObject.Parse(geocodingResult);
            JArray geocodingData = JArray.Parse(geocoding["results"].ToString());
            JObject geocodingResults = JObject.Parse(geocodingData[0].ToString());

            string locality = geocodingResults["name"].ToString();
            double latitude = (double)geocodingResults["latitude"];
            double longitude = (double)geocodingResults["longitude"];

            WeatherData weatherData = new WeatherData
            {
                Locality = locality,
                Latitude = latitude,
                Longitude = longitude,
            };
            return weatherData;
        }

        public async Task<string> UpdateWeatherUI(WeatherData weatherData, string language)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // Запрос к API для получения погодных данных
                    Uri forecastUri = new Uri($"https://api.open-meteo.com/v1/forecast?latitude={weatherData.Latitude.ToString().Replace(",", ".")}&longitude={weatherData.Longitude.ToString().Replace(",", ".")}&hourly=temperature_2m,relativehumidity_2m,apparent_temperature,precipitation_probability,surface_pressure,cloudcover,windspeed_10m,winddirection_10m,uv_index,is_day&daily=sunrise,sunset&windspeed_unit=ms&timezone=auto&forecast_days=1");
                    HttpResponseMessage response = await client.GetAsync(forecastUri);

                    if (response.IsSuccessStatusCode)
                    {
                        string forecastJson = await response.Content.ReadAsStringAsync();
                        JObject forecastData = JObject.Parse(forecastJson);
                        // Обновление информации о погоде
                        string html = UpdateWeather(weatherData, forecastData, language);
                        return html;
                    }
                    else
                    {
                        Logger.LogError($"HTTP error when requesting to {forecastUri}: {response.StatusCode}");
                        Logger.DisplayError(language == "ru" ? $"Ошибка HTTP: {response.StatusCode}" : $"HTTP Error: {response.StatusCode}");
                        return string.Empty;
                    }
                }
                catch (HttpRequestException ex)
                {
                    Logger.LogError($"HTTP request error: {ex}");
                    Logger.DisplayError(language == "ru" ? "Ошибка HTTP при запросе. Пожалуйста, проверьте интернет-соединение." : "HTTP error on request.Please check your internet connection.");
                    return string.Empty;
                }
                catch (FormatException ex)
                {
                    Logger.LogError($"Data conversion error: {ex}");
                    Logger.DisplayError(language == "ru" ? "Ошибка в формате данных. Пожалуйста, проверьте ответ от сервера." : "Error in the data format. Please check the response from the server.");
                    return string.Empty;
                }
                catch (Exception ex)
                {
                    Logger.LogError($"An unknown error has occurred.: {ex}");
                    Logger.DisplayError(language == "ru" ? "Произошла неизвестная ошибка." : "An unknown error has occurred.");
                    return string.Empty;
                }
            }
        }
        private string UpdateWeather(WeatherData weatherData, JObject forecastData, string language)
        {
            try
            {
                // Выбор HTML-шаблона на основе выбранного языка
                string htmlTemplate = (language == "ru") ? Resources.template_ru : Resources.template_en;

                StringBuilder htmlBuilder = new StringBuilder(htmlTemplate);
                ReplacePlaceholders(weatherData, htmlBuilder, forecastData, language);

                // Возвращаем HTML-разметку в виде строки
                return htmlBuilder.ToString();
            }
            catch (FormatException ex)
            {
                Logger.LogError($"Data conversion error: {ex}");
                Logger.DisplayError(language == "ru" ? "Ошибка в формате данных. Пожалуйста, проверьте ответ от сервера." : "Error in the data format. Please check the response from the server.");
                return string.Empty;
            }
            catch (Exception ex)
            {
                Logger.LogError($"An unknown error has occurred.: {ex}");
                Logger.DisplayError(language == "ru" ? "Произошла неизвестная ошибка." : "An unknown error has occurred.");
                return string.Empty;
            }
        }

        private void ReplacePlaceholders(WeatherData weatherData, StringBuilder htmlBuilder, JObject forecastData, string language)
        {
            try
            {
                // Извлечение значения "sunrise" и "sunset"
                string sunriseStr = forecastData["daily"]["sunrise"][0].ToString();
                string sunsetStr = forecastData["daily"]["sunset"][0].ToString();

                if (language == "ru")
                    htmlBuilder = htmlBuilder.Replace("{city}", $"{weatherData.Locality}, сейчас {DateTime.Now.ToShortTimeString()}");
                else
                    htmlBuilder = htmlBuilder.Replace("{city}", $"{weatherData.Locality}, now {DateTime.Now.ToShortTimeString()}");

                List<WeatherData> weatherDataList = new List<WeatherData>();

                WeatherData.ParseWeatherDataArray(forecastData, weatherDataList);

                // Добавляем обработку текущей погоды
                string currentTime = DateTime.Now.ToString("dd.MM.yyyy HH:00:00");

                DateTime sunrise = DateTime.ParseExact(sunriseStr, "yyyy-MM-ddTHH:mm", null);
                DateTime sunset = DateTime.ParseExact(sunsetStr, "yyyy-MM-ddTHH:mm", null);
                TimeSpan daylightDuration = sunset - sunrise;

                string table_html = string.Empty;
                foreach (var weatherData1 in weatherDataList)
                {
                    // Находим первую запись, время которого равно текущему времени
                    if (weatherData1.Time.ToString() == currentTime)
                    {
                        htmlBuilder = htmlBuilder.Replace("{current_temperature}", $"🌡️ {weatherData1.Temperature}");
                        htmlBuilder = htmlBuilder.Replace("{current_relativehumidity}", $"💧 {weatherData1.Humidity}");
                        htmlBuilder = htmlBuilder.Replace("{current_feels_like}", weatherData1.ApparentTemperature.ToString());
                        htmlBuilder = htmlBuilder.Replace("{current_precipitation_probability}", weatherData1.PrecipitationProbability.ToString());
                        htmlBuilder = htmlBuilder.Replace("{current_surface_pressure}", $"🌡 {(int)(weatherData1.Surface_pressure * 0.750064)}");
                        htmlBuilder = htmlBuilder.Replace("{current_cloudiness}", GetWeatherCondition(weatherData1.CloudCover, language));
                        htmlBuilder = htmlBuilder.Replace("{current_windspeed}", $"💨 {weatherData1.WindSpeed_10m}");
                        htmlBuilder = htmlBuilder.Replace("{current_winddirection}", $"{GetWindDirection(weatherData1.WindDirection_10m, language)}");
                        htmlBuilder = htmlBuilder.Replace("{current_uv_index}", GetUV_index(weatherData1.UV_Index, language));
                        htmlBuilder = htmlBuilder.Replace("{current_isday}", $"{weatherData1.IsDay}");
                        htmlBuilder = htmlBuilder.Replace("{daylight_hours}", Daylight_hours(sunrise, sunset, daylightDuration, language));
                    }
                }

                List<WeatherData> averageWeatherDataList = WeatherData.CalculateSixHourAverages(weatherDataList);

                string[] time_day;
                if (language == "ru")
                {
                    time_day = new string[4] { "Ночь", "Утро", "День", "Вечер" };
                }
                else
                {
                    time_day = new string[4] { "Night", "Morning", "Day", "Evening" };
                }
                for (int i = 0; i < averageWeatherDataList.Count; i++)
                {
                    table_html += "<tr>";
                    table_html += "<td>" + time_day[i] + "</td>";
                    table_html += "<td>" + averageWeatherDataList[i].Temperature + " °C</td>";
                    table_html += "<td>" + averageWeatherDataList[i].Humidity + " %</td>";
                    table_html += "<td>" + averageWeatherDataList[i].ApparentTemperature + " °C</td>";
                    table_html += "<td>" + averageWeatherDataList[i].PrecipitationProbability + " %</td>";
                    table_html += "<td>" + GetWeatherCondition(averageWeatherDataList[i].CloudCover, language) + "</td>";
                    table_html += language == "ru" ? "<td>" + averageWeatherDataList[i].WindSpeed_10m + " м/с</td>" : "<td>" + averageWeatherDataList[i].WindSpeed_10m + " m/s</td>";
                    table_html += "</tr>";
                }
                htmlBuilder.Replace("{weather}", table_html);
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

        private string GetWeatherCondition(double cloudinessPercent, string language)
        {
            try
            {
                string weatherCondition = "";

                if (cloudinessPercent < 20)
                    weatherCondition = language == "ru" ? "☀️ Ясно" : "☀️ Sunny";
                else if (cloudinessPercent >= 20 && cloudinessPercent < 50)
                    weatherCondition = language == "ru" ? "🌤️ Малооблачно" : "🌤️ Low clouds";
                else if (cloudinessPercent >= 50 && cloudinessPercent < 80)
                    weatherCondition = language == "ru" ? "⛅ Облачно с прояснениями" : "⛅ Cloudy with clarifications";
                else
                    weatherCondition = language == "ru" ? "☁️ Пасмурно" : "☁️ Overcast";
                return weatherCondition;
            }
            catch (Exception ex)
            {
                Logger.LogError($"An unknown error has occurred: {ex}");
                Logger.DisplayError(language == "ru" ? "Произошла неизвестная ошибка." : "An unknown error has occurred.");
                return string.Empty;
            }
        }
        private string GetWindDirection(double deg, string language)
        {
            string result = string.Empty;
            try
            {
                string[] directions;
                // Создание массива строк с описаниями направлений ветра.
                if (language == "ru")
                    directions = new string[8] { "Северный ↓", "Северо-Восточный ↙", "Восточный ←", "Юго-Восточный ↖", "Южный ↑", "Юго-Западный ↗", "Западный →", "Северо-Западный ↘" };
                else
                    directions = new string[8] { "North ↓", "North-East ↙", "East ←", "South-east ↖", "South ↑", "South-West ↗", "West →", "North-West ↘" };
                result = directions[(int)(Math.Round(deg / 45) % 8)]; // Текстовое описание направления ветра.
            }
            catch (Exception ex)
            {
                Logger.LogError($"An unknown error has occurred.: {ex}");
                Logger.DisplayError(language == "ru" ? "Произошла неизвестная ошибка." : "An unknown error has occurred.");
            }
            return result;
        }
        private string GetUV_index(double uv_index, string language)
        {
            string result = string.Empty;
            try
            {
                string[] UV_intensity;
                // Создание массива строк с описаниями интенсивности УФ-индекса.
                if (language == "ru")
                    UV_intensity = new string[5] { "Низкий УФ-индекс", "Умеренный УФ-индекс", "Высокий УФ-индекс", "Очень высокий УФ-индекс", "Чрезмерный УФ-индекс" };
                else
                    UV_intensity = new string[5] { "Low UV-index", "Temperate UV-index", "High UV-index", "Very high UV-index", "Excessive UV-index" };

                if (uv_index >= 0 && uv_index <= 2)
                {
                    result = UV_intensity[0];
                }
                else if (uv_index >= 3 && uv_index <= 5)
                {
                    result = UV_intensity[1];
                }
                else if (uv_index >= 6 && uv_index <= 7)
                {
                    result = UV_intensity[2];
                }
                else if (uv_index >= 8 && uv_index <= 10)
                {
                    result = UV_intensity[3];
                }
                else
                {
                    result = UV_intensity[4];
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"An unknown error has occurred.: {ex}");
                Logger.DisplayError(language == "ru" ? "Произошла неизвестная ошибка." : "An unknown error has occurred.");
            }
            return result; // Возвращение текстового описания интенсивности УФ-индекса.   
        }
        private string Daylight_hours(DateTime sunrise, DateTime sunset, TimeSpan daylightDuration, string language)
        {
            string result = string.Empty;
            try
            {
                result = language == "ru" ? "Световой день: " : "Daylight hours: ";
                result += language == "ru" ? $"{daylightDuration.Hours} ч {daylightDuration.Minutes} мин; " : $"{daylightDuration.Hours} h {daylightDuration.Minutes} min; ";
                result += language == "ru" ? $"🌅 Восход: {sunrise.ToShortTimeString()} 🌇 Закат: {sunset.ToShortTimeString()}" : $" 🌅 Sunrise: {sunrise.ToShortTimeString()} 🌇 Sunset: {sunset.ToShortTimeString()}"; //10d8701b31d54b3c9250bfed8aa0924d
            }
            catch (Exception ex)
            {
                Logger.LogError($"An unknown error has occurred.: {ex}");
                Logger.DisplayError(language == "ru" ? "Произошла неизвестная ошибка." : "An unknown error has occurred.");
            }
            return result;
        }
    }
}
