using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceMissionControl.Application.Telemetry
{
	public interface ITelemetryService
	{
		Task<bool> SendTelemetryAsync(string spacecraftId, Dictionary<string, string> data);
		Task<bool> VerifyConnectionAsync();
	}
}
