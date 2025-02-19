using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using UnityEngine;

public class MisInitRepository
{
	public static Dictionary<int, MissionInit> m_MisInitMap = new Dictionary<int, MissionInit>();

	public static void LoadData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("Initialization");
		sqliteDataReader.Read();
		MonsterSetInfo item4 = default(MonsterSetInfo);
		MonsterSetInfo item5 = default(MonsterSetInfo);
		SetDoodadEffect item6 = default(SetDoodadEffect);
		ENpcBattleInfo item8 = default(ENpcBattleInfo);
		KillMons item9 = default(KillMons);
		NpcType item10 = default(NpcType);
		while (sqliteDataReader.Read())
		{
			MissionInit missionInit = new MissionInit();
			missionInit.m_ID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ID")));
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Finished"));
			string[] array = @string.Split(';');
			if (@string != "0")
			{
				for (int i = 0; i < array.Length; i++)
				{
					string[] array2 = array[i].Split('_');
					for (int j = 0; j < array2.Length; j++)
					{
						missionInit.m_ComMisID.Add(Convert.ToInt32(array2[j]));
					}
				}
			}
			missionInit.m_NComMisID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("UnFinished")));
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("PlotID"));
			if (!@string.Equals("0"))
			{
				array = @string.Split(',');
				string[] array3 = array;
				foreach (string value in array3)
				{
					missionInit.m_triggerPlot.Add(Convert.ToInt32(value));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("NPCact"));
			array = @string.Split(',');
			for (int l = 0; l < array.Length; l++)
			{
				string[] array2 = array[l].Split('_');
				if (array2.Length == 4)
				{
					NpcAct item = default(NpcAct);
					item.npcid = Convert.ToInt32(array2[0]);
					item.animation = array2[1];
					item.btrue = Convert.ToInt32(array2[2]) == 1;
					item.bFloat = Convert.ToSingle(array2[3]);
					missionInit.m_NpcAct.Add(item);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("NPCface"));
			array = @string.Split(',');
			for (int m = 0; m < array.Length; m++)
			{
				string[] array2 = array[m].Split('_');
				if (array2.Length >= 2)
				{
					NpcFace npcFace = new NpcFace();
					npcFace.npcid = Convert.ToInt32(array2[0]);
					if (int.TryParse(array2[1], out var _))
					{
						npcFace.angle = Convert.ToInt32(array2[1]);
					}
					else
					{
						npcFace.npcother = array2[1];
					}
					if (array2.Length == 3)
					{
						npcFace.bmove = Convert.ToInt32(array2[2]) == 1;
					}
					missionInit.m_NpcFace.Add(npcFace);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("NPCcamp"));
			array = @string.Split(',');
			for (int n = 0; n < array.Length; n++)
			{
				string[] array2 = array[n].Split('_');
				if (array2.Length == 2)
				{
					NpcCamp item2 = default(NpcCamp);
					item2.npcid = Convert.ToInt32(array2[0]);
					item2.camp = Convert.ToInt32(array2[1]);
					missionInit.m_NpcCamp.Add(item2);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("DelMonster"));
			array = @string.Split(',');
			for (int num = 0; num < array.Length; num++)
			{
				int num2 = Convert.ToInt32(array[num]);
				if (num2 != 0)
				{
					missionInit.m_DeleteMonster.Add(num2);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("DelNPC"));
			if (@string != "0")
			{
				array = @string.Split(',');
				for (int num3 = 0; num3 < array.Length; num3++)
				{
					missionInit.m_iDeleteNpc.Add(Convert.ToInt32(array[num3]));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TransNPC"));
			array = @string.Split(';');
			for (int num4 = 0; num4 < array.Length; num4++)
			{
				string[] array2 = array[num4].Split('_');
				if (array2.Length == 2)
				{
					NpcStyle npcStyle = new NpcStyle();
					npcStyle.npcid = Convert.ToInt32(array2[0]);
					string[] array4 = array2[1].Split(',');
					if (array4.Length == 3)
					{
						float x = Convert.ToSingle(array4[0]);
						float y = Convert.ToSingle(array4[1]);
						float z = Convert.ToSingle(array4[2]);
						npcStyle.pos = new Vector3(x, y, z);
					}
					missionInit.m_TransNpc.Add(npcStyle);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("FolPlayer"));
			array = @string.Split(',');
			for (int num5 = 0; num5 < array.Length; num5++)
			{
				string[] array2 = array[num5].Split('_');
				if (array2.Length == 2)
				{
					NpcOpen item3 = default(NpcOpen);
					item3.npcid = Convert.ToInt32(array2[0]);
					item3.bopen = Convert.ToInt32(array2[1]) == 1;
					missionInit.m_FollowPlayerList.Add(item3);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Monstercamp"));
			array = @string.Split(',');
			for (int num6 = 0; num6 < array.Length; num6++)
			{
				string[] array2 = array[num6].Split('_');
				if (array2.Length == 2)
				{
					item4.id = Convert.ToInt32(array2[0]);
					item4.value = Convert.ToInt32(array2[1]);
					if (item4.id != 0)
					{
						missionInit.m_MonsterCampList.Add(item4);
					}
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Monsterharm"));
			array = @string.Split(',');
			for (int num7 = 0; num7 < array.Length; num7++)
			{
				string[] array2 = array[num7].Split('_');
				if (array2.Length == 2)
				{
					item5.id = Convert.ToInt32(array2[0]);
					item5.value = Convert.ToInt32(array2[1]);
					if (item5.id != 0)
					{
						missionInit.m_MonsterHarmList.Add(item5);
					}
				}
			}
			missionInit.m_Special = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Special"));
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ColonyNoOrder"));
			array = @string.Split(',');
			for (int num8 = 0; num8 < array.Length; num8++)
			{
				if (!(array[num8] == "0"))
				{
					missionInit.m_iColonyNoOrderNpcList.Add(Convert.ToInt32(array[num8]));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("MonsterHatred"));
			if (@string != "0")
			{
				array = @string.Split('_', ';');
				for (int num9 = 0; num9 < array.Length; num9++)
				{
					missionInit.m_monsterHatredList.Add(Convert.ToInt32(array[num9]));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("NPCHatred"));
			if (@string != "0")
			{
				array = @string.Split('_', ';');
				for (int num10 = 0; num10 < array.Length; num10++)
				{
					missionInit.m_npcHatredList.Add(Convert.ToInt32(array[num10]));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Harm"));
			if (@string != "0")
			{
				array = @string.Split('_', ';');
				for (int num11 = 0; num11 < array.Length; num11++)
				{
					missionInit.m_harmList.Add(Convert.ToInt32(array[num11]));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("DoodadHarm"));
			if (@string != "0")
			{
				array = @string.Split('_', ';');
				if (array.Length == 3)
				{
					string[] array5 = array;
					foreach (string value2 in array5)
					{
						missionInit.m_doodadHarmList.Add(Convert.ToInt32(value2));
					}
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("DoodadEffect"));
			if (@string != "0")
			{
				array = @string.Split(';');
				string[] array6 = array;
				foreach (string text in array6)
				{
					string[] array2 = text.Split(':');
					if (array2.Length == 3)
					{
						item6.id = Convert.ToInt32(array2[0]);
						item6.names = new List<string>();
						string[] array4 = array2[1].Split(',');
						string[] array7 = array4;
						foreach (string item7 in array7)
						{
							item6.names.Add(item7);
						}
						item6.openOrClose = Convert.ToInt32(array2[2]) == 1;
						missionInit.m_doodadEffectList.Add(item6);
					}
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ENpcBattle"));
			if (@string != "0")
			{
				array = @string.Split(';');
				string[] array8 = array;
				foreach (string text2 in array8)
				{
					string[] array2 = text2.Split(',', '_');
					if (array2.Length >= 2)
					{
						item8.npcId = new List<int>();
						for (int num16 = 0; num16 < array2.Length - 1; num16++)
						{
							item8.npcId.Add(Convert.ToInt32(array2[num16]));
						}
						item8.type = Convert.ToInt32(array2[array2.Length - 1]);
						missionInit.m_npcsBattle.Add(item8);
					}
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Area"));
			if (@string != "0")
			{
				array = @string.Split('_');
				for (int num17 = 0; num17 < array.Length; num17++)
				{
					missionInit.m_plotMissionTrigger.Add(array[num17]);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("CantRevive"));
			array = @string.Split(',');
			for (int num18 = 0; num18 < array.Length; num18++)
			{
				if (Convert.ToInt32(array[num18]) != 0)
				{
					missionInit.cantReviveNpc.Add(Convert.ToInt32(array[num18]));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("KillMons"));
			array = @string.Split(';');
			string[] array9 = array;
			foreach (string text3 in array9)
			{
				string[] array2 = text3.Split('_');
				if (array2.Length == 5)
				{
					item9.id = Convert.ToInt32(array2[0]);
					string[] array4 = array2[1].Split(',');
					if (array4.Length == 3)
					{
						float x = Convert.ToSingle(array4[0]);
						float y = Convert.ToSingle(array4[1]);
						float z = Convert.ToSingle(array4[2]);
						item9.center = new Vector3(x, y, z);
						item9.radius = Convert.ToSingle(array2[2]);
						item9.type = KillMons.Type.protoTypeId;
					}
					else
					{
						item9.center = Vector3.zero;
						item9.radius = 0f;
						item9.type = KillMons.Type.fixedId;
					}
					item9.monId = Convert.ToInt32(array2[3]);
					item9.reviveTime = Convert.ToInt32(array2[4]);
					missionInit.m_killMons.Add(item9);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("NPCType"));
			array = @string.Split(';');
			string[] array10 = array;
			foreach (string text4 in array10)
			{
				if (text4.Equals("0"))
				{
					continue;
				}
				item10.npcs = new List<int>();
				item10.type = -1;
				string[] array11 = text4.Split('_');
				for (int num21 = 0; num21 < array11.Length; num21++)
				{
					if (num21 < array11.Length - 1)
					{
						item10.npcs.Add(Convert.ToInt32(array11[num21]));
					}
					else
					{
						item10.type = Convert.ToInt32(array11[num21]);
					}
				}
				missionInit.m_npcType.Add(item10);
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("KillNPC"));
			if (@string != "0")
			{
				array = @string.Split('_', ';');
				for (int num22 = 0; num22 < array.Length; num22++)
				{
					missionInit.m_killNpcList.Add(Convert.ToInt32(array[num22]));
				}
			}
			m_MisInitMap.Add(missionInit.m_ID, missionInit);
		}
	}
}
