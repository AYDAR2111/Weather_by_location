using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Weather_by_location
{
    public class WeatherData
    {
        public string Locality { get; set; } // Название населенного пункта
        public double Latitude { get; set; } // Широта
        public double Longitude { get; set; } // Долгота
        public DateTime Time { get; set; } // Время
        public double Temperature { get; set; } // Температура
        public double Humidity { get; set; }
        public double ApparentTemperature { get; set; } // Ощущается как температура
        public double PrecipitationProbability { get; set; } // Вероятность выпадения осадков surface_pressure
        public double Surface_pressure { get; set; } // Поверхностное давление
        public double CloudCover { get; set; } // Облачность
        public double WindSpeed_10m { get; set; }// Скорость ветра на высоте 10 метров
        public int WindDirection_10m { get; set; }
        public int UV_Index { get; set; }
        public bool IsDay { get; set; }

        public static void ParseWeatherDataArray(JObject forecastData, List<WeatherData> weatherDataList)
        {
            try
            {
                //JObject jsonObject = JObject.Parse(json);
                JArray timeArray = ParseJsonArray<string>(forecastData, "hourly.time");
                JArray temperatureArray = ParseJsonArray<double>(forecastData, "hourly.temperature_2m");
                JArray relativeHumidityArray = ParseJsonArray<int>(forecastData, "hourly.relativehumidity_2m");
                JArray apparentTemperatureArray = ParseJsonArray<double>(forecastData, "hourly.apparent_temperature");
                JArray precipitationProbabilityArray = ParseJsonArray<int>(forecastData, "hourly.precipitation_probability");
                JArray surface_pressureArray = ParseJsonArray<int>(forecastData, "hourly.surface_pressure");
                JArray cloudCoverArray = ParseJsonArray<int>(forecastData, "hourly.cloudcover");
                JArray windSpeedArray = ParseJsonArray<double>(forecastData, "hourly.windspeed_10m");
                JArray windDirectionArray = ParseJsonArray<int>(forecastData, "hourly.winddirection_10m");
                JArray uvIndexArray = ParseJsonArray<double>(forecastData, "hourly.uv_index");
                JArray isDayArray = ParseJsonArray<bool>(forecastData, "hourly.is_day");

                for (int i = 0; i < timeArray.Count; i++)
                {
                    WeatherData weatherData = new WeatherData
                    {
                        Time = DateTime.Parse(timeArray[i].ToString().Replace("T", " ").Replace(".", "-")),
                        Temperature = Math.Round((double)temperatureArray[i], 1),
                        Humidity = (int)relativeHumidityArray[i],
                        ApparentTemperature = Math.Round((double)apparentTemperatureArray[i], 1),
                        PrecipitationProbability = (int)precipitationProbabilityArray[i],
                        Surface_pressure = Math.Round((double)surface_pressureArray[i], 1),
                        CloudCover = (int)cloudCoverArray[i],
                        WindSpeed_10m = Math.Round((double)windSpeedArray[i], 1),
                        WindDirection_10m = (int)windDirectionArray[i],
                        UV_Index = (int)uvIndexArray[i],
                        IsDay = (bool)isDayArray[i]
                    };
                    weatherDataList.Add(weatherData);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"An unknown error has occurred: {ex}");
            }
        }

        public static List<WeatherData> CalculateSixHourAverages(List<WeatherData> weatherDataList)
        {
            List<WeatherData> averageWeatherDataList = new List<WeatherData>();
            try
            {

                int chunkSize = 6; // Количество часов в каждом среднем интервале
                int startIndex = 0;

                while (startIndex < weatherDataList.Count)
                {
                    int endIndex = Math.Min(startIndex + chunkSize, weatherDataList.Count);

                    // Вычисляем средние значения за указанный интервал 
                    WeatherData averageData = CalculateAverage(weatherDataList.GetRange(startIndex, chunkSize));
                    averageWeatherDataList.Add(averageData);
                    startIndex = endIndex;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"An unknown error has occurred: {ex}");
            }
            return averageWeatherDataList;
        }

        private static WeatherData CalculateAverage(List<WeatherData> weatherDataList)
        {
            if (weatherDataList.Count == 0)
                return new WeatherData();

            double totalTemperature = 0;
            double totalHumidity = 0;
            double totalApparentTemperature = 0;
            double totalPrecipitationProbability = 0;
            double totalCloudCover = 0;
            double totalWindSpeed = 0;

            foreach (var weatherData in weatherDataList)
            {
                totalTemperature += weatherData.Temperature;
                totalHumidity += weatherData.Humidity;
                totalApparentTemperature += weatherData.ApparentTemperature;
                totalPrecipitationProbability += weatherData.PrecipitationProbability;
                totalCloudCover += weatherData.CloudCover;
                totalWindSpeed += weatherData.WindSpeed_10m;
            }

            double averageTemperature = totalTemperature / weatherDataList.Count;
            double averageHumidity = totalHumidity / weatherDataList.Count;
            double averageApparentTemperature = totalApparentTemperature / weatherDataList.Count;
            double averagePrecipitationProbability = totalPrecipitationProbability / weatherDataList.Count;
            double averageCloudCover = totalCloudCover / weatherDataList.Count;
            double averageWindSpeed = totalWindSpeed / weatherDataList.Count;

            return new WeatherData
            {
                Temperature = Math.Round(averageTemperature, 1),
                Humidity = Math.Round(averageHumidity, 1),
                ApparentTemperature = Math.Round(averageApparentTemperature, 1),
                PrecipitationProbability = Math.Round(averagePrecipitationProbability, 1),
                CloudCover = Math.Round(averageCloudCover, 1),
                WindSpeed_10m = Math.Round(averageWindSpeed, 1)
            };
        }

        private static JArray ParseJsonArray<T>(JObject jsonObject, string arrayPath)
        {
            JArray jsonArray = jsonObject.SelectToken(arrayPath) as JArray;
            return jsonArray ?? new JArray();
        }
    }
}
