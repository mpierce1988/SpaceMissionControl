using SpaceMissionControl.Application.Weather;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceMissionControl.Application.Test
{
	[TestClass]
	public class SimulatedWeatherServiceTests
	{
		private IWeatherService _weatherService;

		[TestInitialize]
		public void Setup()
		{
			_weatherService = new SimulatedWeatherService();
		}

		// TODO Create tests for the .IsSafeToLaunch method
		// IsSafeToLaunch takes a WeatherCondition
		// A WeatherCondition is safe to launch if it meets ALL of the following requirements
		// -- WindSpeed is less than 30
		// -- Visibility is greater than 3
		// -- No lightning
		// -- Temperature is greater than or equal to -10, and less than or equal to 35.
		// Create a test for a valid, safe WeatherCondition. Then create tests for unsafe conditions
		// Think about boundaries (for example, WindSpeed is less than 30 - what if WindSpeed IS 30?)
	}
}
