using SpaceMissionControl.Application.Navigation;
using SpaceMissionControl.Application.Telemetry;
using SpaceMissionControl.Application.Weather;
using SpaceMissionControl.Model.Mission;
using SpaceMissionControl.Model.Spacecraft;

namespace SpaceMissionControl.Application.Mission
{
	public class MissionControlService
	{
		private readonly IWeatherService _weatherService;
		private readonly ITelemetryService _telemetryService;
		private readonly INavigationSystem _navigationSystem;
		private readonly Dictionary<string, Spacecraft> _activeSpacecraft;

		public MissionControlService(IWeatherService weatherService, ITelemetryService telemetryService, INavigationSystem navigationSystem)
		{
			_weatherService = weatherService;
			_telemetryService = telemetryService;
			_navigationSystem = navigationSystem;
			_activeSpacecraft = new Dictionary<string, Spacecraft>();
		}

		public void RegisterSpacecraft(Spacecraft spacecraft)
		{
			if (_activeSpacecraft.ContainsKey(spacecraft.Id))
				throw new InvalidOperationException($"Spacecraft with ID {spacecraft.Id} is already registered.");

			_activeSpacecraft.Add(spacecraft.Id, spacecraft);
		}

		public Spacecraft GetSpacecraft(string id)
		{
			if (!_activeSpacecraft.ContainsKey(id))
				throw new KeyNotFoundException($"Spacecraft with ID {id} not found");

			return _activeSpacecraft[id];
		}

		public async Task<LaunchResult> LaunchSpacecraftAsync(string spacecraftId, string launchSite, MissionParameters parameters)
		{
			// Check if spacecraft exists
			if (!_activeSpacecraft.TryGetValue(spacecraftId, out var spacecraft))
				return new LaunchResult(false, "Spacecraft not found");

			// Check if already launched
			if (spacecraft.IsLaunched)
				return new LaunchResult(false, "Spacecraft already launched");

			// Check weather conditions
			var weatherCondition = await _weatherService.GetCurrentWeatherAsync(launchSite);
			if (!_weatherService.IsSafeToLaunch(weatherCondition))
				return new LaunchResult(false, "Unsafe weather conditions for launch");

			// Check fuel requirements
			double requiredFuel = _navigationSystem.CalculateRequiredFuel(parameters.Destination, spacecraft.Type);
			if (spacecraft.FuelLevel < requiredFuel)
				return new LaunchResult(false, $"Insufficient fuel. Required: {requiredFuel}, Available: {spacecraft.FuelLevel}");

			// Launch spacecraft
			if(!spacecraft.Launch())
			{
				return new LaunchResult(false, "Failed to launch spacecraft");
			}

			// Send telemetry data
			await _telemetryService.SendTelemetryAsync(spacecraftId, new Dictionary<string, string>
			{
				{"event", "launch" },
				{"destination", parameters.Destination },
				{"fuel_level", spacecraft.FuelLevel.ToString() }
			});

			// Return success
			return new LaunchResult(true, "Launch successful");
		}

		public List<string> PlanMission(string spacecraftId, string destination)
		{
			if (!_activeSpacecraft.TryGetValue(spacecraftId, out var spacecraft))
				throw new KeyNotFoundException($"Spacecraft with ID {spacecraftId} not found");

			bool isReachable = _navigationSystem.IsDestinationReachable(spacecraft.FuelLevel, destination, spacecraft.Type);

			if (!isReachable)
				throw new InvalidOperationException($"Destination {destination} is not reachable with current fuel level.");

			return _navigationSystem.GetRoutePlan(destination);
		}
	}
}
