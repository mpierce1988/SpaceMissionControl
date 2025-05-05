using SpaceMissionControl.Application.Mission;
using SpaceMissionControl.Application.Navigation;
using SpaceMissionControl.Application.Weather;
using SpaceMissionControl.Model.Mission;
using SpaceMissionControl.Model.Spacecraft;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceMissionControl.Presentation
{
	public class MissionControlUI
	{
		#region Private Fields

		private readonly MissionControlService _missionControl;
		private readonly INavigationSystem _navigationSystem;
		private readonly IWeatherService _weatherService;
		private bool _isRunning = true;

		private readonly string[] _launchSites =
		{
			"Cape Canaveral",
			"Kennedy Space Center",
			"Baikonur",
			"Vandenberg",
			"Jiuquan"
		};

		private readonly string[] _destinations =
		{
			"Earth Orbit",
			"Moon",
			"Mars",
			"Venus",
			"Jupiter",
			"Saturn",
			"Alpha Centauri"
		};

		#endregion

		#region Constructor

		public MissionControlUI(MissionControlService missionControl, INavigationSystem navigationSystem, IWeatherService weatherService)
		{
			_missionControl = missionControl;
			_navigationSystem = navigationSystem;
			_weatherService = weatherService;
		}

		#endregion

		#region Public Methods

		public async Task RunAsync()
		{
			DisplayWelcomeScreen();

			while (_isRunning)
			{
				DisplayMainMenu();
				int choice = GetMenuChoice(6);

				switch (choice)
				{
					case 1:
						DisplayAllSpacecraft();
						break;
					case 2:
						await LaunchSpacecraftAsync();
						break;
					case 3:
						PlanMission();
						break;
					case 4:
						RefuelSpacecraft();
						break;
					case 5:
						DisplayWeatherReport();
						break;
					case 6:
						_isRunning = false;
						break;
				}

				if (_isRunning)
				{
					Console.WriteLine("\nPress any key to continue");
					Console.ReadKey(true);
				}
			}

			Console.Clear();
			Console.WriteLine("Thank you for using Space Mission Control!");
			Thread.Sleep(1500);
		}

		#endregion

		#region Private Methods

		private void DisplayWelcomeScreen()
		{
			Console.Clear();

			// ASCII art logo
			var logo = @"
 ________  ________  ________  ________  _______                            
|\   ____\|\   __  \|\   __  \|\   ____\|\  ___ \                           
\ \  \___|\ \  \|\  \ \  \|\  \ \  \___|\ \   __/|                          
 \ \_____  \ \   ____\ \   __  \ \  \    \ \  \_|/__                        
  \|____|\  \ \  \___|\ \  \ \  \ \  \____\ \  \_|\ \                       
    ____\_\  \ \__\    \ \__\ \__\ \_______\ \_______\                      
   |\_________\|__|     \|__|\|__|\|_______|\|_______|                      
   \|_________|                                                             
                                                                            
                                                                            
 _____ ______   ___  ________   ________  ___  ________  ________           
|\   _ \  _   \|\  \|\   ____\ |\   ____\|\  \|\   __  \|\   ___  \         
\ \  \\\__\ \  \ \  \ \  \___|_\ \  \___|\ \  \ \  \|\  \ \  \\ \  \        
 \ \  \\|__| \  \ \  \ \_____  \\ \_____  \ \  \ \  \\\  \ \  \\ \  \       
  \ \  \    \ \  \ \  \|____|\  \\|____|\  \ \  \ \  \\\  \ \  \\ \  \      
   \ \__\    \ \__\ \__\____\_\  \ ____\_\  \ \__\ \_______\ \__\\ \__\     
    \|__|     \|__|\|__|\_________\\_________\|__|\|_______|\|__| \|__|     
                       \|_________\|_________|                              
                                                                            
                                                                            
 ________  ________  ________   _________  ________  ________  ___          
|\   ____\|\   __  \|\   ___  \|\___   ___\\   __  \|\   __  \|\  \         
\ \  \___|\ \  \|\  \ \  \\ \  \|___ \  \_\ \  \|\  \ \  \|\  \ \  \        
 \ \  \    \ \  \\\  \ \  \\ \  \   \ \  \ \ \   _  _\ \  \\\  \ \  \       
  \ \  \____\ \  \\\  \ \  \\ \  \   \ \  \ \ \  \\  \\ \  \\\  \ \  \____  
   \ \_______\ \_______\ \__\\ \__\   \ \__\ \ \__\\ _\\ \_______\ \_______\
    \|_______|\|_______|\|__| \|__|    \|__|  \|__|\|__|\|_______|\|_______|
";

			Console.ForegroundColor = ConsoleColor.Blue;
			Console.WriteLine(logo);
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("                           WELCOME TO MISSION CONTROL                           ");
			Console.ResetColor();

			Thread.Sleep(1500);
		}

		private void DisplayMainMenu()
		{
			Console.Clear();
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("=== SPACE MISSION CONTROL - MAIN MENU ===");
			Console.ResetColor();
			Console.WriteLine();
			Console.WriteLine("1. Display All Spacecraft");
			Console.WriteLine("2. Launch Spacecraft");
			Console.WriteLine("3. Plan Mission");
			Console.WriteLine("4. Refuel Spacecraft");
			Console.WriteLine("5. Check Weather Conditions");
			Console.WriteLine("6. Exit");
			Console.WriteLine();
			Console.Write("Enter your choice (1-6): ");
		}

		private void DisplayAllSpacecraft()
		{
			Console.Clear();
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("=== SPACECRAFT INVENTORY ===");
			Console.ResetColor();
			Console.WriteLine();

			// try to get all the spacecraft
			try
			{
				// Get list of IDs by using reflection (just for demo)
				var type = _missionControl.GetType();
				var field = type.GetField("_activeSpacecraft", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

				var activeSpacecraft = (Dictionary<string, Spacecraft>)field.GetValue(_missionControl);

				if (activeSpacecraft is null || activeSpacecraft.Count == 0)
				{
					Console.WriteLine("No spacecraft registered.");
					return;
				}

				// Table header
				Console.WriteLine($"{"ID",-10} {"Name",-20} {"Type",-15} {"Fuel Level",-15} {"Status",-15}");
				Console.WriteLine(new string('-', 75));

				// List each spacecraft
				foreach (var spacecraft in activeSpacecraft.Values)
				{
					Console.Write($"{spacecraft.Id,-10} ");
					Console.Write($"{spacecraft.Name,-20} ");
					Console.Write($"{spacecraft.Type,-15} ");

					// Colorize fuel level based on amount
					if (spacecraft.FuelLevel < 30)
					{
						Console.ForegroundColor = ConsoleColor.Red;
					}
					else if (spacecraft.FuelLevel < 70)
					{
						Console.ForegroundColor = ConsoleColor.Yellow;
					}
					else
					{
						Console.ForegroundColor = ConsoleColor.Green;
					}

					Console.Write($"{spacecraft.FuelLevel,-15:F1} ");
					Console.ResetColor();

					// Status with color
					switch (spacecraft.LaunchStatus)
					{
						case LaunchStatus.NotLaunched:
							Console.Write($"{spacecraft.LaunchStatus,-15}");
							break;
						case LaunchStatus.Launched:
							Console.ForegroundColor = ConsoleColor.Green;
							Console.Write($"{spacecraft.LaunchStatus,-15}");
							Console.ResetColor();
							break;
						case LaunchStatus.OutOfFuel:
							Console.ForegroundColor = ConsoleColor.Red;
							Console.Write($"{spacecraft.LaunchStatus,-15}");
							Console.ResetColor();
							break;
						case LaunchStatus.MissionComplete:
							Console.ForegroundColor = ConsoleColor.Blue;
							Console.Write($"{spacecraft.LaunchStatus,-15}");
							Console.ResetColor();
							break;
					}

					Console.WriteLine();
				}
			}
			catch (Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"Error retrieving spacecraft: {ex.Message}");
				Console.ResetColor();
			}
		}

		private async Task LaunchSpacecraftAsync()
		{
			Console.Clear();
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("=== LAUNCH SPACECRAFT ===");
			Console.ResetColor();
			Console.WriteLine();

			try
			{

				Spacecraft? spacecraft = GetSpacecraftChoice();

				if (spacecraft is null) return;

				if (spacecraft.IsLaunched)
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine($"Error: {spacecraft.Name} is already launched!");
					Console.ResetColor();
					return;
				}


				string launchSite = GetLaunchSiteChoice();
				
				string destination = GetDestinationChoice();

				MissionParameters missionParams = new()
				{
					Destination = destination,
					LaunchDate = DateTime.Now,
					CrewSize = spacecraft.Type == SpacecraftType.Shuttle ? 4 : 0
				};

				DisplayLaunchCountdown(spacecraft.Name, launchSite, destination);

				var result = await _missionControl.LaunchSpacecraftAsync(spacecraft.Id, launchSite, missionParams);

				if (result.Success)
				{
					// Success animation
					DisplayLaunchAnimation();

					Console.ForegroundColor = ConsoleColor.Green;
					Console.WriteLine($"\nLaunch successful! {spacecraft.Name} is now en route to {destination}");
					Console.ResetColor();

					// Show route plan animation
					var route = _navigationSystem.GetRoutePlan(destination);
					double requiredFuelAfterLaunch = _navigationSystem.CalculateRequiredFuel(destination, spacecraft.Type) - 50;
					spacecraft.ConsumeFuel(requiredFuelAfterLaunch);

					await DisplayRouteAnimationAsync(spacecraft.Name, route, spacecraft.FuelLevel);
				}
				else
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine($"\nLaunch failed: {result.Message}");
					Console.ResetColor();
				}
			}
			catch (Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"Error during launch: {ex.Message}");
				Console.ResetColor();
			}
		}

		private void PlanMission()
		{
			Console.Clear();
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("=== PLAN MISSION ===");
			Console.ResetColor();
			Console.WriteLine();

			try
			{
				Spacecraft? spacecraft = GetSpacecraftChoice();

				if (spacecraft is null) return;

				string destChoice = GetDestinationChoice();

				bool isReachable = _navigationSystem.IsDestinationReachable(spacecraft.FuelLevel, destChoice, spacecraft.Type); 

				if(!isReachable)
				{
					Console.ForegroundColor = ConsoleColor.Red;
					double requiredFuel = _navigationSystem.CalculateRequiredFuel(destChoice, spacecraft.Type);
					Console.WriteLine($"Warning! {destChoice} is not reachable with current fuel level");
					Console.WriteLine($"Required fuel: {requiredFuel}, Available: {spacecraft.FuelLevel}");
					Console.ResetColor();
					return;
				}

				var routePlan = _missionControl.PlanMission(spacecraft.Id, destChoice);

				// Display route plan
				Console.Clear();
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine($"=== MISSION PLAN: {spacecraft.Name} to {destChoice} ===");
				Console.ResetColor();
				Console.WriteLine();

				Console.WriteLine("Route Plan:");

				for(int i = 0; i < routePlan.Count; i++)
				{
					Console.Write($"{i + 1}. {routePlan[i]}");

					if(i < routePlan.Count - 1)
					{
						Console.Write(" --> ");
					}

					if((i + 1) % 3 == 0)
					{
						Console.WriteLine();
					}
				}

				Console.WriteLine("\n");
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("Mission planning complete!");
				Console.ResetColor();
			}
			catch (Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"Error during mission planning: {ex.Message}");
				Console.ResetColor();
			}
		}

		private void RefuelSpacecraft()
		{
			Console.Clear();
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("=== REFUEL SPACECRAFT ===");
			Console.ResetColor();
			Console.WriteLine();

			try
			{
				Spacecraft? spacecraft = GetSpacecraftChoice();

				if (spacecraft is null) return;

				double fuelAmount = GetFuelAmountChoice();

				double previousFuel = spacecraft.FuelLevel;

				spacecraft.Refuel(fuelAmount);

				// Show animation
				Console.WriteLine();
				Console.WriteLine("Refueling in progress");

				for(int i = 0; i < 20; i++)
				{
					Console.Write("█");
					Thread.Sleep(100);
				}

				Console.WriteLine("\n");
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine($"Refueling complete! Added {fuelAmount} units.");
				Console.WriteLine($"Previous fuel level: {previousFuel}");
				Console.WriteLine($"New fuel level: {spacecraft.FuelLevel}");
				Console.ResetColor();
			}
			catch (Exception)
			{

				throw;
			}
		}

		#endregion

		#region Display Methods

		private async void DisplayWeatherReport()
		{
			Console.Clear();
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("=== WEATHER CONDITIONS ===");
			Console.ResetColor();
			Console.WriteLine();

			try
			{
				string launchSite = GetLaunchSiteChoice();

				Console.WriteLine();
				Console.Write("Connecting to weather satellites");

				for(int i = 0; i < 5; i++)
				{
					Console.Write(".");
					Thread.Sleep(300);
				}

				Console.WriteLine();

				var weather = await _weatherService.GetCurrentWeatherAsync(launchSite);
				bool isSafe = _weatherService.IsSafeToLaunch(weather);

				Console.Clear();
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine($"=== WEATHER REPORT: {launchSite} ===");
				Console.ResetColor();
				Console.WriteLine();

				// ASCII art weather icon
				if (weather.Lightning)
				{
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.WriteLine(@"    .-.
   (   ).
  (___(__)
   /\/\/\
  /      \
 ⚡️  ⚡️  ⚡️");
					Console.ResetColor();
				}
				else if (weather.WindSpeed > 25)
				{
					Console.ForegroundColor = ConsoleColor.Cyan;
					Console.WriteLine(@"    .-.
   (   ).
  (___(__)
    ~~~~
   ~~~~~
  ~~~~~~");
					Console.ResetColor();
				}
				else
				{
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.WriteLine(@"    \   /
     .-.
  ‒ (   ) ‒
     `-᾿
    /   \");
					Console.ResetColor();
				}

				Console.WriteLine();
				Console.WriteLine($"Temperature: {weather.Temperature:F1} deg C");
				Console.WriteLine($"Wind Speed: {weather.WindSpeed:F1} mph");
				Console.WriteLine($"Visibility: {weather.Visibility:F1} miles");
				Console.WriteLine($"Lightning: {(weather.Lightning ? "Yes" : "No")}");
				Console.WriteLine();

				if (isSafe)
				{
					Console.ForegroundColor = ConsoleColor.Green;
					Console.WriteLine("LAUNCH CONDITIONS: GO FOR LAUNCH");
				}
				else
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("LAUNCH CONDITIONS: NO GO - UNSAFE");

					Console.WriteLine("\nIssues:");
					if (weather.WindSpeed >= 30)
						Console.WriteLine("- Wind speed too high");
					if (weather.Visibility <= 3)
						Console.WriteLine("- Poor visibility");
					if (weather.Lightning)
						Console.WriteLine("- Lightning detected");
					if (weather.Temperature < -10 || weather.Temperature > 35)
						Console.WriteLine("- Temperature out of range");
				}

				Console.ResetColor();
			}
			catch (Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"Error getting weather data: {ex.Message}");
				Console.ResetColor();
			}
		}

		private async Task DisplayRouteAnimationAsync(string name, List<string> route, double fuelAfterTrip)
		{
			Console.Clear();
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine($"=== ROUTE PLAN FOR {name.ToUpper()} ===");
			Console.ResetColor();
			Console.WriteLine();

			// Display the ASCII spacecraft
			string spacecraft = @"
      .-.
     (   )
      '-'
     /   \
    /     \
";

			// Show initial position
			Console.WriteLine("Earth");
			Console.WriteLine(spacecraft);

			// Wait a moment before starting the journey
			await Task.Delay(1500);

			// Display space travel animation
			for (int i = 0; i < route.Count; i++)
			{
				Console.Clear();

				// Show progress bar
				Console.Write("[");
				for (int j = 0; j <= i; j++)
				{
					Console.Write("=");
				}
				for (int j = i + 1; j < route.Count; j++)
				{
					Console.Write(" ");
				}
				Console.WriteLine("]");

				// Show current location
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine($"Current location: {route[i]}");
				Console.ResetColor();

				// Show spacecraft with "jet stream"
				string jetStream = new string('.', i + 1);
				Console.WriteLine($"{jetStream}{spacecraft}");

				// Show the route so far
				Console.WriteLine("\nRoute progress:");
				for (int j = 0; j <= i; j++)
				{
					if (j > 0)
					{
						Console.Write(" -> ");
					}

					if (j == i)
					{
						Console.ForegroundColor = ConsoleColor.Green;
					}

					Console.Write(route[j]);
					Console.ResetColor();
				}

				// If we're at the destination, highlight it
				if (i == route.Count - 1)
				{
					Console.WriteLine("\n");
					Console.ForegroundColor = ConsoleColor.Green;
					Console.WriteLine($"DESTINATION REACHED: {route[i]}");
					Console.WriteLine($"REMAINING FUEL: {fuelAfterTrip}");
					Console.ResetColor();
				}
				else
				{
					// Show in progress
					// Mission complete message
					Console.WriteLine("\n");
					Console.ForegroundColor = ConsoleColor.Magenta;
					Console.WriteLine($"Mission Progress: {name} is now traveling to its destination.");
					Console.ResetColor();
				}

				// Pause before next step
				await Task.Delay(1000);
			}

			await Task.Delay(2000);
		}

		private void DisplayLaunchAnimation()
		{
			Console.Clear();

			string[] frames = new[]
			{
		@"
          
          
          
          
      /|\
     / | \
    /__|__\
    |  |  |
    |  |  |
    |__|__|
    |__|__|
    |__|__|
   /|__|__|\
  / |__|__| \
 /__|__|__|__\
|___|__|__|___|
    |__|__|
     /__\
    //////
    
    ",
		@"
          
          
          
          
      /|\    
     / | \   *
    /__|__\  
    |  |  |   *
    |  |  |  
    |__|__|   *
    |__|__|  
    |__|__|   *
   /|__|__|\  
  / |__|__| \ *
 /__|__|__|__\
|___|__|__|___|
    |__|__|    
     /__\      
    //////     
    *    *   *
    ",
		@"
          
          
          
          
      /|\      *
     / | \    * *
    /__|__\     *
    |  |  |    * *
    |  |  |   *  *
    |__|__|    * *
    |__|__|   *  *
    |__|__|    * *
   /|__|__|\  *  *
  / |__|__| \   **
 /__|__|__|__\*  *
|___|__|__|___|  *
    |__|__|    ***
     /__\      * *
    //////     ***
   **    **   ** **
    ",
		@"
          
          
           *
          * *
      /|\ *  *
     / | \* * *
    /__|__\*  *
    |  |  |** **
    |  |  |* * *
    |__|__|** **
    |__|__|* * *
    |__|__|******
   /|__|__|\*****
  / |__|__| \****
 /__|__|__|__\***
|___|__|__|___|**
    |__|__|   ****
     /__\    *****
             *****
            *******
    ",
		@"
        *
       * *
      *   *
     *  *  *
    *   *   *
   *  *   *  *
  *   *   *   *
 *  *   *   *  *
*   *   *   *   *
    ",
		@"
         
        ***
       *****
      *******
     *  *  *  *
    *   *   *   *
   *     *     *
  *       *       *
 *         *         *
    ",
		@"
         
         
        *****
       *******
      *********
     ***********
    *************
   ***************
  *****************
    "
	};

			// Display each frame with a short delay
			foreach (var frame in frames)
			{
				Console.Clear();
				Console.WriteLine(frame);
				Thread.Sleep(500); // Half-second delay between frames
			}

			// Final message after animation
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("SPACECRAFT SUCCESSFULLY LAUNCHED!");
			Console.ResetColor();
			Thread.Sleep(1000);
		}

		private void DisplayLaunchCountdown(string name, string launchSite, string destination)
		{
			Console.Clear();
			Console.WriteLine($"Preparing to launch {name} to {destination} from {launchSite}");
			Console.WriteLine();

			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("LAUNCH SEQUENCE INITIATED");
			Console.ResetColor();

			// Countdown Animation
			for (int i = 10; i > 0; i--)
			{
				Console.Write($"\rT-minus {i}...");
				Thread.Sleep(500);
			}

			Console.WriteLine("\rLAUNCH!                ");
		}

		#endregion

		#region Input Methods

		private int GetMenuChoice(int maxChoice)
		{
			while (true)
			{
				if (int.TryParse(Console.ReadLine(), out int choice) && choice >= 1 && choice <= maxChoice)
				{
					return choice;
				}

				Console.ForegroundColor = ConsoleColor.Red;
				Console.Write($"Invalid choice, please enter a nubmer between 1 and {maxChoice}");
				Console.ResetColor();
			}
		}

		private string GetDestinationChoice()
		{
			// Pick a destination
			Console.WriteLine("\nSelect a destination");
			for (int i = 0; i < _destinations.Length; i++)
			{
				Console.WriteLine($"{i + 1}. {_destinations[i]}");
			}

			Console.Write($"Enter choice (1-{_destinations.Length}): ");
			int destChoice = GetMenuChoice(_destinations.Length);
			return _destinations[destChoice - 1];
		}

		private string GetLaunchSiteChoice()
		{
			// Pick a launch site
			Console.WriteLine("\nSelect a Launch Site:");
			for (int i = 0; i < _launchSites.Length; i++)
			{
				Console.WriteLine($"{i + 1}. {_launchSites[i]}");
			}

			Console.Write($"Enter choice (1-{_launchSites.Length}): ");
			int siteChoice = GetMenuChoice(_launchSites.Length);
			return _launchSites[siteChoice - 1];
		}

		private Spacecraft? GetSpacecraftChoice()
		{
			// First, display all spacecraft to choose from
			DisplayAllSpacecraft();

			Console.WriteLine();

			Console.Write("Enter spacecraft ID to launch: ");
			string spacecraftId = Console.ReadLine();
			try
			{
				return _missionControl.GetSpacecraft(spacecraftId);
			}
			catch (KeyNotFoundException)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"Error: Spacecraft with ID {spacecraftId} not found");
				return null;
			}
		}

		private double GetFuelAmountChoice()
		{
			Console.Write("Enter amount of fuel to add: ");

			while(true)
			{
				if(double.TryParse(Console.ReadLine(), out var result) && result > 0)
				{
					return result;
				}

				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Error: Please enter a positive fuel amount");
				Console.ResetColor();
			}
		}

		#endregion
	}
}
