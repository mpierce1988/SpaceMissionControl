using SpaceMissionControl.Model.Spacecraft;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceMissionControl.Application.Navigation
{
	public interface INavigationSystem
	{
		double CalculateRequiredFuel(string destination, SpacecraftType spacecraftType);
		bool IsDestinationReachable(double availableFuel, string destination, SpacecraftType spacecraftType);
		List<string> GetRoutePlan(string destination);
	}
}
