using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceMissionControl.Model.Weather
{
	public class WeatherCondition
	{
		public double WindSpeed { get; set; }
		public double Temperature { get; set; }
		public double Visibility { get; set; }
		public bool Lightning { get; set; }
	}
}
