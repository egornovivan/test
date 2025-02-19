using System;
using System.Collections.Generic;
using UnityEngine;

public struct AISpeciesData
{
	private bool _bGrd;

	private int _id;

	private float _percent;

	public float Percent => _percent;

	public int Id => (!_bGrd) ? _id : (_id | 0x40000000);

	public static AISpeciesData[] AnalysisSpeciesString(string species, bool grp = false)
	{
		if (species == null || species == string.Empty || species == "0")
		{
			return null;
		}
		List<AISpeciesData> list = new List<AISpeciesData>();
		string[] array = species.Split(';');
		string[] array2 = array;
		AISpeciesData item = default(AISpeciesData);
		foreach (string text in array2)
		{
			string[] array3 = text.Split(',');
			if (array3.Length == 2)
			{
				item._bGrd = grp;
				item._id = Convert.ToInt32(array3[0]);
				item._percent = Convert.ToSingle(array3[1]);
				list.Add(item);
			}
		}
		return list.ToArray();
	}

	public static AISpeciesData[] AnalysisSpeciesString(string species, string grpSpecies)
	{
		List<AISpeciesData> list = new List<AISpeciesData>();
		string[] array = species.Split(';');
		string[] array2 = array;
		AISpeciesData item = default(AISpeciesData);
		foreach (string text in array2)
		{
			string[] array3 = text.Split(',');
			if (array3.Length == 2)
			{
				item._bGrd = false;
				item._id = Convert.ToInt32(array3[0]);
				item._percent = Convert.ToSingle(array3[1]);
				list.Add(item);
			}
		}
		array = grpSpecies.Split(';');
		string[] array4 = array;
		AISpeciesData item2 = default(AISpeciesData);
		foreach (string text2 in array4)
		{
			string[] array5 = text2.Split(',');
			if (array5.Length == 2)
			{
				item2._bGrd = true;
				item2._id = Convert.ToInt32(array5[0]);
				item2._percent = Convert.ToSingle(array5[1]);
				list.Add(item2);
			}
		}
		return list.ToArray();
	}

	public static int GetRandomAI(AISpeciesData[] data)
	{
		if (data == null || data.Length == 0)
		{
			Debug.LogError("Failed to get RandomAI data");
			return -1;
		}
		int result = -1;
		float num = 0f;
		float value = UnityEngine.Random.value;
		for (int i = 0; i < data.Length; i++)
		{
			AISpeciesData aISpeciesData = data[i];
			num += aISpeciesData.Percent;
			if (value <= num)
			{
				result = aISpeciesData.Id;
				break;
			}
		}
		return result;
	}

	public static int GetRandomAI(AISpeciesData[] data, float value)
	{
		if (data == null || data.Length == 0)
		{
			return -1;
		}
		int result = -1;
		float num = 0f;
		for (int i = 0; i < data.Length; i++)
		{
			AISpeciesData aISpeciesData = data[i];
			num += aISpeciesData.Percent;
			if (value <= num)
			{
				result = aISpeciesData.Id;
				break;
			}
		}
		return result;
	}
}
