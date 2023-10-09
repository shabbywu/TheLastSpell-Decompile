using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace TheLastStand.Framework.Debugging;

public static class DiskLogger
{
	private static string logFileName = "game_log";

	private static string logFileSuffix = "log";

	private static StreamWriter logWriter;

	public static void Start(string gameName, byte logsToKeep = 10)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		logFileName = gameName;
		Rotate(logsToKeep);
		logWriter = new StreamWriter(new FileStream(Path.Combine(Path.GetDirectoryName(Application.consoleLogPath), GetLogFileName(0)), FileMode.Create), Encoding.UTF8);
		Application.logMessageReceivedThreaded += new LogCallback(Log);
	}

	private static string GetLogFileName(byte rotationIndex)
	{
		if (rotationIndex == 0)
		{
			return logFileName + "." + logFileSuffix;
		}
		return $"{logFileName}.{rotationIndex}.{logFileSuffix}";
	}

	private static void Log(string condition, string stackTrace, LogType type)
	{
		lock (logWriter)
		{
			logWriter.WriteLine(((object)(LogType)(ref type)).ToString() + ": " + condition);
			logWriter.WriteLine();
			logWriter.WriteLine("=== STACK TRACE ===");
			logWriter.WriteLine(stackTrace);
		}
	}

	private static void Rotate(byte logsToKeep = 10)
	{
		if (logsToKeep <= 0)
		{
			throw new Exception($"Invalid number of logs to keep ({logsToKeep} ??) given to the disk logger");
		}
		string directoryName = Path.GetDirectoryName(Application.consoleLogPath);
		string path = Path.Combine(directoryName, GetLogFileName(logsToKeep));
		if (File.Exists(path))
		{
			File.Delete(path);
		}
		for (int num = logsToKeep - 1; num >= 0; num--)
		{
			string text = Path.Combine(directoryName, GetLogFileName((byte)num));
			if (File.Exists(text))
			{
				File.Move(text, Path.Combine(directoryName, GetLogFileName((byte)(num + 1))));
			}
		}
	}
}
