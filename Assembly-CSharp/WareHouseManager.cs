using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using UnityEngine;

public class WareHouseManager : MonoBehaviour
{
	public static Dictionary<int, WareHouse> m_WareHouseMap = new Dictionary<int, WareHouse>();

	public static List<WareHouseObject> m_WareHouseObjectList = new List<WareHouseObject>();

	private Dictionary<AssetReq, int> reqlist = new Dictionary<AssetReq, int>();

	private void Start()
	{
		RemoveAll();
		string path = "Prefab/Item/Scene/emergency_kit";
		foreach (KeyValuePair<int, WareHouse> item in m_WareHouseMap)
		{
			UnityEngine.Object @object = Resources.Load(path);
			if (@object != null)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(@object, item.Value.m_Pos, item.Value.m_Rotation) as GameObject;
				gameObject.name = "emergency_kit";
				gameObject.transform.parent = base.transform;
				WareHouseObject component = gameObject.GetComponent<WareHouseObject>();
				component._id = item.Value.m_ID;
			}
		}
	}

	private void CreateWareHouse(GameObject go, int id)
	{
		go.name = "emergency_kit";
		go.transform.parent = base.transform;
		go.transform.localScale = Vector3.one;
		WareHouseObject component = go.GetComponent<WareHouseObject>();
		component._id = id;
	}

	public void OnSpawned(GameObject go, AssetReq req)
	{
		if (reqlist.ContainsKey(req))
		{
			CreateWareHouse(go, reqlist[req]);
			reqlist.Remove(req);
		}
		else
		{
			UnityEngine.Object.Destroy(go);
		}
	}

	private void RemoveAll()
	{
		for (int i = 0; i < m_WareHouseObjectList.Count; i++)
		{
			if (!(m_WareHouseObjectList[i] == null))
			{
				UnityEngine.Object.Destroy(m_WareHouseObjectList[i].gameObject);
			}
		}
		m_WareHouseObjectList.Clear();
	}

	public static void AddWareHouseObjectList(WareHouseObject obj)
	{
		if (!m_WareHouseObjectList.Contains(obj))
		{
			m_WareHouseObjectList.Add(obj);
		}
	}

	public static void RemoveWareHouseObjectList(WareHouseObject obj)
	{
		if (m_WareHouseObjectList.Contains(obj))
		{
			m_WareHouseObjectList.Remove(obj);
		}
	}

	public static WareHouseObject GetWareHouseObject(int id)
	{
		return m_WareHouseObjectList.Find((WareHouseObject ite) => WareHouseObject.MatchID(ite, id));
	}

	public static void LoadData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("box");
		sqliteDataReader.Read();
		while (sqliteDataReader.Read())
		{
			WareHouse wareHouse = new WareHouse();
			wareHouse.m_ID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("boxid")));
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
				wareHouse.m_Pos = new Vector3(x, y, z);
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
				wareHouse.m_Rotation = new Quaternion(x2, y2, z2, w);
			}
			wareHouse.m_itemsDesc = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("itemlist"));
			m_WareHouseMap.Add(wareHouse.m_ID, wareHouse);
		}
	}

	public static WareHouse GetWareHouseData(int id)
	{
		if (m_WareHouseMap.ContainsKey(id))
		{
			if (m_WareHouseMap[id] == null)
			{
				return null;
			}
			return m_WareHouseMap[id];
		}
		return null;
	}
}
