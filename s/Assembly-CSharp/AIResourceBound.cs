using System;
using UnityEngine;

public struct AIResourceBound
{
	public int type;

	public float arg0;

	public float arg1;

	public float arg2;

	public float radius => type switch
	{
		0 => 0f, 
		1 => arg0, 
		2 => arg0, 
		3 => arg0, 
		4 => arg0, 
		5 => arg2, 
		_ => 0f, 
	};

	public bool CheckObstacle(Vector3 position, Quaternion rot, LayerMask layer)
	{
		switch (type)
		{
		case 0:
			return false;
		case 1:
		{
			Vector3 start2 = position + rot * Vector3.right * arg1 * 0.5f;
			Vector3 end3 = position - rot * Vector3.right * arg1 * 0.5f;
			return Physics.CheckCapsule(start2, end3, arg0, layer);
		}
		case 2:
		{
			Vector3 end2 = position + Vector3.up * arg1;
			return Physics.CheckCapsule(position, end2, arg0, layer);
		}
		case 3:
		{
			Vector3 start = position + rot * Vector3.forward * arg1 * 0.5f;
			Vector3 end = position - rot * Vector3.forward * arg1 * 0.5f;
			return Physics.CheckCapsule(start, end, arg0, layer);
		}
		case 4:
			return Physics.CheckSphere(position + Vector3.up * arg0, arg0, layer);
		case 5:
			return false;
		default:
			return false;
		}
	}

	public void ToResourceBound(string s)
	{
		if (s == null || s == string.Empty || s == "0")
		{
			return;
		}
		string[] array = s.Split(',');
		if (array.Length != 4)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarning("Resource bound data is error!" + s);
			}
		}
		else
		{
			type = Convert.ToInt32(array[0]);
			arg0 = Convert.ToSingle(array[1]);
			arg1 = Convert.ToSingle(array[2]);
			arg2 = Convert.ToSingle(array[3]);
		}
	}
}
