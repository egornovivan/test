namespace InControl;

public class Logger
{
	public delegate void LogMessageHandler(LogMessage message);

	public static event LogMessageHandler OnLogMessage;

	public static void LogInfo(string text)
	{
		if (Logger.OnLogMessage != null)
		{
			LogMessage logMessage = default(LogMessage);
			logMessage.text = text;
			logMessage.type = LogMessageType.Info;
			LogMessage message = logMessage;
			Logger.OnLogMessage(message);
		}
	}

	public static void LogWarning(string text)
	{
		if (Logger.OnLogMessage != null)
		{
			LogMessage logMessage = default(LogMessage);
			logMessage.text = text;
			logMessage.type = LogMessageType.Warning;
			LogMessage message = logMessage;
			Logger.OnLogMessage(message);
		}
	}

	public static void LogError(string text)
	{
		if (Logger.OnLogMessage != null)
		{
			LogMessage logMessage = default(LogMessage);
			logMessage.text = text;
			logMessage.type = LogMessageType.Error;
			LogMessage message = logMessage;
			Logger.OnLogMessage(message);
		}
	}
}
