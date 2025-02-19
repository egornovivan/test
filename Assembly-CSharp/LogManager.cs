using System;
using System.IO;
using uLink;
using UnityEngine;

public class LogManager
{
	private static NetworkLogFlags customLogFlags;

	public static void InitLogManager()
	{
		try
		{
			string dir = Path.Combine(Application.dataPath, "../log");
			if (!Directory.Exists(dir))
			{
				Directory.CreateDirectory(dir);
			}
			SetLevel(NetworkLogFlags.AuthoritativeServer, uLink.NetworkLogLevel.Debug);
			NetworkLog.errorWriter = delegate(NetworkLogFlags flags, object[] args)
			{
				string text = $"{DateTime.Now} => {NetworkLogUtility.ObjectsToString(args)}{Environment.NewLine}";
				string text2 = $"{dir}/{DateTime.Today.Year:d4}{DateTime.Today.Month:d2}{DateTime.Today.Day:d2}_error.log";
				File.AppendAllText(text2.ToString(), text.ToString());
			};
			NetworkLog.warningWriter = delegate(NetworkLogFlags flags, object[] args)
			{
				string text3 = $"{DateTime.Now} => {NetworkLogUtility.ObjectsToString(args)}{Environment.NewLine}";
				string text4 = $"{dir}/{DateTime.Today.Year:d4}{DateTime.Today.Month:d2}{DateTime.Today.Day:d2}_warning.log";
				File.AppendAllText(text4.ToString(), text3.ToString());
			};
			NetworkLog.infoWriter = delegate(NetworkLogFlags flags, object[] args)
			{
				string text5 = $"{DateTime.Now} => {NetworkLogUtility.ObjectsToString(args)}{Environment.NewLine}";
				string text6 = $"{dir}/{DateTime.Today.Year:d4}{DateTime.Today.Month:d2}{DateTime.Today.Day:d2}_info.log";
				File.AppendAllText(text6.ToString(), text5.ToString());
			};
		}
		catch (Exception ex)
		{
			Debug.Log("Failed to InitLogManager! " + ex.ToString());
		}
	}

	public static void SetLevel(NetworkLogFlags logFlags, uLink.NetworkLogLevel logLevel)
	{
		customLogFlags = logFlags;
		NetworkLog.SetLevel(logFlags, logLevel);
	}

	public static void Info(params object[] args)
	{
		Info(customLogFlags, args);
	}

	public static void Info(NetworkLogFlags flags, params object[] args)
	{
		NetworkLog.Info(flags, args);
	}

	public static void Warning(params object[] args)
	{
		Warning(customLogFlags, args);
	}

	public static void Warning(NetworkLogFlags flags, params object[] args)
	{
		NetworkLog.Warning(flags, args);
	}

	public static void Error(params object[] args)
	{
		Error(customLogFlags, args);
	}

	public static void Error(NetworkLogFlags flags, params object[] args)
	{
		NetworkLog.Error(flags, args);
	}
}
