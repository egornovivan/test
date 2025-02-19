using System;
using UnityEngine;

public class PlayerDesc
{
	public int ID;

	public int Force;

	public string Name;

	public EPlayerType Type;

	public int StartWorld;

	public Vector3 StartLocation;

	public string StartStr
	{
		get
		{
			return StartWorld + "|" + StartLocationStr;
		}
		set
		{
			string[] array = value.Split(new string[1] { "|" }, StringSplitOptions.RemoveEmptyEntries);
			if (array.Length > 1)
			{
				int.TryParse(array[0], out StartWorld);
				StartLocationStr = array[1];
			}
			else if (array.Length == 1)
			{
				StartWorld = 0;
				StartLocationStr = array[0];
			}
			else
			{
				StartWorld = 0;
				StartLocation = Vector3.zero;
			}
		}
	}

	public string StartLocationStr
	{
		get
		{
			return StartLocation.x.ToString("0.##") + "," + StartLocation.y.ToString("0.##") + "," + StartLocation.z.ToString("0.##");
		}
		set
		{
			try
			{
				string[] array = value.Split(new string[1] { "," }, StringSplitOptions.RemoveEmptyEntries);
				if (array.Length > 0)
				{
					float.TryParse(array[0], out StartLocation.x);
				}
				if (array.Length > 1)
				{
					float.TryParse(array[1], out StartLocation.y);
				}
				if (array.Length > 2)
				{
					float.TryParse(array[2], out StartLocation.z);
				}
			}
			catch
			{
				StartLocation = Vector3.zero;
			}
		}
	}
}
