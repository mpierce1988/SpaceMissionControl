namespace SpaceMissionControl.Model.Spacecraft
{
	public class Spacecraft
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public SpacecraftType Type { get; set; }
		public double FuelLevel { get; set; }
		public bool IsLaunched { get; set; }
		public LaunchStatus LaunchStatus { get; set; }

		public Spacecraft(string id, string name, SpacecraftType type, double fuelLevel)
		{
			Id = id;
			Name = name;
			Type = type;
			FuelLevel = fuelLevel;
			IsLaunched = false;
		}

		public bool Launch()
		{
			if (IsLaunched) return false;

			if (FuelLevel < 50) return false;

			IsLaunched = true;
			FuelLevel -= 50;
			LaunchStatus = LaunchStatus.Launched;
			return true;
		}

		public void ConsumeFuel(double amount)
		{
			if (amount < 0)
				throw new ArgumentException("Fuel consumption cannot be negative");

			if (FuelLevel >= amount)
				FuelLevel -= amount;
			else
				FuelLevel = 0;

			if(FuelLevel == 0 && IsLaunched)
			{
				LaunchStatus = LaunchStatus.OutOfFuel;
			}
		}

		public void Refuel(double amount)
		{
			if (amount < 0) throw new ArgumentException("Refuel amount cannot be negative");

			FuelLevel += amount;

			if(IsLaunched && LaunchStatus == LaunchStatus.OutOfFuel)
			{
				LaunchStatus = LaunchStatus.Launched;
			}
		}
	}
}
