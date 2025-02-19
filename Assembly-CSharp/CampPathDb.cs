using System.Collections.Generic;
using UnityEngine;

public class CampPathDb
{
	private static Dictionary<string, PEPathData> m_PathDic;

	public static void LoadData(string[] paths)
	{
		if (m_PathDic == null)
		{
			m_PathDic = new Dictionary<string, PEPathData>();
		}
		if (paths == null)
		{
			return;
		}
		PEPathData value = default(PEPathData);
		for (int i = 0; i < paths.Length; i++)
		{
			GameObject gameObject = Resources.Load(paths[i]) as GameObject;
			if (!(gameObject != null))
			{
				continue;
			}
			PEPath component = gameObject.GetComponent<PEPath>();
			if (component != null)
			{
				value.warpMode = component.wrapMode;
				value.path = GetPathWay(component.gameObject);
				if (!m_PathDic.ContainsKey(paths[i]))
				{
					m_PathDic.Add(paths[i], value);
				}
			}
		}
	}

	public static void Release()
	{
		m_PathDic.Clear();
		m_PathDic = null;
	}

	public static PEPathData GetPathData(string str)
	{
		return m_PathDic[str];
	}

	private static Vector3[] GetPathWay(GameObject obj)
	{
		List<Vector3> list = new List<Vector3>();
		foreach (Transform item in obj.transform)
		{
			list.Add(item.position);
		}
		return list.ToArray();
	}
}
