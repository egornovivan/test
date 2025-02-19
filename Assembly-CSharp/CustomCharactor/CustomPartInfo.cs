using UnityEngine;

namespace CustomCharactor;

public class CustomPartInfo
{
	public string ModelFilePath { get; private set; }

	public string ModelName { get; private set; }

	public SkinnedMeshRenderer Smr { get; private set; }

	public CustomPartInfo(string modelPathName)
	{
		if (!string.IsNullOrEmpty(modelPathName))
		{
			string[] array = modelPathName.Split(':');
			if (array.Length < 2)
			{
				array = modelPathName.Split('-');
			}
			if (array.Length >= 2)
			{
				ModelFilePath = array[0];
				ModelName = array[1];
				Smr = GetPartSmr();
				return;
			}
		}
		ModelFilePath = (ModelName = string.Empty);
	}

	private SkinnedMeshRenderer GetPartSmr()
	{
		GameObject gameObject = AssetsLoader.Instance.LoadPrefabImm(ModelFilePath, bIntoCache: true) as GameObject;
		if (null == gameObject)
		{
			return null;
		}
		Transform transform = gameObject.transform.FindChild(ModelName);
		if (null == transform)
		{
			return null;
		}
		return transform.GetComponent<SkinnedMeshRenderer>();
	}

	public static string GetModelFilePath(string modelPathName)
	{
		if (!string.IsNullOrEmpty(modelPathName))
		{
			string[] array = modelPathName.Split(':');
			if (array.Length < 2)
			{
				array = modelPathName.Split('-');
			}
			if (array.Length >= 2)
			{
				return array[0];
			}
		}
		return string.Empty;
	}
}
