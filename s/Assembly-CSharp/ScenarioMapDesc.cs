using System.IO;

public class ScenarioMapDesc
{
	public string UID;

	public string Name;

	public string Path;

	public ScenarioMapDesc()
	{
		UID = string.Empty;
		Name = string.Empty;
		Path = string.Empty;
	}

	public ScenarioMapDesc(string uid, string path)
	{
		UID = uid;
		Path = path;
		DirectoryInfo directoryInfo = new DirectoryInfo(path);
		if (directoryInfo != null)
		{
			Name = directoryInfo.Name;
			Path = directoryInfo.FullName;
		}
		else
		{
			Name = "No Name";
		}
	}
}
