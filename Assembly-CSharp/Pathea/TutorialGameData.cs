using System.IO;

namespace Pathea;

public class TutorialGameData
{
	private YirdData mYirdData;

	private static readonly string s_WorldDir = "Mainland";

	public YirdData yirdData => mYirdData;

	public bool Load(string dir)
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(dir, s_WorldDir));
		if (!directoryInfo.Exists)
		{
			return false;
		}
		mYirdData = new YirdData(directoryInfo.FullName);
		return true;
	}
}
