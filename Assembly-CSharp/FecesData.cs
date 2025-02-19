using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Data.SqliteClient;

public class FecesData
{
	public int id;

	public string path;

	public List<ProbableItem> fixItem = new List<ProbableItem>();

	public List<ProbableItem> probableItems = new List<ProbableItem>();

	public static Dictionary<int, FecesData> fecesDataDict = new Dictionary<int, FecesData>();

	public static void LoadData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("feces");
		while (sqliteDataReader.Read())
		{
			FecesData fecesData = new FecesData();
			fecesData.id = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("id")));
			fecesData.path = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("path"));
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("fixedItemId"));
			string[] array = @string.Split(';');
			string[] array2 = array;
			foreach (string str in array2)
			{
				fecesData.fixItem.Add(default(ProbableItem).ParseString(str));
			}
			string string2 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("randomItem"));
			string[] array3 = string2.Split(';');
			string[] array4 = array3;
			foreach (string str2 in array4)
			{
				fecesData.probableItems.Add(default(ProbableItem).ParseString(str2));
			}
			fecesDataDict.Add(fecesData.id, fecesData);
		}
	}

	public static FecesData GetFecesData(int id)
	{
		if (fecesDataDict.ContainsKey(id))
		{
			return fecesDataDict[id];
		}
		return null;
	}

	public static List<int> GetAllId()
	{
		return fecesDataDict.Keys.ToList();
	}

	public static string GetModelPath(int seed)
	{
		if (seed < 0)
		{
			seed = -seed;
		}
		int count = fecesDataDict.Keys.Count;
		int index = seed % count;
		int key = fecesDataDict.Keys.ToList()[index];
		return fecesDataDict[key].path;
	}
}
