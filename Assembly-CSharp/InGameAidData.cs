using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Data.SqliteClient;
using Pathea;
using UnityEngine;

public class InGameAidData
{
	public enum InGameAidType
	{
		None,
		JoinMission,
		CompleteTask,
		PutOnEquip,
		OpenUI,
		ClickBtn,
		InBuff,
		UseItem,
		ShowInGameAid,
		GetItem,
		GetServant,
		PlayTalk
	}

	public const int Version = 1608210;

	public static Dictionary<int, InGameAidData> AllData = new Dictionary<int, InGameAidData>();

	public static Dictionary<InGameAidType, List<int>> AllStartTypeMap = new Dictionary<InGameAidType, List<int>>();

	public static Dictionary<InGameAidType, List<int>> AllShowTypeMap = new Dictionary<InGameAidType, List<int>>();

	public static Dictionary<InGameAidType, List<int>> AllCompleteTypeMap = new Dictionary<InGameAidType, List<int>>();

	public static bool ShowInGameAidCtrl = true;

	public static List<int> CurCheckShowIDs = new List<int>();

	public static List<int> CurShowIDs = new List<int>();

	public static List<int> CompleteIDs = new List<int>();

	public int ID { get; private set; }

	public InGameAidType StartType { get; private set; }

	public List<int> StartValue { get; private set; }

	public InGameAidType ShowType { get; private set; }

	public List<int> ShowValue { get; private set; }

	public InGameAidType ComplateType { get; private set; }

	public List<int> ComplateValue { get; private set; }

	public int CountentID { get; private set; }

	public static event Action<int> AddEvent;

	public InGameAidData(int id, InGameAidType startType, List<int> startValue, InGameAidType showType, List<int> showValue, InGameAidType complateType, List<int> complateValue, int contentID)
	{
		ID = id;
		StartType = startType;
		StartValue = startValue;
		ShowType = showType;
		ShowValue = showValue;
		ComplateType = complateType;
		ComplateValue = complateValue;
		CountentID = contentID;
	}

	private static void AddCompleteID(int completeID)
	{
		if (AllData != null && AllData.Count > 0 && !CompleteIDs.Contains(completeID))
		{
			CompleteIDs.Add(completeID);
			if (CurCheckShowIDs.Contains(completeID))
			{
				CurCheckShowIDs.Remove(completeID);
			}
		}
	}

	private static void AddCurCheckShowList(int id)
	{
		if (!CurCheckShowIDs.Contains(id))
		{
			CurCheckShowIDs.Add(id);
		}
	}

	private static void AddShowID(int id)
	{
		if (!CurShowIDs.Contains(id) && !CompleteIDs.Contains(id))
		{
			CurCheckShowIDs.Remove(id);
			CurShowIDs.Add(id);
			if (InGameAidData.AddEvent != null)
			{
				InGameAidData.AddEvent(id);
			}
			CheckShowInGameAid(id);
			if (AllCompleteTypeMap.ContainsKey(InGameAidType.None) && AllCompleteTypeMap[InGameAidType.None].Contains(id))
			{
				AddCompleteID(id);
			}
		}
	}

	private static void LoadAllMap()
	{
		AllStartTypeMap.Clear();
		AllShowTypeMap.Clear();
		AllCompleteTypeMap.Clear();
		InGameAidData inGameAidData = null;
		foreach (KeyValuePair<int, InGameAidData> allDatum in AllData)
		{
			inGameAidData = allDatum.Value;
			if (!AllStartTypeMap.ContainsKey(inGameAidData.StartType))
			{
				AllStartTypeMap.Add(inGameAidData.StartType, new List<int>());
			}
			AllStartTypeMap[inGameAidData.StartType].Add(inGameAidData.ID);
			if (inGameAidData.ShowType != 0)
			{
				if (!AllShowTypeMap.ContainsKey(inGameAidData.ShowType))
				{
					AllShowTypeMap.Add(inGameAidData.ShowType, new List<int>());
				}
				AllShowTypeMap[inGameAidData.ShowType].Add(inGameAidData.ID);
			}
			if (!AllCompleteTypeMap.ContainsKey(inGameAidData.ComplateType))
			{
				AllCompleteTypeMap.Add(inGameAidData.ComplateType, new List<int>());
			}
			AllCompleteTypeMap[inGameAidData.ComplateType].Add(inGameAidData.ID);
		}
		LoadNoneStartType();
	}

	public static void RemoveCompleteIDs()
	{
		if (CompleteIDs == null || CompleteIDs.Count <= 0)
		{
			return;
		}
		foreach (int value in Enum.GetValues(typeof(InGameAidType)))
		{
			for (int i = 0; i < CompleteIDs.Count; i++)
			{
				int item = CompleteIDs[i];
				if (AllStartTypeMap.ContainsKey((InGameAidType)value) && !AllStartTypeMap[(InGameAidType)value].Contains(item))
				{
					AllStartTypeMap[(InGameAidType)value].Add(item);
				}
				if (AllShowTypeMap.ContainsKey((InGameAidType)value) && !AllShowTypeMap[(InGameAidType)value].Contains(item))
				{
					AllShowTypeMap[(InGameAidType)value].Add(item);
				}
				if (AllCompleteTypeMap.ContainsKey((InGameAidType)value) && !AllCompleteTypeMap[(InGameAidType)value].Contains(item))
				{
					AllCompleteTypeMap[(InGameAidType)value].Add(item);
				}
			}
		}
	}

	private static void LoadNoneStartType()
	{
		if (!AllStartTypeMap.ContainsKey(InGameAidType.None) || AllStartTypeMap[InGameAidType.None].Count <= 0)
		{
			return;
		}
		List<int> list = AllStartTypeMap[InGameAidType.None];
		for (int i = 0; i < list.Count; i++)
		{
			int item = list[i];
			if (!CompleteIDs.Contains(item) && !CurCheckShowIDs.Contains(item))
			{
				CurCheckShowIDs.Add(item);
			}
		}
		AllStartTypeMap.Remove(InGameAidType.None);
	}

	public static void LoadData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("IngameAid");
		InGameAidData inGameAidData = null;
		while (sqliteDataReader.Read())
		{
			int num = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ID")));
			int startType = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("S_Type")));
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("S_Value"));
			int showType = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Type")));
			string string2 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Value"));
			int complateType = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("C_Type")));
			string string3 = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("C_Value"));
			int contentID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Content")));
			string[] array = @string.Split(',');
			int[] array2 = new int[array.Length];
			for (int i = 0; i < array2.Length; i++)
			{
				int result = 0;
				if (int.TryParse(array[i], out result))
				{
					array2[i] = result;
					continue;
				}
				Debug.LogError("Field [S_Value] has errory! and ID is " + num + " by table [IngameAid]");
				return;
			}
			string[] array3 = string2.Split(',');
			int[] array4 = new int[array3.Length];
			for (int j = 0; j < array4.Length; j++)
			{
				int result2 = 0;
				if (int.TryParse(array3[j], out result2))
				{
					array4[j] = result2;
					continue;
				}
				Debug.LogError("Field [Value] has errory! and ID is " + num + " by table [IngameAid]");
				return;
			}
			string[] array5 = string3.Split(',');
			int[] array6 = new int[array5.Length];
			for (int k = 0; k < array6.Length; k++)
			{
				int result3 = 0;
				if (int.TryParse(array5[k], out result3))
				{
					array6[k] = result3;
					continue;
				}
				Debug.LogError("Field [C_Value] has errory! and ID is " + num + " by table [IngameAid]");
				return;
			}
			inGameAidData = new InGameAidData(num, (InGameAidType)startType, array2.ToList(), (InGameAidType)showType, array4.ToList(), (InGameAidType)complateType, array6.ToList(), contentID);
			AllData.Add(num, inGameAidData);
		}
	}

	public static bool Deserialize(byte[] data)
	{
		Clear();
		try
		{
			MemoryStream input = new MemoryStream(data, writable: false);
			using (BinaryReader binaryReader = new BinaryReader(input))
			{
				int num = binaryReader.ReadInt32();
				if (num != 1608210 && PeGameMgr.IsSingle)
				{
					Debug.LogWarning($"InGameAidData version valid! CurVersion:{1608210} ErrorVersion:{num}");
					return false;
				}
				ShowInGameAidCtrl = binaryReader.ReadBoolean();
				int num2 = binaryReader.ReadInt32();
				for (int i = 0; i < num2; i++)
				{
					CurCheckShowIDs.Add(binaryReader.ReadInt32());
				}
				num2 = binaryReader.ReadInt32();
				for (int j = 0; j < num2; j++)
				{
					CurShowIDs.Add(binaryReader.ReadInt32());
				}
				num2 = binaryReader.ReadInt32();
				for (int k = 0; k < num2; k++)
				{
					CompleteIDs.Add(binaryReader.ReadInt32());
				}
			}
			LoadAllMap();
			RemoveCompleteIDs();
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogWarning("InGameAidData deserialize error: " + ex);
			return false;
		}
	}

	public static byte[] Serialize()
	{
		try
		{
			MemoryStream memoryStream = new MemoryStream(200);
			using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
			{
				binaryWriter.Write(1608210);
				binaryWriter.Write(ShowInGameAidCtrl);
				binaryWriter.Write(CurCheckShowIDs.Count);
				for (int i = 0; i < CurCheckShowIDs.Count; i++)
				{
					binaryWriter.Write(CurCheckShowIDs[i]);
				}
				binaryWriter.Write(CurShowIDs.Count);
				for (int j = 0; j < CurShowIDs.Count; j++)
				{
					binaryWriter.Write(CurShowIDs[j]);
				}
				binaryWriter.Write(CompleteIDs.Count);
				for (int k = 0; k < CompleteIDs.Count; k++)
				{
					binaryWriter.Write(CompleteIDs[k]);
				}
			}
			return memoryStream.ToArray();
		}
		catch (Exception ex)
		{
			Debug.LogWarning("InGameAidData serialize error: " + ex);
			return null;
		}
	}

	public static void Clear()
	{
		CurCheckShowIDs.Clear();
		CurShowIDs.Clear();
		CompleteIDs.Clear();
		LoadAllMap();
	}

	public static void CheckJoinMission(int missionID)
	{
		CheckValue(InGameAidType.JoinMission, missionID);
	}

	public static void CheckCompleteTask(int missionID)
	{
		CheckValue(InGameAidType.CompleteTask, missionID);
	}

	public static void CheckPutOnEquip(int equipID)
	{
		CheckValue(InGameAidType.PutOnEquip, equipID);
	}

	public static void CheckOpenUI(UIEnum.WndType type)
	{
		CheckValue(InGameAidType.OpenUI, (int)type);
	}

	public static void CheckClickBtn(int btnID)
	{
		CheckValue(InGameAidType.ClickBtn, btnID);
	}

	public static void CheckInBuff(int buffID)
	{
		CheckValue(InGameAidType.InBuff, buffID);
	}

	public static void CheckUseItem(int itemID)
	{
		CheckValue(InGameAidType.UseItem, itemID);
	}

	public static void CheckShowInGameAid(int igaID)
	{
		CheckValue(InGameAidType.ShowInGameAid, igaID);
	}

	public static void CheckGetItem(int itemID)
	{
		CheckValue(InGameAidType.GetItem, itemID);
	}

	public static void CheckGetServant(int servantID)
	{
		CheckValue(InGameAidType.GetServant, servantID);
	}

	public static void CheckNpcTalk(int talkID)
	{
		CheckValue(InGameAidType.PlayTalk, talkID);
	}

	public static void CheckValue(InGameAidType type, int value)
	{
		if (PeGameMgr.sceneMode != 0)
		{
			return;
		}
		if (AllCompleteTypeMap.ContainsKey(type))
		{
			List<int> list = AllCompleteTypeMap[type];
			if (list == null || list.Count <= 0)
			{
				AllCompleteTypeMap.Remove(type);
			}
			else
			{
				List<int> list2 = new List<int>();
				for (int i = 0; i < list.Count; i++)
				{
					int num = list[i];
					list2 = AllData[num].ComplateValue;
					if ((list2.Count == 1 && list2[0] == -1) || list2.Contains(value))
					{
						AddCompleteID(num);
						list.RemoveAt(i);
						i--;
						if (AllStartTypeMap.ContainsKey(type) && AllStartTypeMap[type].Contains(num))
						{
							AllStartTypeMap[type].Remove(num);
						}
						if (AllShowTypeMap.ContainsKey(type) && AllShowTypeMap[type].Contains(num))
						{
							AllShowTypeMap[type].Remove(num);
						}
					}
				}
			}
		}
		if (AllStartTypeMap.ContainsKey(type))
		{
			List<int> list3 = AllStartTypeMap[type];
			if (list3 == null || list3.Count <= 0)
			{
				AllStartTypeMap.Remove(type);
			}
			else
			{
				List<int> list4 = new List<int>();
				for (int j = 0; j < list3.Count; j++)
				{
					int num2 = list3[j];
					list4 = AllData[num2].StartValue;
					if ((list4.Count == 1 && list4[0] == -1) || list4.Contains(value))
					{
						AddCurCheckShowList(num2);
						list3.RemoveAt(j);
						j--;
					}
				}
			}
		}
		if (!AllShowTypeMap.ContainsKey(type))
		{
			return;
		}
		List<int> list5 = AllShowTypeMap[type];
		if (list5 == null || list5.Count <= 0)
		{
			AllShowTypeMap.Remove(type);
			return;
		}
		List<int> list6 = new List<int>();
		for (int k = 0; k < list5.Count; k++)
		{
			int num3 = list5[k];
			list6 = AllData[num3].ShowValue;
			if (((list6.Count == 1 && list6[0] == -1) || list6.Contains(value)) && CurCheckShowIDs.Contains(num3))
			{
				AddShowID(num3);
				list5.RemoveAt(k);
				k--;
			}
		}
	}
}
