using SpaceMissionControl.Model.Weather;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceMissionControl.Application.Weather
{
	public class SimulatedWeatherService : IWeatherService
	{
		private readonly Random _rng;

		private readonly Dictionary<string, (double minTemp, double maxTemp, double maxWind)> _sitePatterns =
			new Dictionary<string, (double, double, double)>(StringComparer.OrdinalIgnoreCase)
			{
				{ "Cape Canaveral", (15, 32, 25) },
				{ "Baikonur", (5, 30, 20) },
				{ "Kennedy Space Center", (18, 35, 22) },
				{ "Vandenberg", (12, 25, 30) },
				{ "Jiuquan", (10, 28, 18) }
			};

		#region Constructor

		public SimulatedWeatherService()
		{
			_rng = new Random();
		}

		#endregion

		#region Public Methods

		public Task<WeatherCondition> GetCurrentWeatherAsync(string launchSite)
		{
			// Get pattern for site, or use default if not found
			var pattern = _sitePatterns.GetValueOrDefault(launchSite, (10, 30, 25));

			// Generate random but realistic weather based on site pattern
			var condition = new WeatherCondition
			{
				WindSpeed = _rng.NextDouble() * pattern.maxWind,
				Temperature = _rng.NextDouble() * (pattern.maxTemp - pattern.minTemp) + pattern.minTemp,
				Visibility = _rng.NextDouble() * 15, // 0-15 miles of visibility
				Lightning = _rng.NextDouble() < 0.1 // 10% chance of lightning
			};

			return Task.FromResult(condition);
		}

		public bool IsSafeToLaunch(WeatherCondition condition)
		{
			bool isSafe = condition.WindSpeed < 30 &&    // Wind speed less than 30 mph
						 condition.Visibility > 3 &&     // Visibility greater than 3 miles
						 !condition.Lightning &&         // No lightning
						 condition.Temperature is >= -10 and <= 35; // Temperature between -10°C and 35°C

			return isSafe;
		}

		#endregion
	}
}
