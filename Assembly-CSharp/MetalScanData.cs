using System;
using System.Collections.Generic;
using System.IO;
using Mono.Data.SqliteClient;
using UnityEngine;

public class MetalScanData
{
	public delegate void OnAddMetalEvent();

	public static Dictionary<int, MetalScanItem> mMetalDic = new Dictionary<int, MetalScanItem>();

	public static List<int> m_ActiveIDList = new List<int>();

	public static List<bool> m_ScanState = new List<bool>();

	public static event OnAddMetalEvent e_OnAddMetal;

	static MetalScanData()
	{
		MetalScanData.e_OnAddMetal = null;
	}

	public static void LoadData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("Mineral");
		while (sqliteDataReader.Read())
		{
			MetalScanItem metalScanItem = new MetalScanItem();
			metalScanItem.mMatName = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("IconName"));
			metalScanItem.mTexName = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TexName"));
			metalScanItem.mDesID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Des")));
			metalScanItem.mType = Convert.ToByte(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("VoxelType")));
			string[] array = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Color")).Split(',');
			metalScanItem.mColor = new Color(Convert.ToSingle(array[0]) / 255f, Convert.ToSingle(array[1]) / 255f, Convert.ToSingle(array[2]) / 255f);
			mMetalDic[Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ID")))] = metalScanItem;
		}
	}

	public static MetalScanItem GetItemByID(int ID)
	{
		if (mMetalDic.ContainsKey(ID))
		{
			return mMetalDic[ID];
		}
		return null;
	}

	public static MetalScanItem GetItemByVoxelType(byte type)
	{
		foreach (MetalScanItem value in mMetalDic.Values)
		{
			if (value.mType == type)
			{
				return value;
			}
		}
		return null;
	}

	public static Color GetColorByType(byte type)
	{
		return GetItemByVoxelType(type)?.mColor ?? Color.white;
	}

	public static void Clear()
	{
		m_ActiveIDList.Clear();
		m_ScanState.Clear();
	}

	public static bool HasMetal(int metalId)
	{
		return m_ActiveIDList.Contains(metalId);
	}

	public static void AddMetalScan(IEnumerable<int> metalID, bool openWnd = true)
	{
		bool flag = false;
		foreach (int item in metalID)
		{
			if (!m_ActiveIDList.Contains(item))
			{
				m_ActiveIDList.Add(item);
				m_ScanState.Add(item: true);
				if (m_ActiveIDList.Count == 1 && openWnd)
				{
					GameUI.Instance.mPhoneWnd.Show(UIPhoneWnd.PageSelect.Page_Scan);
				}
				flag = true;
			}
		}
		if (MetalScanData.e_OnAddMetal != null && flag)
		{
			MetalScanData.e_OnAddMetal();
		}
	}

	public static void AddMetalScan(int metalID)
	{
		bool flag = false;
		if (!m_ActiveIDList.Contains(metalID))
		{
			m_ActiveIDList.Add(metalID);
			m_ScanState.Add(item: true);
			if (m_ActiveIDList.Count == 1)
			{
				GameUI.Instance.mPhoneWnd.Show(UIPhoneWnd.PageSelect.Page_Scan);
			}
			flag = true;
		}
		if (MetalScanData.e_OnAddMetal != null && flag)
		{
			MetalScanData.e_OnAddMetal();
		}
	}

	public static byte[] Serialize()
	{
		try
		{
			MemoryStream memoryStream = new MemoryStream(200);
			using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
			{
				binaryWriter.Write(m_ActiveIDList.Count);
				for (int i = 0; i < m_ActiveIDList.Count; i++)
				{
					binaryWriter.Write(m_ActiveIDList[i]);
					binaryWriter.Write(m_ScanState[i]);
				}
			}
			return memoryStream.ToArray();
		}
		catch (Exception message)
		{
			Debug.LogWarning(message);
			return null;
		}
	}

	public static bool Deserialize(byte[] buf)
	{
		Clear();
		try
		{
			MemoryStream input = new MemoryStream(buf, writable: false);
			using BinaryReader binaryReader = new BinaryReader(input);
			int num = binaryReader.ReadInt32();
			int num2 = 4 * (num + 1);
			bool flag = buf.Length > num2;
			for (int i = 0; i < num; i++)
			{
				m_ActiveIDList.Add(binaryReader.ReadInt32());
				if (flag)
				{
					m_ScanState.Add(binaryReader.ReadBoolean());
				}
			}
			if (!flag)
			{
				for (int j = 0; j < num; j++)
				{
					m_ScanState.Add(item: true);
				}
			}
			return true;
		}
		catch (Exception message)
		{
			Debug.LogWarning(message);
			return false;
		}
	}
}
