using SpaceMissionControl.Model.Spacecraft;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceMissionControl.Application.Navigation
{
	public class NavigationSystem : INavigationSystem
	{
		#region Private Fields

		private readonly Dictionary<string, double> _destinationDistances;
		private readonly Dictionary<SpacecraftType, double> _fuelEfficiency;

		#endregion

		#region Constructor

		public NavigationSystem()
		{
			// Initialize known destinations and their distances (in million km)
			_destinationDistances = new Dictionary<string, double>
			{
				{ "Earth Orbit", 0.4 },
				{ "Moon", 0.384 },
				{ "Mars", 54.6 },
				{ "Venus", 41.4 },
				{ "Mercury", 77.3 },
				{ "Jupiter", 588.0 },
				{ "Saturn", 1200.0 },
				{ "Uranus", 2600.0 },
				{ "Neptune", 4300.0 },
				{ "Pluto", 5900.0 },
				{ "Alpha Centauri", 41300000.0 }
			};

			// Fuel efficiency for each spacecraft type (distance in million km per fuel unit)
			_fuelEfficiency = new Dictionary<SpacecraftType, double>
			{
				{ SpacecraftType.Satellite, 1.5 },
				{ SpacecraftType.Rover, 0.8 },
				{ SpacecraftType.Shuttle, 2.0 },
				{ SpacecraftType.SpaceStation, 0.5 }
			};
		}

		#endregion

		public double CalculateRequiredFuel(string destination, SpacecraftType spacecraftType)
		{
			// Check if destination exists
			if (!_destinationDistances.TryGetValue(destination, out var distance))
				throw new ArgumentException($"Unknown destination: {destination}");

			// Check if spacecraft type exists
			if (!_fuelEfficiency.TryGetValue(spacecraftType, out var efficiency))
				throw new ArgumentException($"Unknown spacecraft type: {spacecraftType}");

			// Calculate required fuel
			double requiredFuel = distance / efficiency;

			// Add 20% safety margin
			requiredFuel *= 1.2;

			// Launch requires 50 fuel units
			requiredFuel += 50;

			return Math.Ceiling(requiredFuel);
		}

		public List<string> GetRoutePlan(string destination)
		{
			// Check if destination exists
			if (!_destinationDistances.ContainsKey(destination))
				throw new ArgumentException($"Unknown destination: {destination}");

			var routePlan = new List<string> { "Launch Pad" };

			// Add intermediate waypoints based on destination
			switch (destination)
			{
				case "Earth Orbit":
					routePlan.Add("Earth Orbit");
					break;

				case "Moon":
					routePlan.Add("Earth Orbit");
					routePlan.Add("Lunar Transfer");
					routePlan.Add("Lunar Orbit");
					break;

				case "Mars":
					routePlan.Add("Earth Orbit");
					routePlan.Add("Lunar Transfer");
					routePlan.Add("Mars Approach");
					routePlan.Add("Mars Orbit");
					break;

				case "Jupiter":
				case "Saturn":
				case "Uranus":
				case "Neptune":
				case "Pluto":
					routePlan.Add("Earth Orbit");
					routePlan.Add("Mars Flyby");
					routePlan.Add("Asteroid Belt Crossing");
					routePlan.Add($"{destination} Approach");
					routePlan.Add($"{destination} Orbit");
					break;

				case "Alpha Centauri":
					routePlan.Add("Earth Orbit");
					routePlan.Add("Solar System Exit");
					routePlan.Add("Interstellar Space");
					routePlan.Add("Alpha Centauri Approach");
					routePlan.Add("Alpha Centauri System");
					break;

				default:
					routePlan.Add("Earth Orbit");
					routePlan.Add($"{destination} Approach");
					routePlan.Add($"{destination} Arrival");
					break;
			}

			return routePlan;
		}

		public bool IsDestinationReachable(double availableFuel, string destination, SpacecraftType spacecraftType)
		{
			try
			{
				double requiredFuel = CalculateRequiredFuel(destination, spacecraftType);
				return availableFuel >= requiredFuel;
			}
			catch (ArgumentException)
			{
				return false;
			}
		}
	}
}
