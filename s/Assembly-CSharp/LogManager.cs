using System;
using System.IO;
using System.Text;
using UnityEngine;

public class LogManager
{
	private static string LogPath;

	private static string LogFile => $"{LogPath}/{DateTime.Today.Day}_{DateTime.Today.Month}_{DateTime.Today.Year}_log.log";

	private static string WarningFile => $"{LogPath}/{DateTime.Today.Day}_{DateTime.Today.Month}_{DateTime.Today.Year}_warning.log";

	private static string ErrorFile => $"{LogPath}/{DateTime.Today.Day}_{DateTime.Today.Month}_{DateTime.Today.Year}_error.log";

	private static string ExceptionFile => $"{LogPath}/{DateTime.Today.Day}_{DateTime.Today.Month}_{DateTime.Today.Year}_exception.log";

	public static void InitLogManager(string dir)
	{
		LogPath = dir;
		Application.logMessageReceivedThreaded += Application_logMessageReceived;
	}

	private static void Application_logMessageReceived(string condition, string stackTrace, LogType type)
	{
		switch (type)
		{
		case LogType.Log:
		{
			string contents4 = $"[{type}]:{DateTime.Now}\r\n{condition}\r\n{stackTrace}\r\n";
			File.AppendAllText(LogFile, contents4, Encoding.UTF8);
			break;
		}
		case LogType.Warning:
		{
			string contents3 = $"[{type}]:{DateTime.Now}\r\n{condition}\r\n{stackTrace}\r\n";
			File.AppendAllText(WarningFile, contents3, Encoding.UTF8);
			break;
		}
		case LogType.Error:
		case LogType.Assert:
		{
			string contents2 = $"[{type}]:{DateTime.Now}\r\n{condition}\r\n{stackTrace}\r\n";
			File.AppendAllText(ErrorFile, contents2, Encoding.UTF8);
			break;
		}
		case LogType.Exception:
		{
			string contents = $"[{type}]:{DateTime.Now}\r\n{condition}\r\n{stackTrace}\r\n";
			File.AppendAllText(ExceptionFile, contents, Encoding.UTF8);
			break;
		}
		}
	}
}
