using SpaceMissionControl.Model.Spacecraft;

namespace SpaceMissionControl.Application.Test
{
	#region Constructor Tests

	/// <summary>
	/// This is a basic example of Unit Testing without dependencies.
	/// </summary>
	[TestClass]
	public sealed class SpacecraftTests
	{
		[TestMethod]
		public void Constructor_SetsInitialValues_ReturnsCorrectValues()
		{
			// Arrange
			string id = "SC-001";
			string name = "Voyager";
			SpacecraftType type = SpacecraftType.Satellite;
			double fuelLevel = 100;

			// Act
			Spacecraft spacecraft = new Spacecraft(id, name, type, fuelLevel);

			// Assert
			Assert.AreEqual(id, spacecraft.Id);
			Assert.AreEqual(name, spacecraft.Name);
			Assert.AreEqual(type, spacecraft.Type);
			Assert.AreEqual(fuelLevel, spacecraft.FuelLevel);
			Assert.IsFalse(spacecraft.IsLaunched);
			Assert.AreEqual(LaunchStatus.NotLaunched, spacecraft.LaunchStatus);
		}

		#endregion

		#region Launch Tests

		[TestMethod]
		public void Launch_WithSufficientFuel_ReturnsTrue()
		{
			// Arrange
			int initialFuel = 100;
			int fuelToLaunch = 50;
			var spacecraft = new Spacecraft("SC-001", "Voyager", SpacecraftType.Satellite, initialFuel);

			// Act
			bool result = spacecraft.Launch();

			// Assert
			Assert.IsTrue(result);
			Assert.IsTrue(spacecraft.IsLaunched);
			Assert.AreEqual(initialFuel - fuelToLaunch, spacecraft.FuelLevel);
			Assert.AreEqual(LaunchStatus.Launched, spacecraft.LaunchStatus);
		}

		[TestMethod]
		public void Launch_WithInsufficientFuel_ReturnsFalse()
		{
			// Arrange
			int initialFuel = 40;
			var spacecraft = new Spacecraft("SC-001", "Voyager", SpacecraftType.Satellite, initialFuel);

			// Act
			bool result = spacecraft.Launch();

			// Assert
			// TODO Assert result is false
			// Assert spacecraft has NOT launched
			// Assert current spacecraft FuelLevel and InitialFuel are equal (since it did not launch)
			// Assert current spacecraft LaunchStatus is NotLaunched			
		}

		[TestMethod]
		public void Launch_WhenAlreadyLaunched_ReturnsFalse()
		{
			// Arrange
			var spacecraft = new Spacecraft("SC-001", "Voyager", SpacecraftType.Satellite, 100);
			spacecraft.Launch(); // first launch
			double fuelAfterFirstLaunch = spacecraft.FuelLevel;

			// Act
			bool result = spacecraft.Launch();

			// Assert
			// TODO Assert result is False
			// Assert current spacecraft FuelLevel matches FuelAfterFirstLaunch
			// Assert spacecraft LaunchStatus is Launched
		}

		#endregion

		#region ConsumeFuel Tests

		[DataTestMethod]
		[DataRow(10)]
		[DataRow(25)]
		[DataRow(0)]
		public void ConsumeFuel_WithValidAmount_ReducesFuelLevel(double amount)
		{
			// Arrange
			double initialFuel = 100;
			var spacecraft = new Spacecraft("SC-001", "Voyager", SpacecraftType.Satellite, initialFuel);
			double expectedFuel = initialFuel - amount;

			// Act
			spacecraft.ConsumeFuel(amount);

			// Assert
			Assert.AreEqual(expectedFuel, spacecraft.FuelLevel);
		}

		[TestMethod]
		public void ConsumeFuel_WithNegativeAmount_ThrowsArgumentException()
		{
			// Arrange
			var spacecraft = new Spacecraft("SC-001", "Voyager", SpacecraftType.Satellite, 100);
			double fuelToConsume = -10;
			
			// Act
			Action act = () => spacecraft.ConsumeFuel(fuelToConsume);

			// Assert
			Assert.ThrowsException<ArgumentException>(act);
		}

		[TestMethod]
		public void ConsumeFuel_WhenLaunched_UpdatesStatusToOutOfFuelWhenEmpty()
		{
			// Arrange
			var spacecraft = new Spacecraft("SC-001", "Voyager", SpacecraftType.Satellite, 100);
			spacecraft.Launch(); // Now launched with 50 remaining fuel

			// Act
			spacecraft.ConsumeFuel(50);

			// Assert
			Assert.AreEqual(0, spacecraft.FuelLevel);
			Assert.AreEqual(LaunchStatus.OutOfFuel, spacecraft.LaunchStatus);
		}

		#endregion

		#region Refuel Tests

		// TODO Refuel_WithValidAmounts_IncreasesFuelLevel() - use DataTestMethod

		// TODO Refuel_WithNegativeAmount_ThrowsArgumentException()

		// TODO Refuel_WhenLaunched_UpdatesStatusToLaunched()

		#endregion
	}
}
