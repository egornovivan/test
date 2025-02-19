using UnityEngine;

namespace WhiteCat;

public class BoneConfig : ScriptableObject
{
	public string[] boneNames;

	public string[] searchFolders;

	private static BoneConfig _instance;

	public static BoneConfig instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = Resources.Load<BoneConfig>("BoneConfig");
			}
			return _instance;
		}
	}
}
