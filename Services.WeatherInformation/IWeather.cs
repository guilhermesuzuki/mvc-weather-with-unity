using System;

namespace Services.WeatherInformation
{
    public interface IWeather
    {
        string WeatherCode { get; }
        int MaximumTemperature { get; set; }
        int MaximumTemperatureInFahrenheit { get; }
        int MinimumTemperature { get; set; }
        int MinimumTemperatureInFahrenheit { get; }
        int Temperature { get; set; }
        int TemperatureInFahrenheit { get; }
        WeatherModel.eWeatherConditions WeatherCondition { get; set; }
        string WeatherDescription { get; }
        bool IsNight { get; }
    }
}