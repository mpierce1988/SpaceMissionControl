using Microsoft.Extensions.DependencyInjection;
using SpaceMissionControl.Application.Mission;
using SpaceMissionControl.Application.Navigation;
using SpaceMissionControl.Application.Telemetry;
using SpaceMissionControl.Application.Weather;
using SpaceMissionControl.Model.Spacecraft;
using System.Threading.Tasks;

namespace SpaceMissionControl.Presentation
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // Set up dependency injection
            var serviceProvider = ConfigureServices();

            Console.Title = "Space Mission Control Center";

            // Get required services
            var missionControl = serviceProvider.GetRequiredService<MissionControlService>();
            var navigationSystem = serviceProvider.GetRequiredService<INavigationSystem>();
            var weatherService = serviceProvider.GetRequiredService<IWeatherService>();

            // Create and register some spacecraft
            RegisterSampleSpacecraft(missionControl);

            // Run the UI
            var ui = new MissionControlUI(missionControl, navigationSystem, weatherService);
            await ui.RunAsync();
        }

        private static ServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // Register HTTP client
            services.AddTransient<HttpClient>();

            // Register services
            services.AddSingleton<MissionControlService>();
            services.AddSingleton<INavigationSystem, NavigationSystem>();
            services.AddSingleton<IWeatherService, SimulatedWeatherService>();
            services.AddSingleton<ITelemetryService, ConsoleTelemetryService>();

            return services.BuildServiceProvider();
        }

        private static void RegisterSampleSpacecraft(MissionControlService missionControl)
        {
            var spacecrafts = new[]
            {
                new Spacecraft("SC-001", "Voyager III", SpacecraftType.Satellite, 200),
                new Spacecraft("SC-002", "Curiosity 2", SpacecraftType.Rover, 150),
                new Spacecraft("SC-003", "Enterprise", SpacecraftType.Shuttle, 500),
                new Spacecraft("SC-004", "Hubble 2", SpacecraftType.Satellite, 120),
                new Spacecraft("SC-005", "ISS Next Gen", SpacecraftType.SpaceStation, 1000)
            };

            foreach(Spacecraft spacecraft in spacecrafts)
            {
                missionControl.RegisterSpacecraft(spacecraft);
            }
        }
    }
}
