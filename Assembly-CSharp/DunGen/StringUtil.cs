using System.Text.RegularExpressions;

namespace DunGen;

public static class StringUtil
{
	private static Regex capitalLetterPattern = new Regex("([A-Z])");

	public static string SplitCamelCase(string input)
	{
		return capitalLetterPattern.Replace(input, " $1").Trim();
	}
}
