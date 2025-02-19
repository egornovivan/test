using System;
using System.Collections.Generic;
using ItemAsset;
using Mono.Data.SqliteClient;
using UnityEngine;

public class SceneBoxMgr
{
	public static Dictionary<int, SceneBox> m_SceneBoxMap = new Dictionary<int, SceneBox>();

	public static void LoadData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("box");
		sqliteDataReader.Read();
		while (sqliteDataReader.Read())
		{
			SceneBox sceneBox = new SceneBox();
			sceneBox.m_ID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("boxid")));
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("position"));
			if (@string != "0")
			{
				string[] array = @string.Split(',');
				if (array.Length < 3)
				{
					Debug.LogError("Mission's LookPosition is Error");
				}
				float x = Convert.ToSingle(array[0]);
				float y = Convert.ToSingle(array[1]);
				float z = Convert.ToSingle(array[2]);
				sceneBox.m_Pos = new Vector3(x, y, z);
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("rotation"));
			if (@string != "0")
			{
				string[] array2 = @string.Split(',');
				if (array2.Length < 4)
				{
					Debug.LogError("Mission's LookPosition is Error");
				}
				float x2 = Convert.ToSingle(array2[0]);
				float y2 = Convert.ToSingle(array2[1]);
				float z2 = Convert.ToSingle(array2[2]);
				float w = Convert.ToSingle(array2[3]);
				sceneBox.m_Rotation = new Quaternion(x2, y2, z2, w);
			}
			sceneBox.m_items = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("itemlist"));
			m_SceneBoxMap.Add(sceneBox.m_ID, sceneBox);
		}
	}

	public static SceneBox GetBox(int boxId)
	{
		if (m_SceneBoxMap.ContainsKey(boxId))
		{
			return m_SceneBoxMap[boxId];
		}
		return null;
	}

	public static List<int> DescToItems(string desc, List<ItemObject> effItems)
	{
		if (effItems == null)
		{
			return null;
		}
		if (desc != "0")
		{
			List<int> list = new List<int>();
			string[] array = desc.Split(';');
			for (int i = 0; i < array.Length; i++)
			{
				string[] array2 = array[i].Split(',');
				if (array2.Length == 2)
				{
					int itemId = Convert.ToInt32(array2[0]);
					int num = Convert.ToInt32(array2[1]);
					ItemObject itemObject = ItemManager.CreateItem(itemId, num);
					if (itemObject != null)
					{
						effItems.Add(itemObject);
						list.Add(itemObject.instanceId);
					}
				}
			}
			return list;
		}
		return null;
	}
}
