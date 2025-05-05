using SpaceMissionControl.Application.Telemetry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceMissionControl.Presentation
{
	public class ConsoleTelemetryService : ITelemetryService
	{
		public Task<bool> SendTelemetryAsync(string spacecraftId, Dictionary<string, string> data)
		{
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine($"\n[TELEMETRY] Spacecraft {spacecraftId} data:");

			foreach(var (key, value) in data)
			{
				Console.WriteLine($"   {key}: {value}");
			}

			Console.ResetColor();
			return Task.FromResult(true);
		}

		public Task<bool> VerifyConnectionAsync()
		{
			return Task.FromResult(true);
		}
	}
}
