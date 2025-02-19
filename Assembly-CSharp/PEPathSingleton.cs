using System.Collections.Generic;
using Pathea;
using PETools;
using UnityEngine;

public class PEPathSingleton : MonoLikeSingleton<PEPathSingleton>
{
	private Dictionary<string, PEPathData> m_PathDic;

	protected override void OnInit()
	{
		base.OnInit();
		m_PathDic = new Dictionary<string, PEPathData>();
	}

	public PEPathData GetPathData(string pathName)
	{
		if (m_PathDic.ContainsKey(pathName))
		{
			return m_PathDic[pathName];
		}
		GameObject gameObject = Resources.Load(pathName) as GameObject;
		if (gameObject != null)
		{
			PEPath component = gameObject.GetComponent<PEPath>();
			if (component != null)
			{
				PEPathData pEPathData = default(PEPathData);
				pEPathData.warpMode = component.wrapMode;
				pEPathData.path = GetPathWay(component.gameObject);
				string key = PEUtil.ToPrefabName(component.name);
				if (!m_PathDic.ContainsKey(key))
				{
					m_PathDic.Add(key, pEPathData);
				}
				return pEPathData;
			}
		}
		return default(PEPathData);
	}

	private Vector3[] GetPathWay(GameObject obj)
	{
		List<Vector3> list = new List<Vector3>();
		foreach (Transform item in obj.transform)
		{
			list.Add(item.position);
		}
		return list.ToArray();
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		m_PathDic.Clear();
	}
}
