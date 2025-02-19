public class LogFilter
{
	public enum EFilterLevel
	{
		Develop,
		Debug,
		Info,
		Warn,
		Error,
		Fatal
	}

	public static EFilterLevel curLevel = EFilterLevel.Warn;

	public static bool logDev => curLevel <= EFilterLevel.Develop;

	public static bool logDebug => curLevel <= EFilterLevel.Debug;

	public static bool logInfo => curLevel <= EFilterLevel.Info;

	public static bool logWarn => curLevel <= EFilterLevel.Warn;

	public static bool logError => curLevel <= EFilterLevel.Error;

	public static bool logFatal => curLevel <= EFilterLevel.Fatal;
}
