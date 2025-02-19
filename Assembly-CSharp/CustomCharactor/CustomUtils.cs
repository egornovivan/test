using System.Collections.Generic;
using UnityEngine;

namespace CustomCharactor;

public static class CustomUtils
{
	public static Transform FindInChildren(Transform transform, string name)
	{
		if (transform.name == name)
		{
			return transform;
		}
		int childCount = transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Transform transform2 = FindInChildren(transform.GetChild(i), name);
			if (transform2 != null)
			{
				return transform2;
			}
		}
		return null;
	}

	public static List<Transform> FindSmrBonesByName(List<Transform> allBonesList, SkinnedMeshRenderer smr)
	{
		Transform[] bones = smr.bones;
		int num = bones.Length;
		List<Transform> list = new List<Transform>(num);
		for (int i = 0; i < num; i++)
		{
			Transform transform = allBonesList.Find((Transform tt) => 0 == string.Compare(bones[i].name, tt.name, ignoreCase: true));
			if (transform != null)
			{
				list.Add(transform);
			}
			else
			{
				Debug.LogError("[FindSmrBonesByName]Cant find bone:" + bones[i].name);
			}
		}
		return list;
	}
}
