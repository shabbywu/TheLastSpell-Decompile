using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace TheLastStand.Framework.Diagnostic;

public class PerformanceLogger
{
	private const byte SECONDS_BETWEEN_LOG = 1;

	private const string FILE_EXTENSION = ".CSV";

	private readonly string outputLogPath;

	private bool shouldDie;

	public PerformanceLogger(string outputLogPath)
	{
		Debug.Log((object)"Performance logger starting...");
		this.outputLogPath = outputLogPath + ".CSV";
		Task.Run((Func<Task?>)Log);
	}

	public void Kill()
	{
		shouldDie = true;
		Debug.Log((object)"Killing performance log...");
	}

	private async Task Log()
	{
		_ = 1;
		try
		{
			using FileStream fs = new FileStream(outputLogPath, FileMode.Create);
			using StreamWriter sw = new StreamWriter(fs);
			sw.AutoFlush = false;
			using Process proc = Process.GetCurrentProcess();
			while (!shouldDie)
			{
				sw.WriteLine($"{DateTime.Now.ToLongTimeString()} {proc.ProcessName}#{proc.Id} ==============");
				sw.WriteLine($"  Physical memory usage     : {proc.WorkingSet64}");
				sw.WriteLine($"  Base priority             : {proc.BasePriority}");
				sw.WriteLine($"  Priority class            : {proc.PriorityClass}");
				sw.WriteLine($"  User processor time       : {proc.UserProcessorTime}");
				sw.WriteLine($"  Privileged processor time : {proc.PrivilegedProcessorTime}");
				sw.WriteLine($"  Total processor time      : {proc.TotalProcessorTime}");
				sw.WriteLine($"  Paged system memory size  : {proc.PagedSystemMemorySize64}");
				sw.WriteLine($"  Paged memory size         : {proc.PagedMemorySize64}");
				proc.Refresh();
				await Task.Delay(1000);
				await fs.FlushAsync();
			}
			Debug.Log((object)"Dead.");
		}
		catch (Exception ex)
		{
			Debug.LogError((object)ex);
		}
	}
}
