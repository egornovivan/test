using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding;

[ExecuteInEditMode]
public class UnityReferenceHelper : MonoBehaviour
{
	[SerializeField]
	[HideInInspector]
	private string guid;

	public string GetGUID()
	{
		return guid;
	}

	public void Awake()
	{
		Reset();
	}

	public void Reset()
	{
		if (guid == null || guid == string.Empty)
		{
			guid = Guid.NewGuid().ToString();
			Debug.Log("Created new GUID - " + guid);
			return;
		}
		UnityReferenceHelper[] array = Object.FindObjectsOfType(typeof(UnityReferenceHelper)) as UnityReferenceHelper[];
		foreach (UnityReferenceHelper unityReferenceHelper in array)
		{
			if (unityReferenceHelper != this && guid == unityReferenceHelper.guid)
			{
				guid = Guid.NewGuid().ToString();
				Debug.Log("Created new GUID - " + guid);
				break;
			}
		}
	}
}
