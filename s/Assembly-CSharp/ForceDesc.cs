using System;
using System.Collections.Generic;
using UnityEngine;

public class ForceDesc
{
	public int ID;

	public Color32 Color;

	public string Name;

	public List<int> Allies = new List<int>();

	public int JoinablePlayerCount;

	public int JoinWorld;

	public Vector3 JoinLocation = Vector3.zero;

	public bool PublicInventory = true;

	public bool ItemUseShare = true;

	public bool ItemShare = true;

	public bool InternalConflict = true;

	public bool AllyConflict = true;

	public bool EnemyConflict = true;

	public string JoinStr
	{
		get
		{
			return JoinWorld + "|" + JoinLocationStr;
		}
		set
		{
			string[] array = value.Split(new string[1] { "|" }, StringSplitOptions.RemoveEmptyEntries);
			if (array.Length > 1)
			{
				int.TryParse(array[0], out JoinWorld);
				JoinLocationStr = array[1];
			}
			else if (array.Length == 1)
			{
				JoinWorld = 0;
				JoinLocationStr = array[0];
			}
			else
			{
				JoinWorld = 0;
				JoinLocation = Vector3.zero;
			}
		}
	}

	public string JoinLocationStr
	{
		get
		{
			return JoinLocation.x.ToString("0.##") + "," + JoinLocation.y.ToString("0.##") + "," + JoinLocation.z.ToString("0.##");
		}
		set
		{
			try
			{
				string[] array = value.Split(new string[1] { "," }, StringSplitOptions.RemoveEmptyEntries);
				if (array.Length > 0)
				{
					float.TryParse(array[0], out JoinLocation.x);
				}
				if (array.Length > 1)
				{
					float.TryParse(array[1], out JoinLocation.y);
				}
				if (array.Length > 2)
				{
					float.TryParse(array[2], out JoinLocation.z);
				}
			}
			catch
			{
				JoinLocation = Vector3.zero;
			}
		}
	}
}
