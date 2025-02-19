using System;
using System.Collections.Generic;
using System.IO;
using Mono.Data.SqliteClient;
using UnityEngine;

public class RMRepository
{
	public static Dictionary<int, RandomField> m_RandomFieldMap = new Dictionary<int, RandomField>();

	public static Dictionary<int, MissionCommonData> m_RandMisMap = new Dictionary<int, MissionCommonData>();

	public static MissionCommonData GetRandomMission(int id)
	{
		if (m_RandMisMap.ContainsKey(id))
		{
			return m_RandMisMap[id];
		}
		return null;
	}

	public static bool HasRandomMission(int misid)
	{
		return m_RandMisMap.ContainsKey(misid);
	}

	public static void LoadRandMission()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("RandomQuest_List");
		sqliteDataReader.Read();
		TargetListInfo item = default(TargetListInfo);
		MissionIDNum item2 = default(MissionIDNum);
		MissionIDNum item3 = default(MissionIDNum);
		MissionIDNum item4 = default(MissionIDNum);
		MissionIDNum item5 = default(MissionIDNum);
		MissionIDNum item6 = default(MissionIDNum);
		TargetListInfo item7 = default(TargetListInfo);
		TargetListInfo item8 = default(TargetListInfo);
		TargetListInfo item9 = default(TargetListInfo);
		TargetListInfo item10 = default(TargetListInfo);
		TargetListInfo item11 = default(TargetListInfo);
		TargetListInfo item12 = default(TargetListInfo);
		while (sqliteDataReader.Read())
		{
			MissionCommonData missionCommonData = new MissionCommonData();
			RandomField randomField = new RandomField();
			missionCommonData.m_Type = MissionType.MissionType_Sub;
			missionCommonData.m_ID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ID")));
			int stringId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("MissionName")));
			missionCommonData.m_MissionName = PELocalization.GetString(stringId);
			missionCommonData.m_MaxNum = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("MaxNum")));
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TargetIDList"));
			string[] array = @string.Split(';');
			for (int i = 0; i < array.Length; i++)
			{
				if (!(array[i] == "0"))
				{
					item.listid = new List<int>();
					string[] array2 = array[i].Split(',');
					for (int j = 0; j < array2.Length; j++)
					{
						item.listid.Add(Convert.ToInt32(array2[j]));
					}
					randomField.TargetIDMap.Add(item);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Get_DemandItem"));
			array = @string.Split(',');
			for (int k = 0; k < array.Length; k++)
			{
				if (!(array[k] == "0"))
				{
					string[] array2 = array[k].Split('_');
					if (array2.Length == 2)
					{
						item2.id = Convert.ToInt32(array2[0]);
						item2.num = Convert.ToInt32(array2[1]);
						missionCommonData.m_Get_DemandItem.Add(item2);
					}
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Get_DeleteItem"));
			array = @string.Split(',');
			for (int l = 0; l < array.Length; l++)
			{
				if (!(array[l] == "0"))
				{
					string[] array2 = array[l].Split('_');
					if (array2.Length == 2)
					{
						item3.id = Convert.ToInt32(array2[0]);
						item3.num = Convert.ToInt32(array2[1]);
						missionCommonData.m_Get_DeleteItem.Add(item3);
					}
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Get_MissionItem"));
			array = @string.Split(',');
			for (int m = 0; m < array.Length; m++)
			{
				if (!(array[m] == "0"))
				{
					string[] array2 = array[m].Split('_');
					if (array2.Length == 2)
					{
						item4.id = Convert.ToInt32(array2[0]);
						item4.num = Convert.ToInt32(array2[1]);
						missionCommonData.m_Get_MissionItem.Add(item4);
					}
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("CoRewardItem"));
			array = @string.Split(';');
			for (int n = 0; n < array.Length; n++)
			{
				if (array[n] == "0")
				{
					continue;
				}
				string[] array2 = array[n].Split(',');
				List<MissionIDNum> list = new List<MissionIDNum>();
				for (int num = 0; num < array2.Length; num++)
				{
					string[] array3 = array2[num].Split('_');
					if (array3.Length == 2)
					{
						item5.id = Convert.ToInt32(array3[0]);
						item5.num = Convert.ToInt32(array3[1]);
						list.Add(item5);
					}
				}
				if (list.Count != 0)
				{
					randomField.RewardMap.Add(list);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("CoRemoveItem"));
			array = @string.Split(',');
			for (int num2 = 0; num2 < array.Length; num2++)
			{
				if (!(array[num2] == "0"))
				{
					string[] array2 = array[num2].Split('_');
					if (array2.Length == 2)
					{
						item6.id = Convert.ToInt32(array2[0]);
						item6.num = Convert.ToInt32(array2[1]);
						missionCommonData.m_Com_RemoveItem.Add(item6);
					}
				}
			}
			stringId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Description")));
			missionCommonData.m_Description = PELocalization.GetString(stringId);
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("player_talk"));
			array = @string.Split(',');
			if (array.Length == 2)
			{
				missionCommonData.m_PlayerTalk[0] = Convert.ToInt32(array[0]);
				missionCommonData.m_PlayerTalk[1] = Convert.ToInt32(array[1]);
			}
			else if (array.Length == 1 && array[0] != "0")
			{
				missionCommonData.m_PlayerTalk[0] = Convert.ToInt32(array[0]);
				missionCommonData.m_PlayerTalk[1] = 0;
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TalkOP"));
			array = @string.Split(';');
			for (int num3 = 0; num3 < array.Length; num3++)
			{
				if (!(array[num3] == "0"))
				{
					item7.listid = new List<int>();
					string[] array2 = array[num3].Split(',');
					for (int num4 = 0; num4 < array2.Length; num4++)
					{
						item7.listid.Add(Convert.ToInt32(array2[num4]));
					}
					randomField.TalkOPMap.Add(item7);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TalkIN"));
			array = @string.Split(';');
			for (int num5 = 0; num5 < array.Length; num5++)
			{
				item8.listid = new List<int>();
				string[] array2 = array[num5].Split(',');
				for (int num6 = 0; num6 < array2.Length; num6++)
				{
					item8.listid.Add(Convert.ToInt32(array2[num6]));
				}
				randomField.TalkINMap.Add(item8);
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TalkED"));
			array = @string.Split(';');
			for (int num7 = 0; num7 < array.Length; num7++)
			{
				if (!(array[num7] == "0"))
				{
					item9.listid = new List<int>();
					string[] array2 = array[num7].Split(',');
					for (int num8 = 0; num8 < array2.Length; num8++)
					{
						item9.listid.Add(Convert.ToInt32(array2[num8]));
					}
					randomField.TalkEDMap.Add(item9);
				}
			}
			if (Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("bGiveUp"))) == 0)
			{
				missionCommonData.m_bGiveUp = false;
			}
			else
			{
				missionCommonData.m_bGiveUp = true;
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TalkOP_SP"));
			array = @string.Split(';');
			for (int num9 = 0; num9 < array.Length; num9++)
			{
				if (!(array[num9] == "0"))
				{
					item10.listid = new List<int>();
					string[] array2 = array[num9].Split(',');
					for (int num10 = 0; num10 < array2.Length; num10++)
					{
						item10.listid.Add(Convert.ToInt32(array2[num10]));
					}
					randomField.TalkOPSMap.Add(item10);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TalkIN_SP"));
			array = @string.Split(';');
			for (int num11 = 0; num11 < array.Length; num11++)
			{
				if (!(array[num11] == "0"))
				{
					item11.listid = new List<int>();
					string[] array2 = array[num11].Split(',');
					for (int num12 = 0; num12 < array2.Length; num12++)
					{
						item11.listid.Add(Convert.ToInt32(array2[num12]));
					}
					randomField.TalkINSMap.Add(item11);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TalkED_SP"));
			array = @string.Split(';');
			for (int num13 = 0; num13 < array.Length; num13++)
			{
				if (!(array[num13] == "0"))
				{
					item12.listid = new List<int>();
					string[] array2 = array[num13].Split(',');
					for (int num14 = 0; num14 < array2.Length; num14++)
					{
						item12.listid.Add(Convert.ToInt32(array2[num14]));
					}
					randomField.TalkEDSMap.Add(item12);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("NextQuest"));
			if (@string != "0")
			{
				array = @string.Split(',');
				for (int num15 = 0; num15 < array.Length; num15++)
				{
					missionCommonData.m_EDID.Add(Convert.ToInt32(array[num15]));
				}
			}
			m_RandomFieldMap.Add(missionCommonData.m_ID, randomField);
			m_RandMisMap.Add(missionCommonData.m_ID, missionCommonData);
		}
	}

	public static MissionCommonData CreateRandomMission(int id, ref int idx, ref int rewardIdx)
	{
		MissionCommonData randomMission = GetRandomMission(id);
		if (randomMission == null)
		{
			return null;
		}
		if (!m_RandomFieldMap.ContainsKey(id))
		{
			return null;
		}
		MissionCommonData missionCommonData = new MissionCommonData(randomMission);
		RandomField randomField = m_RandomFieldMap[id];
		if (-1 >= idx || randomField.TargetIDMap.Count <= idx)
		{
			idx = UnityEngine.Random.Range(0, randomField.TargetIDMap.Count);
		}
		TargetListInfo targetListInfo = randomField.TargetIDMap[idx];
		missionCommonData.m_TargetIDList.Clear();
		missionCommonData.m_TalkOP.Clear();
		missionCommonData.m_TalkIN.Clear();
		missionCommonData.m_TalkED.Clear();
		missionCommonData.m_PromptOP.Clear();
		missionCommonData.m_PromptIN.Clear();
		missionCommonData.m_PromptED.Clear();
		missionCommonData.m_Com_RewardItem.Clear();
		missionCommonData.m_Com_RemoveItem.Clear();
		MissionIDNum item = default(MissionIDNum);
		for (int i = 0; i < targetListInfo.listid.Count; i++)
		{
			int num = targetListInfo.listid[i] / 1000;
			if (num == 2)
			{
				TypeCollectData typeCollectData = MissionManager.GetTypeCollectData(targetListInfo.listid[i]);
				if (typeCollectData == null)
				{
					continue;
				}
				item.id = typeCollectData.m_ItemID;
				item.num = typeCollectData.m_ItemNum;
				missionCommonData.m_Com_RemoveItem.Add(item);
			}
			missionCommonData.m_TargetIDList.Add(targetListInfo.listid[i]);
		}
		if (randomField.TalkOPMap.Count > idx)
		{
			targetListInfo = randomField.TalkOPMap[idx];
			for (int j = 0; j < targetListInfo.listid.Count; j++)
			{
				missionCommonData.m_TalkOP.Add(targetListInfo.listid[j]);
			}
		}
		if (randomField.TalkINMap.Count > idx)
		{
			targetListInfo = randomField.TalkINMap[idx];
			for (int k = 0; k < targetListInfo.listid.Count; k++)
			{
				if (targetListInfo.listid[k] != 0)
				{
					missionCommonData.m_TalkIN.Add(targetListInfo.listid[k]);
				}
			}
		}
		if (randomField.TalkEDMap.Count > idx)
		{
			targetListInfo = randomField.TalkEDMap[idx];
			for (int l = 0; l < targetListInfo.listid.Count; l++)
			{
				missionCommonData.m_TalkED.Add(targetListInfo.listid[l]);
			}
		}
		if (randomField.TalkOPSMap.Count > idx)
		{
			targetListInfo = randomField.TalkOPSMap[idx];
			for (int m = 0; m < targetListInfo.listid.Count; m++)
			{
				missionCommonData.m_PromptOP.Add(targetListInfo.listid[m]);
			}
		}
		if (randomField.TalkINSMap.Count > idx)
		{
			targetListInfo = randomField.TalkINSMap[idx];
			for (int n = 0; n < targetListInfo.listid.Count; n++)
			{
				missionCommonData.m_PromptIN.Add(targetListInfo.listid[n]);
			}
		}
		if (randomField.TalkEDSMap.Count > idx)
		{
			targetListInfo = randomField.TalkEDSMap[idx];
			for (int num2 = 0; num2 < targetListInfo.listid.Count; num2++)
			{
				missionCommonData.m_PromptED.Add(targetListInfo.listid[num2]);
			}
		}
		if (randomField.RewardMap.Count == 0)
		{
			return null;
		}
		rewardIdx = UnityEngine.Random.Range(0, randomField.RewardMap.Count);
		for (int num3 = 0; num3 < randomField.RewardMap[rewardIdx].Count; num3++)
		{
			MissionIDNum item2 = randomField.RewardMap[rewardIdx][num3];
			missionCommonData.m_Com_RewardItem.Add(item2);
		}
		foreach (MissionIDNum item3 in randomField.FixedRewardMap)
		{
			missionCommonData.m_Com_RewardItem.Add(item3);
		}
		return missionCommonData;
	}

	public static byte[] Export(PlayerMission pm, int playerId = 0)
	{
		if (pm == null)
		{
			return null;
		}
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		List<int> list = new List<int>();
		foreach (KeyValuePair<int, Dictionary<string, string>> item in pm.m_MissionInfo)
		{
			if (m_RandMisMap.ContainsKey(item.Key))
			{
				list.Add(item.Key);
			}
		}
		binaryWriter.Write(list.Count);
		for (int i = 0; i < list.Count; i++)
		{
			MissionCommonData adrmMissionCommonData = MissionManager.Manager.GetAdrmMissionCommonData(playerId, list[i]);
			if (adrmMissionCommonData != null)
			{
				binaryWriter.Write(adrmMissionCommonData.m_ID);
				binaryWriter.Write(adrmMissionCommonData.m_MissionName);
				binaryWriter.Write(adrmMissionCommonData.m_iNpc);
				binaryWriter.Write(adrmMissionCommonData.m_iReplyNpc);
				binaryWriter.Write((int)adrmMissionCommonData.m_Type);
				binaryWriter.Write(adrmMissionCommonData.m_MaxNum);
				binaryWriter.Write(adrmMissionCommonData.m_Description);
				binaryWriter.Write(adrmMissionCommonData.m_TargetIDList.Count);
				for (int j = 0; j < adrmMissionCommonData.m_TargetIDList.Count; j++)
				{
					binaryWriter.Write(adrmMissionCommonData.m_TargetIDList[j]);
				}
				binaryWriter.Write(adrmMissionCommonData.m_PlayerTalk.Length);
				for (int k = 0; k < adrmMissionCommonData.m_PlayerTalk.Length; k++)
				{
					binaryWriter.Write(adrmMissionCommonData.m_PlayerTalk[k]);
				}
				binaryWriter.Write(adrmMissionCommonData.m_Get_DemandItem.Count);
				for (int l = 0; l < adrmMissionCommonData.m_Get_DemandItem.Count; l++)
				{
					binaryWriter.Write(adrmMissionCommonData.m_Get_DemandItem[l].id);
					binaryWriter.Write(adrmMissionCommonData.m_Get_DemandItem[l].num);
				}
				binaryWriter.Write(adrmMissionCommonData.m_Get_DeleteItem.Count);
				for (int m = 0; m < adrmMissionCommonData.m_Get_DeleteItem.Count; m++)
				{
					binaryWriter.Write(adrmMissionCommonData.m_Get_DeleteItem[m].id);
					binaryWriter.Write(adrmMissionCommonData.m_Get_DeleteItem[m].num);
				}
				binaryWriter.Write(adrmMissionCommonData.m_Get_MissionItem.Count);
				for (int n = 0; n < adrmMissionCommonData.m_Get_MissionItem.Count; n++)
				{
					binaryWriter.Write(adrmMissionCommonData.m_Get_MissionItem[n].id);
					binaryWriter.Write(adrmMissionCommonData.m_Get_MissionItem[n].num);
				}
				binaryWriter.Write(adrmMissionCommonData.m_Com_RewardItem.Count);
				for (int num = 0; num < adrmMissionCommonData.m_Com_RewardItem.Count; num++)
				{
					binaryWriter.Write(adrmMissionCommonData.m_Com_RewardItem[num].id);
					binaryWriter.Write(adrmMissionCommonData.m_Com_RewardItem[num].num);
				}
				binaryWriter.Write(adrmMissionCommonData.m_Com_SelRewardItem.Count);
				for (int num2 = 0; num2 < adrmMissionCommonData.m_Com_SelRewardItem.Count; num2++)
				{
					binaryWriter.Write(adrmMissionCommonData.m_Com_SelRewardItem[num2].id);
					binaryWriter.Write(adrmMissionCommonData.m_Com_SelRewardItem[num2].num);
				}
				binaryWriter.Write(adrmMissionCommonData.m_Com_RemoveItem.Count);
				for (int num3 = 0; num3 < adrmMissionCommonData.m_Com_RemoveItem.Count; num3++)
				{
					binaryWriter.Write(adrmMissionCommonData.m_Com_RemoveItem[num3].id);
					binaryWriter.Write(adrmMissionCommonData.m_Com_RemoveItem[num3].num);
				}
				binaryWriter.Write(adrmMissionCommonData.m_TalkOP.Count);
				for (int num4 = 0; num4 < adrmMissionCommonData.m_TalkOP.Count; num4++)
				{
					binaryWriter.Write(adrmMissionCommonData.m_TalkOP[num4]);
				}
				binaryWriter.Write(adrmMissionCommonData.m_OPID.Count);
				for (int num5 = 0; num5 < adrmMissionCommonData.m_OPID.Count; num5++)
				{
					binaryWriter.Write(adrmMissionCommonData.m_OPID[num5]);
				}
				binaryWriter.Write(adrmMissionCommonData.m_TalkIN.Count);
				for (int num6 = 0; num6 < adrmMissionCommonData.m_TalkIN.Count; num6++)
				{
					binaryWriter.Write(adrmMissionCommonData.m_TalkIN[num6]);
				}
				binaryWriter.Write(adrmMissionCommonData.m_INID.Count);
				for (int num7 = 0; num7 < adrmMissionCommonData.m_INID.Count; num7++)
				{
					binaryWriter.Write(adrmMissionCommonData.m_INID[num7]);
				}
				binaryWriter.Write(adrmMissionCommonData.m_TalkED.Count);
				for (int num8 = 0; num8 < adrmMissionCommonData.m_TalkED.Count; num8++)
				{
					binaryWriter.Write(adrmMissionCommonData.m_TalkED[num8]);
				}
				binaryWriter.Write(adrmMissionCommonData.m_EDID.Count);
				for (int num9 = 0; num9 < adrmMissionCommonData.m_EDID.Count; num9++)
				{
					binaryWriter.Write(adrmMissionCommonData.m_EDID[num9]);
				}
			}
		}
		binaryWriter.Close();
		memoryStream.Close();
		return memoryStream.ToArray();
	}

	public static void Import(byte[] buffer)
	{
		if (buffer.Length == 0)
		{
			return;
		}
		MemoryStream memoryStream = new MemoryStream(buffer);
		BinaryReader binaryReader = new BinaryReader(memoryStream);
		int num = binaryReader.ReadInt32();
		int num2 = 0;
		MissionIDNum item = default(MissionIDNum);
		for (int i = 0; i < num; i++)
		{
			int num3 = binaryReader.ReadInt32();
			if (!m_RandMisMap.ContainsKey(num3))
			{
				return;
			}
			MissionCommonData missionCommonData = m_RandMisMap[num3];
			if (missionCommonData != null)
			{
				missionCommonData.m_TargetIDList.Clear();
				missionCommonData.m_TalkOP.Clear();
				missionCommonData.m_TalkIN.Clear();
				missionCommonData.m_TalkED.Clear();
				missionCommonData.m_PromptOP.Clear();
				missionCommonData.m_PromptIN.Clear();
				missionCommonData.m_PromptED.Clear();
				missionCommonData.m_Com_RewardItem.Clear();
				missionCommonData.m_Com_RemoveItem.Clear();
				missionCommonData.m_MissionName = binaryReader.ReadString();
				missionCommonData.m_iNpc = binaryReader.ReadInt32();
				missionCommonData.m_iReplyNpc = binaryReader.ReadInt32();
				NpcMissionDataRepository.AddReplyMission(missionCommonData.m_iReplyNpc, num3);
				missionCommonData.m_Type = (MissionType)binaryReader.ReadInt32();
				missionCommonData.m_MaxNum = binaryReader.ReadInt32();
				missionCommonData.m_Description = binaryReader.ReadString();
				num2 = binaryReader.ReadInt32();
				for (int j = 0; j < num2; j++)
				{
					missionCommonData.m_TargetIDList.Add(Convert.ToInt32(binaryReader.ReadInt32()));
				}
				num2 = binaryReader.ReadInt32();
				for (int k = 0; k < num2; k++)
				{
					missionCommonData.m_PlayerTalk[k] = Convert.ToInt32(binaryReader.ReadInt32());
				}
				num2 = binaryReader.ReadInt32();
				for (int l = 0; l < num2; l++)
				{
					item.id = Convert.ToInt32(binaryReader.ReadInt32());
					item.num = Convert.ToInt32(binaryReader.ReadInt32());
					missionCommonData.m_Get_DemandItem.Add(item);
				}
				num2 = binaryReader.ReadInt32();
				for (int m = 0; m < num2; m++)
				{
					item.id = Convert.ToInt32(binaryReader.ReadInt32());
					item.num = Convert.ToInt32(binaryReader.ReadInt32());
					missionCommonData.m_Get_DeleteItem.Add(item);
				}
				num2 = binaryReader.ReadInt32();
				for (int n = 0; n < num2; n++)
				{
					item.id = Convert.ToInt32(binaryReader.ReadInt32());
					item.num = Convert.ToInt32(binaryReader.ReadInt32());
					missionCommonData.m_Get_MissionItem.Add(item);
				}
				num2 = binaryReader.ReadInt32();
				for (int num4 = 0; num4 < num2; num4++)
				{
					item.id = Convert.ToInt32(binaryReader.ReadInt32());
					item.num = Convert.ToInt32(binaryReader.ReadInt32());
					missionCommonData.m_Com_RewardItem.Add(item);
				}
				num2 = binaryReader.ReadInt32();
				for (int num5 = 0; num5 < num2; num5++)
				{
					item.id = Convert.ToInt32(binaryReader.ReadInt32());
					item.num = Convert.ToInt32(binaryReader.ReadInt32());
					missionCommonData.m_Com_SelRewardItem.Add(item);
				}
				num2 = binaryReader.ReadInt32();
				for (int num6 = 0; num6 < num2; num6++)
				{
					item.id = Convert.ToInt32(binaryReader.ReadInt32());
					item.num = Convert.ToInt32(binaryReader.ReadInt32());
					missionCommonData.m_Com_RemoveItem.Add(item);
				}
				num2 = binaryReader.ReadInt32();
				for (int num7 = 0; num7 < num2; num7++)
				{
					missionCommonData.m_TalkOP.Add(Convert.ToInt32(binaryReader.ReadInt32()));
				}
				num2 = binaryReader.ReadInt32();
				for (int num8 = 0; num8 < num2; num8++)
				{
					missionCommonData.m_OPID.Add(Convert.ToInt32(binaryReader.ReadInt32()));
				}
				num2 = binaryReader.ReadInt32();
				for (int num9 = 0; num9 < num2; num9++)
				{
					missionCommonData.m_TalkIN.Add(Convert.ToInt32(binaryReader.ReadInt32()));
				}
				num2 = binaryReader.ReadInt32();
				for (int num10 = 0; num10 < num2; num10++)
				{
					missionCommonData.m_INID.Add(Convert.ToInt32(binaryReader.ReadInt32()));
				}
				num2 = binaryReader.ReadInt32();
				for (int num11 = 0; num11 < num2; num11++)
				{
					missionCommonData.m_TalkED.Add(Convert.ToInt32(binaryReader.ReadInt32()));
				}
				num2 = binaryReader.ReadInt32();
				for (int num12 = 0; num12 < num2; num12++)
				{
					missionCommonData.m_EDID.Add(Convert.ToInt32(binaryReader.ReadInt32()));
				}
			}
		}
		binaryReader.Close();
		memoryStream.Close();
	}
}
