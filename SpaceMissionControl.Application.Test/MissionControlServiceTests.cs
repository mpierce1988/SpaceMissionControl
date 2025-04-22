using Moq;
using SpaceMissionControl.Application.Mission;
using SpaceMissionControl.Application.Navigation;
using SpaceMissionControl.Application.Telemetry;
using SpaceMissionControl.Application.Weather;
using SpaceMissionControl.Model.Mission;
using SpaceMissionControl.Model.Spacecraft;
using SpaceMissionControl.Model.Weather;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceMissionControl.Application.Test
{
	[TestClass]
	public class MissionControlServiceTests
	{
		private Mock<IWeatherService> _mockWeatherService;
		private Mock<ITelemetryService> _mockTelemetryService;
		private Mock<INavigationSystem> _mockNavigationSystem;
		private MissionControlService _missionControlService;

		[TestInitialize]
		public void Setup()
		{
			// Setup mocks
			_mockWeatherService = new Mock<IWeatherService>();
			_mockTelemetryService = new Mock<ITelemetryService>();
			_mockNavigationSystem = new Mock<INavigationSystem>();

			// Create the system/service under test using the mocked dependencies
			_missionControlService = new MissionControlService(
				_mockWeatherService.Object,
				_mockTelemetryService.Object,
				_mockNavigationSystem.Object
				);
		}
		#region Register Spacecraft

		[TestMethod]
		public void RegisterSpacecraft_AddsSpacecraftToRegistry()
		{
			// Arrange
			string spacecraftId = "SC-001";
			var spacecraft = new Spacecraft(spacecraftId, "Voyager", SpacecraftType.Satellite, 100);

			// Act
			_missionControlService.RegisterSpacecraft(spacecraft);
			var retrievedSpacecraft = _missionControlService.GetSpacecraft(spacecraftId);

			// Assert
			Assert.AreEqual(spacecraft, retrievedSpacecraft);
		}

		[TestMethod]
		public void RegisterSpacecraft_WithDuplicateId_ThrowsInvalidOperationException()
		{
			// Arrange
			string spacecraftId = "SC-001";
			var spacecraft1 = new Spacecraft(spacecraftId, "Voyager", SpacecraftType.Satellite, 100);
			var spacecraft2 = new Spacecraft(spacecraftId, "Explorer", SpacecraftType.Rover, 150);
			_missionControlService.RegisterSpacecraft(spacecraft1); // Register the first spacecraft

			// Act
			Action act = () => _missionControlService.RegisterSpacecraft(spacecraft2);

			// Assert
			Assert.ThrowsException<InvalidOperationException>(act);
			
		}

		#endregion

		#region GetSpacecraft Tests

		[TestMethod]
		public void GetSpacecraft_WithNonExistentId_ThrowsKeyNotFoundException()
		{
			// Arrange
			string nonExistentId = "nonexistent-id";

			// Act
			Action act = () => _missionControlService.GetSpacecraft(nonExistentId);

			// Assert
			Assert.ThrowsException<KeyNotFoundException>(act);
		}

		#endregion

		#region LaunchSpacecraft Tests

		[TestMethod]
		public async Task LaunchSpacecraft_WithSafeConditions_ReturnsSuccessResult()
		{
			// Arrange
			var spacecraft = new Spacecraft("SC-001", "Voyager", SpacecraftType.Satellite, 100);
			_missionControlService.RegisterSpacecraft(spacecraft);

			string launchSite = "Cape Canaveral";
			MissionParameters parameters = new()
			{
				Destination = "Mars",
				Distance = 54600000,
				CrewSize = 0,
				LaunchDate = DateTime.Now
			};

			// Setup weather service mock
			WeatherCondition goodWeather = new()
			{
				WindSpeed = 5,
				Temperature = 22,
				Visibility = 10,
				Lightning = false
			};

			_mockWeatherService
				.Setup(ws => ws.GetCurrentWeatherAsync(launchSite))
				.ReturnsAsync(goodWeather);

			_mockWeatherService
				.Setup(ws => ws.IsSafeToLaunch(goodWeather))
				.Returns(true);

			// Setup navigation system mock
			_mockNavigationSystem
				.Setup(ns => ns.CalculateRequiredFuel(parameters.Destination, spacecraft.Type))
				.Returns(40); // Less than spacecraft's fuel

			// Set up telemetry service mock
			_mockTelemetryService
				.Setup(ts => ts.SendTelemetryAsync(
					It.IsAny<string>(),
					It.IsAny<Dictionary<string, string>>()
					))
				.ReturnsAsync(true);

			// Act
			var result = await _missionControlService.LaunchSpacecraftAsync(spacecraft.Id, launchSite, parameters);

			// Assert
			Assert.IsTrue(result.Success);
			Assert.AreEqual("Launch successful", result.Message);
			Assert.IsTrue(spacecraft.IsLaunched);

			// Bonus: Verify telemetry data was sent
			_mockTelemetryService.Verify(
				t => t.SendTelemetryAsync(
					spacecraft.Id,
					It.Is<Dictionary<string, string>>(d => 
						d.ContainsKey("event") && d["event"] == "launch" &&
						d.ContainsKey("destination") && d["destination"] == parameters.Destination
						)
					),
				Times.Once
				);
		}

		[TestMethod]
		public async Task LaunchSpacecraftAsync_WithBadWeather_ReturnsFailureResult()
		{
			// Arrange
			var spacecraft = new Spacecraft("SC-001", "Voyager", SpacecraftType.Satellite, 100);
			_missionControlService.RegisterSpacecraft(spacecraft);

			string launchSite = "Cape Canaveral";
			var parameters = new MissionParameters()
			{
				Destination = "Mars",
				Distance = 54600000,
				CrewSize = 0,
				LaunchDate = DateTime.Now
			};

			// Setup weather service mock for unsafe weather conditions
			var badWeather = new WeatherCondition()
			{
				WindSpeed = 35, // High wind speed
				Temperature = 22,
				Visibility = 2, // Low visibility
				Lightning = true // Lightning present
			};

			_mockWeatherService
				.Setup(w => w.GetCurrentWeatherAsync(launchSite))
				.ReturnsAsync(badWeather);

			_mockWeatherService
				.Setup(w => w.IsSafeToLaunch(badWeather))
				.Returns(false);

			// Act
			var result = await _missionControlService.LaunchSpacecraftAsync(
				spacecraft.Id, launchSite, parameters);

			// Assert
			Assert.IsFalse(result.Success);
			Assert.AreEqual("Unsafe weather conditions for launch", result.Message);
			Assert.IsFalse(spacecraft.IsLaunched);

			// Verify telemetry was NOT sent
			_mockTelemetryService.Verify(
				t => t.SendTelemetryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()),
				Times.Never
				);
		}

		[TestMethod]
		public async Task LaunchSpacecraftAsync_WithInsufficientFuel_ReturnsFailureResult()
		{
			// Arrange
			var spacecraft = new Spacecraft("SC-001", "Voyager", SpacecraftType.Satellite, 100);
			_missionControlService.RegisterSpacecraft(spacecraft);

			string launchSite = "Cape Canaveral";
			var parameters = new MissionParameters()
			{
				Destination = "Jupiter",
				Distance = 588000000,
				CrewSize = 0,
				LaunchDate = DateTime.Now
			};

			// Arrange spacecraft and register the craft. Create launch site and MissionParameters

			// Setup weather service mock
			var goodWeather = new WeatherCondition()
			{
				WindSpeed = 5,
				Temperature = 22,
				Visibility = 10,
				Lightning = false
			};			

			/* Setup the mock call to .GetCurrentWeatherAsync(launchSite), and return goodWeather */

			/* Setup the mock call to .IsSafeToLaunch(goodWeather), and return true */

			// Mock navigation system 
			/* Setup call to .CalculatedRequiredFuel(parameters.Destination, spacecraft.Type), and return MORE fuel than the spacecraft has */

			// Act
			var result = await _missionControlService.LaunchSpacecraftAsync(
				spacecraft.Id, launchSite, parameters);

			// Assert
			Assert.IsFalse(result.Success);
			Assert.IsTrue(result.Message.Contains("Insufficient fuel"));
			Assert.IsFalse(spacecraft.IsLaunched);
		}

		#endregion

		#region PlanMission Tests

		[TestMethod]
		public void PlanMission_WithReachableDestination_ReturnsRoutePlan()
		{
			// Arrange
			var spacecraft = new Spacecraft("SC-001", "Voyager", SpacecraftType.Satellite, 100);
			_missionControlService.RegisterSpacecraft(spacecraft);

			string destination = "Mars";
			var expectedRoute = new List<string> { "Earth Orbit", "Lunar Transfer", "Mars Approach", "Mars Orbit" };

			/* Setup required calls to mock NavigationSystem */
			

			// Act
			var routePlan = _missionControlService.PlanMission(spacecraft.Id, destination);

			// Assert
			Assert.AreEqual(expectedRoute, routePlan);

			// Verify navigation system was called with the correct parameters
			_mockNavigationSystem.Verify(
				n => n.IsDestinationReachable(spacecraft.FuelLevel, destination, spacecraft.Type),
				Times.Once
				);

			_mockNavigationSystem.Verify(
				n => n.GetRoutePlan(destination),
				Times.Once
				);
		}

		[TestMethod]
		public void PlanMission_WithUnreachableDestination_ThrowsInvalidOperationException()
		{
			// Arrange
			var spacecraft = new Spacecraft("SC-001", "Voyager", SpacecraftType.Satellite, 30);
			_missionControlService.RegisterSpacecraft(spacecraft);

			string destination = "Alpha Centauri";

			// Act
			/* Implement */

			// Assert
			/* Implement */
		}
		#endregion
	}
}
