using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceMissionControl.Model.Spacecraft
{
	public class LaunchResult
	{
		public bool Success { get; set; }
		public string Message { get; set; }

		public LaunchResult(bool success, string message)
		{
			Success = success; 
			Message = message;
		}
	}
}
