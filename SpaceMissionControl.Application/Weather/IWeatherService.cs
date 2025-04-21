using SpaceMissionControl.Model.Weather;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceMissionControl.Application.Weather
{
	public interface IWeatherService
	{
		Task<WeatherCondition> GetCurrentWeatherAsync(string launchSite);
		bool IsSafeToLaunch(WeatherCondition condition);
	}
}
