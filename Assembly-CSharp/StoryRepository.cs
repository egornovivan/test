using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using Pathea;
using PETools;
using UnityEngine;

public class StoryRepository
{
	public static Dictionary<int, StoryData> m_StoryRespository = new Dictionary<int, StoryData>();

	public static Dictionary<int, AdStoryData> m_AdStoryRespository = new Dictionary<int, AdStoryData>();

	public static StoryData GetStroyData(int id)
	{
		if (!m_StoryRespository.ContainsKey(id))
		{
			return null;
		}
		return m_StoryRespository[id];
	}

	public static void LoadData_story()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("Plot");
		sqliteDataReader.Read();
		RandScenariosData randScenarios = default(RandScenariosData);
		MissionIDNum item = default(MissionIDNum);
		MotionStyle item6 = default(MotionStyle);
		SenceFace item7 = default(SenceFace);
		SenceAct item8 = default(SenceAct);
		MonsterSetInfo item13 = default(MonsterSetInfo);
		MonsterSetInfo item14 = default(MonsterSetInfo);
		MoveMons item15 = default(MoveMons);
		CameraInfo item17 = default(CameraInfo);
		SetDoodadEffect item18 = default(SetDoodadEffect);
		ENpcBattleInfo item20 = default(ENpcBattleInfo);
		AbnormalInfo item21 = default(AbnormalInfo);
		ReputationInfo reputationInfo = default(ReputationInfo);
		ReputationInfo reputationInfo2 = default(ReputationInfo);
		ReputationInfo nativeAttitude = default(ReputationInfo);
		KillMons item22 = default(KillMons);
		ChangePartrolmode item23 = default(ChangePartrolmode);
		NpcType item24 = default(NpcType);
		CampState item25 = default(CampState);
		CampState item26 = default(CampState);
		while (sqliteDataReader.Read())
		{
			StoryData storyData = new StoryData();
			storyData.m_ID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ID")));
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("plot"));
			string[] array;
			if (!@string.Equals("0"))
			{
				array = @string.Split(',');
				string[] array2 = array;
				foreach (string value in array2)
				{
					storyData.m_triggerPlot.Add(Convert.ToInt32(value));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ScenarioID"));
			array = @string.Split(',');
			for (int j = 0; j < array.Length; j++)
			{
				int num = Convert.ToInt32(array[j]);
				if (num != 0)
				{
					storyData.m_TalkList.Add(num);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ScenarioSP"));
			array = @string.Split(',');
			for (int k = 0; k < array.Length; k++)
			{
				int num2 = Convert.ToInt32(array[k]);
				if (num2 != 0)
				{
					storyData.m_ServantTalkList.Add(num2);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("RandScenario"));
			array = @string.Split('_');
			if (array.Length == 4)
			{
				randScenarios.scenarioIds = new List<int>();
				randScenarios.id = Convert.ToInt32(array[0]);
				randScenarios.startOrClose = (array[1].Equals("1") ? true : false);
				randScenarios.cd = Convert.ToSingle(array[3]);
				string[] array3 = array[2].Split(',');
				string[] array4 = array3;
				foreach (string value2 in array4)
				{
					randScenarios.scenarioIds.Add(Convert.ToInt32(value2));
				}
				storyData.m_randScenarios = randScenarios;
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("CreateMons"));
			array = @string.Split(',');
			for (int m = 0; m < array.Length; m++)
			{
				string[] array3 = array[m].Split('_');
				if (array3.Length > 1)
				{
					item.num = Convert.ToInt32(array3[1]);
				}
				else
				{
					item.num = 1;
				}
				item.id = Convert.ToInt32(array3[0]);
				if (item.id != 0)
				{
					storyData.m_CreateMonster.Add(item);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("DelMons"));
			array = @string.Split(',');
			for (int n = 0; n < array.Length; n++)
			{
				int num3 = Convert.ToInt32(array[n]);
				if (num3 != 0)
				{
					storyData.m_DeleteMonster.Add(num3);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("TransNPC"));
			array = @string.Split(';');
			for (int num4 = 0; num4 < array.Length; num4++)
			{
				string[] array3 = array[num4].Split('_');
				if (array3.Length == 2)
				{
					NpcStyle npcStyle = new NpcStyle();
					npcStyle.npcid = Convert.ToInt32(array3[0]);
					string[] array5 = array3[1].Split(',');
					if (array5.Length == 3)
					{
						float x = Convert.ToSingle(array5[0]);
						float y = Convert.ToSingle(array5[1]);
						float z = Convert.ToSingle(array5[2]);
						npcStyle.pos = new Vector3(x, y, z);
						storyData.m_TransNpc.Add(npcStyle);
					}
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("MoveNPC"));
			array = @string.Split(':')[0].Split(';');
			for (int num5 = 0; num5 < array.Length; num5++)
			{
				string[] array3 = array[num5].Split('_');
				if (array3.Length == 2 || array3.Length == 4)
				{
					MoveNpcData moveNpcData = new MoveNpcData();
					string[] array5 = array3[0].Split(',');
					moveNpcData.npcsId = new List<int>(Array.ConvertAll(array5, (string s) => int.Parse(s)));
					array5 = array3[1].Split(',');
					if (array5.Length == 3)
					{
						float x = Convert.ToSingle(array5[0]);
						float y = Convert.ToSingle(array5[1]);
						float z = Convert.ToSingle(array5[2]);
						moveNpcData.pos = new Vector3(x, y, z);
					}
					else if (PEMath.IsNumeral(array5[0]))
					{
						moveNpcData.targetNpc = int.Parse(array5[0]);
					}
					else if (array5[0] == "Colony")
					{
						moveNpcData.targetNpc = -99;
					}
					else if (array5[0] == "NColony")
					{
						moveNpcData.targetNpc = -98;
					}
					if (array3.Length == 4)
					{
						moveNpcData.missionOrPlot_id = Convert.ToInt32(array3[2]) * 10000 + Convert.ToInt32(array3[3]);
					}
					storyData.m_MoveNpc.Add(moveNpcData);
				}
			}
			array = @string.Split(':');
			if (array.Length == 2)
			{
				string[] array3 = array[1].Split('_');
				if (array3.Length == 2)
				{
					storyData.m_moveNpc_missionOrPlot_id = Convert.ToInt32(array3[0]) * 10000 + Convert.ToInt32(array3[1]);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("RailNPC"));
			array = @string.Split('_');
			if (array.Length == 2)
			{
				string[] array3 = array[0].Split(',');
				for (int num6 = 0; num6 < array3.Length; num6++)
				{
					storyData.m_NpcRail.inpclist.Add(Convert.ToInt32(array3[num6]));
				}
				if (array[1] == "20000")
				{
					storyData.m_NpcRail.bplayer = true;
				}
				else
				{
					string[] array5 = array[1].Split(',');
					if (array5.Length == 3)
					{
						float x = Convert.ToSingle(array5[0]);
						float y = Convert.ToSingle(array5[1]);
						float z = Convert.ToSingle(array5[2]);
						storyData.m_NpcRail.pos = new Vector3(x, y, z);
					}
					else
					{
						storyData.m_NpcRail.othernpcid = Convert.ToInt32(array[1]);
					}
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("NPCface"));
			array = @string.Split(',');
			for (int num7 = 0; num7 < array.Length; num7++)
			{
				string[] array3 = array[num7].Split('_');
				if (array3.Length >= 2)
				{
					NpcFace npcFace = new NpcFace();
					npcFace.npcid = Convert.ToInt32(array3[0]);
					if (int.TryParse(array3[1], out var _))
					{
						npcFace.angle = Convert.ToInt32(array3[1]);
					}
					else
					{
						npcFace.npcother = array3[1];
					}
					if (array3.Length == 3)
					{
						npcFace.bmove = Convert.ToInt32(array3[2]) == 1;
					}
					storyData.m_NpcFace.Add(npcFace);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("NPCact"));
			array = @string.Split(',');
			for (int num8 = 0; num8 < array.Length; num8++)
			{
				string[] array3 = array[num8].Split('_');
				if (array3.Length == 4)
				{
					NpcAct item2 = default(NpcAct);
					item2.npcid = Convert.ToInt32(array3[0]);
					item2.animation = array3[1];
					item2.btrue = Convert.ToInt32(array3[2]) == 1;
					item2.bFloat = Convert.ToSingle(array3[3]);
					storyData.m_NpcAct.Add(item2);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("MonsAni"));
			array = @string.Split(';');
			for (int num9 = 0; num9 < array.Length; num9++)
			{
				string[] array3 = array[num9].Split('_');
				if (array3.Length == 4)
				{
					MonAct item3 = default(MonAct);
					item3.mons = new List<int>();
					string[] array5 = array3[0].Split(',');
					for (int num10 = 0; num10 < array5.Length; num10++)
					{
						item3.mons.Add(Convert.ToInt32(array5[num10]));
					}
					item3.animation = array3[1];
					item3.btrue = Convert.ToInt32(array3[2]) == 1;
					item3.time = Convert.ToSingle(array3[3]);
					storyData.m_MonAct.Add(item3);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("SetActive"));
			array = @string.Split(';');
			for (int num11 = 0; num11 < array.Length; num11++)
			{
				string[] array3 = array[num11].Split('_');
				if (array3.Length == 3)
				{
					ReputationInfo item4 = default(ReputationInfo);
					item4.isEffect = Convert.ToInt32(array3[0]) == 1;
					item4.type = Convert.ToInt32(array3[1]);
					item4.valve = Convert.ToInt32(array3[2]);
					storyData.m_NpcReq.Add(item4);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("NPCAni"));
			array = @string.Split(';');
			string[] array6 = array;
			foreach (string text in array6)
			{
				string[] array3 = text.Split('_');
				if (array3.Length == 3)
				{
					string[] array5 = array3[0].Split(',');
					for (int num13 = 0; num13 < array5.Length; num13++)
					{
						NpcAct item5 = default(NpcAct);
						item5.npcid = Convert.ToInt32(array5[num13]);
						item5.animation = array3[1];
						item5.btrue = (array3[2].Equals("1") ? true : false);
						storyData.m_NpcAnimator.Add(item5);
					}
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Playeract"));
			if (!@string.Equals("0"))
			{
				array = @string.Split('_');
				if (array.Length == 2)
				{
					storyData.m_PlayerAni.Add(array[0]);
					storyData.m_PlayerAni.Add(array[1]);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("NPCStatus"));
			if (!@string.Equals("0"))
			{
				array = @string.Split(';');
				string[] array7 = array;
				foreach (string text2 in array7)
				{
					string[] array3 = text2.Split('_');
					if (array3.Length == 2)
					{
						item6.id = Convert.ToInt32(array3[0]);
						item6.type = (ENpcMotionStyle)(Convert.ToInt32(array3[1]) + 1);
						storyData.m_MotionStyle.Add(item6);
					}
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Sceneface"));
			array = @string.Split(',');
			for (int num15 = 0; num15 < array.Length; num15++)
			{
				string[] array3 = array[num15].Split('-');
				if (array3.Length == 2)
				{
					item7.name = array3[0];
					item7.angle = Convert.ToSingle(array3[1]);
					storyData.m_SenceFace.Add(item7);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Sceneact"));
			array = @string.Split(',');
			for (int num16 = 0; num16 < array.Length; num16++)
			{
				string[] array3 = array[num16].Split('-');
				if (array3.Length == 2)
				{
					item8.name = array3[0];
					item8.act = array3[1];
					storyData.m_SenceAct.Add(item8);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("NPCcamp"));
			array = @string.Split(',');
			for (int num17 = 0; num17 < array.Length; num17++)
			{
				string[] array3 = array[num17].Split('_');
				if (array3.Length == 2)
				{
					NpcCamp item9 = default(NpcCamp);
					item9.npcid = Convert.ToInt32(array3[0]);
					item9.camp = Convert.ToInt32(array3[1]);
					storyData.m_NpcCamp.Add(item9);
				}
			}
			storyData.m_Special = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Special"));
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Ai"));
			array = @string.Split(',');
			for (int num18 = 0; num18 < array.Length; num18++)
			{
				string[] array3 = array[num18].Split('_');
				if (array3.Length == 2)
				{
					NpcOpen item10 = default(NpcOpen);
					item10.npcid = Convert.ToInt32(array3[0]);
					item10.bopen = Convert.ToInt32(array3[1]) == 1;
					storyData.m_NpcAI.Add(item10);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Invincible"));
			array = @string.Split(',');
			for (int num19 = 0; num19 < array.Length; num19++)
			{
				string[] array3 = array[num19].Split('_');
				if (array3.Length == 2)
				{
					NpcOpen item11 = default(NpcOpen);
					item11.npcid = Convert.ToInt32(array3[0]);
					item11.bopen = Convert.ToInt32(array3[1]) == 1;
					storyData.m_NpcInvincible.Add(item11);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("FolPlayer"));
			array = @string.Split(',');
			for (int num20 = 0; num20 < array.Length; num20++)
			{
				string[] array3 = array[num20].Split('_');
				if (array3.Length == 2)
				{
					NpcOpen item12 = default(NpcOpen);
					item12.npcid = Convert.ToInt32(array3[0]);
					item12.bopen = Convert.ToInt32(array3[1]) == 1;
					storyData.m_FollowPlayerList.Add(item12);
				}
			}
			storyData.m_FollowDist = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("FollowDist")));
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Monstercamp"));
			array = @string.Split(',');
			for (int num21 = 0; num21 < array.Length; num21++)
			{
				string[] array3 = array[num21].Split('_');
				if (array3.Length == 2)
				{
					item13.id = Convert.ToInt32(array3[0]);
					item13.value = Convert.ToInt32(array3[1]);
					if (item13.id != 0)
					{
						storyData.m_MonsterCampList.Add(item13);
					}
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Monsterharm"));
			array = @string.Split(',');
			for (int num22 = 0; num22 < array.Length; num22++)
			{
				string[] array3 = array[num22].Split('_');
				if (array3.Length == 2)
				{
					item14.id = Convert.ToInt32(array3[0]);
					item14.value = Convert.ToInt32(array3[1]);
					if (item14.id != 0)
					{
						storyData.m_MonsterHarmList.Add(item14);
					}
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("MoveMons"));
			array = @string.Split(';');
			string[] array8 = array;
			foreach (string text3 in array8)
			{
				string[] array3 = text3.Split('_');
				if (array3.Length == 3 || array3.Length == 5)
				{
					item15.fixedId = Convert.ToInt32(array3[0]);
					item15.stepOrRun = Convert.ToInt32(array3[1]);
					string[] array5 = array3[2].Split(',');
					float x = Convert.ToSingle(array5[0]);
					float y = Convert.ToSingle(array5[1]);
					float z = Convert.ToSingle(array5[2]);
					item15.dist = new Vector3(x, y, z);
					if (array3.Length == 5)
					{
						item15.missionOrPlot_id = Convert.ToInt32(array3[3]) * 10000 + Convert.ToInt32(array3[4]);
					}
					else
					{
						item15.missionOrPlot_id = 0;
					}
					storyData.m_moveMons.Add(item15);
				}
			}
			array = @string.Split(':');
			if (array.Length == 2)
			{
				string[] array3 = array[1].Split('_');
				if (array3.Length == 2)
				{
					storyData.m_moveMons_missionOrPlot_id = Convert.ToInt32(array3[0]) * 10000 + Convert.ToInt32(array3[1]);
				}
			}
			storyData.m_MoveType = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Movemethod")));
			storyData.m_PausePlayer = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("PausePlayer")));
			storyData.m_PauseNPC = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("PauseNPC"))) <= 0;
			storyData.m_PauseMons = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("PauseMons"))) > 0;
			storyData.m_Delay = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Delay")));
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("EffectsPos"));
			array = @string.Split(';');
			for (int num24 = 0; num24 < array.Length; num24++)
			{
				string[] array3 = array[num24].Split(',');
				if (array3.Length == 3)
				{
					float x = Convert.ToSingle(array3[0]);
					float y = Convert.ToSingle(array3[1]);
					float z = Convert.ToSingle(array3[2]);
					storyData.m_EffectPosList.Add(new Vector3(x, y, z));
				}
			}
			storyData.m_EffectID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("EffectsID")));
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("SoundsPos"));
			array = @string.Split(';');
			for (int num25 = 0; num25 < array.Length; num25++)
			{
				string[] array3 = array[num25].Split(',');
				if (array3.Length == 3 || array3.Length == 1)
				{
					PosData item16 = default(PosData);
					if (array3.Length == 3)
					{
						item16.type = 3;
						float x = Convert.ToSingle(array3[0]);
						float y = Convert.ToSingle(array3[1]);
						float z = Convert.ToSingle(array3[2]);
						item16.pos = new Vector3(x, y, z);
					}
					else if (array3.Length == 1)
					{
						item16.type = 1;
						item16.npcID = Convert.ToInt32(array3[0]);
					}
					storyData.m_SoundPosList.Add(item16);
				}
			}
			storyData.m_SoundID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("SoundsID")));
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("CameraID"));
			array = @string.Split(',');
			for (int num26 = 0; num26 < array.Length; num26++)
			{
				if (!(array[num26] == "0"))
				{
					string[] array3 = array[num26].Split('_');
					item17.cameraId = Convert.ToInt32(array3[0]);
					if (array3.Length == 2)
					{
						item17.talkId = Convert.ToInt32(array3[1]);
					}
					else
					{
						item17.talkId = 0;
					}
					storyData.m_CameraList.Add(item17);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ColonyNoOrder"));
			array = @string.Split(',');
			for (int num27 = 0; num27 < array.Length; num27++)
			{
				if (!(array[num27] == "0"))
				{
					storyData.m_iColonyNoOrderNpcList.Add(Convert.ToInt32(array[num27]));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ColonyOrder"));
			array = @string.Split(',');
			for (int num28 = 0; num28 < array.Length; num28++)
			{
				if (!(array[num28] == "0"))
				{
					storyData.m_iColonyOrderNpcList.Add(Convert.ToInt32(array[num28]));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Siege"));
			storyData.m_pauseSiege = Convert.ToInt32(@string);
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("AttractMons"));
			if (!@string.Equals("0"))
			{
				array = @string.Split(':');
				string[] array3 = array[0].Split(',');
				string[] array9 = array3;
				foreach (string value3 in array9)
				{
					storyData.m_attractMons.Add(Convert.ToInt32(value3));
				}
				storyData.m_attractMons.Add(-9999);
				string[] array5 = array[1].Split('_', ',');
				string[] array10 = array5;
				foreach (string value4 in array10)
				{
					storyData.m_attractMons.Add(Convert.ToInt32(value4));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("KillNPC"));
			if (@string != "0")
			{
				array = @string.Split('_', ';');
				for (int num31 = 0; num31 < array.Length; num31++)
				{
					storyData.m_killNpcList.Add(Convert.ToInt32(array[num31]));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("MonsterHatred"));
			if (@string != "0")
			{
				array = @string.Split('_', ';');
				for (int num32 = 0; num32 < array.Length; num32++)
				{
					storyData.m_monsterHatredList.Add(Convert.ToInt32(array[num32]));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("NPCHatred"));
			if (@string != "0")
			{
				array = @string.Split('_', ';');
				for (int num33 = 0; num33 < array.Length; num33++)
				{
					storyData.m_npcHatredList.Add(Convert.ToInt32(array[num33]));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Harm"));
			if (@string != "0")
			{
				array = @string.Split('_', ';');
				for (int num34 = 0; num34 < array.Length; num34++)
				{
					storyData.m_harmList.Add(Convert.ToInt32(array[num34]));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("DoodadHarm"));
			if (@string != "0")
			{
				array = @string.Split('_', ';');
				if (array.Length == 3)
				{
					string[] array11 = array;
					foreach (string value5 in array11)
					{
						storyData.m_doodadHarmList.Add(Convert.ToInt32(value5));
					}
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("DoodadEffect"));
			if (@string != "0")
			{
				array = @string.Split(';');
				string[] array12 = array;
				foreach (string text4 in array12)
				{
					string[] array3 = text4.Split(':');
					if (array3.Length == 3)
					{
						item18.id = Convert.ToInt32(array3[0]);
						item18.names = new List<string>();
						string[] array5 = array3[1].Split(',');
						string[] array13 = array5;
						foreach (string item19 in array13)
						{
							item18.names.Add(item19);
						}
						item18.openOrClose = Convert.ToInt32(array3[2]) == 1;
						storyData.m_doodadEffectList.Add(item18);
					}
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ENpcBattle"));
			if (@string != "0")
			{
				array = @string.Split(';');
				string[] array14 = array;
				foreach (string text5 in array14)
				{
					string[] array3 = text5.Split(',', '_');
					if (array3.Length >= 2)
					{
						item20.npcId = new List<int>();
						for (int num39 = 0; num39 < array3.Length - 1; num39++)
						{
							item20.npcId.Add(Convert.ToInt32(array3[num39]));
						}
						item20.type = Convert.ToInt32(array3[array3.Length - 1]);
						storyData.m_npcsBattle.Add(item20);
					}
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Area"));
			if (@string != "0")
			{
				array = @string.Split('_');
				if (array.Length == 6)
				{
					for (int num40 = 0; num40 < array.Length; num40++)
					{
						storyData.m_plotMissionTrigger.Add(array[num40]);
					}
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("CantRevive"));
			array = @string.Split(',');
			for (int num41 = 0; num41 < array.Length; num41++)
			{
				if (Convert.ToInt32(array[num41]) != 0)
				{
					storyData.m_cantReviveNpc.Add(Convert.ToInt32(array[num41]));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Abnormal"));
			array = @string.Split(';');
			for (int num42 = 0; num42 < array.Length; num42++)
			{
				string[] array3 = array[num42].Split('_');
				if (array3.Length == 3)
				{
					item21.npcs = new List<int>();
					if (Convert.ToInt32(array3[0]) == 1)
					{
						item21.setOrRevive = true;
					}
					else
					{
						item21.setOrRevive = false;
					}
					string[] array5 = array3[1].Split(',');
					for (int num43 = 0; num43 < array5.Length; num43++)
					{
						item21.npcs.Add(Convert.ToInt32(array5[num43]));
					}
					item21.virusNum = Convert.ToInt32(array3[2]);
					storyData.m_abnormalInfo.Add(item21);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ReputationPuja"));
			array = @string.Split('_');
			if (array.Length == 2)
			{
				reputationInfo.type = Convert.ToInt32(array[0]);
				reputationInfo.valve = Convert.ToInt32(array[1]);
				reputationInfo.isEffect = true;
				storyData.m_reputationChange[0] = reputationInfo;
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ReputationPaja"));
			array = @string.Split('_');
			if (array.Length == 2)
			{
				reputationInfo2.type = Convert.ToInt32(array[0]);
				reputationInfo2.valve = Convert.ToInt32(array[1]);
				reputationInfo2.isEffect = true;
				storyData.m_reputationChange[1] = reputationInfo2;
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("NativeAttitude"));
			array = @string.Split('_');
			if (array.Length == 2)
			{
				nativeAttitude.type = Convert.ToInt32(array[0]);
				nativeAttitude.valve = Convert.ToInt32(array[1]);
				nativeAttitude.isEffect = true;
				storyData.m_nativeAttitude = nativeAttitude;
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ChangeDoodad"));
			array = @string.Split(';');
			if (array.Length == 2)
			{
				string[] array3 = array[0].Split(',');
				string[] array5 = array[1].Split(',');
				string[] array15 = array3;
				foreach (string value6 in array15)
				{
					int num45 = Convert.ToInt32(value6);
					if (num45 != 0)
					{
						storyData.oldDoodad.Add(num45);
					}
				}
				string[] array16 = array5;
				foreach (string value7 in array16)
				{
					int num45 = Convert.ToInt32(value7);
					if (num45 != 0)
					{
						storyData.newDoodad.Add(num45);
					}
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("KillMons"));
			array = @string.Split(';');
			string[] array17 = array;
			foreach (string text6 in array17)
			{
				string[] array3 = text6.Split('_');
				if (array3.Length == 5)
				{
					item22.id = Convert.ToInt32(array3[0]);
					string[] array5 = array3[1].Split(',');
					if (array5.Length == 3)
					{
						float x = Convert.ToSingle(array5[0]);
						float y = Convert.ToSingle(array5[1]);
						float z = Convert.ToSingle(array5[2]);
						item22.center = new Vector3(x, y, z);
						item22.radius = Convert.ToSingle(array3[2]);
						item22.type = KillMons.Type.protoTypeId;
					}
					else
					{
						item22.center = Vector3.zero;
						item22.radius = 0f;
						item22.type = KillMons.Type.fixedId;
					}
					item22.monId = Convert.ToInt32(array3[3]);
					item22.reviveTime = Convert.ToInt32(array3[4]);
					storyData.m_killMons.Add(item22);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("StopKillMons"));
			array = @string.Split(',');
			string[] array18 = array;
			foreach (string text7 in array18)
			{
				if (!text7.Equals("0"))
				{
					storyData.m_stopKillMonsID.Add(Convert.ToInt32(text7));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("MonsPatrolRange"));
			array = @string.Split(';');
			string[] array19 = array;
			foreach (string text8 in array19)
			{
				string[] array3 = text8.Split('_');
				if (array3.Length == 3)
				{
					item23.monsId = new List<int>();
					string[] array5 = array3[0].Split(',');
					string[] array20 = array5;
					foreach (string value8 in array20)
					{
						item23.monsId.Add(Convert.ToInt32(value8));
					}
					item23.type = Convert.ToInt32(array3[1]);
					item23.radius = Convert.ToInt32(array3[2]);
					storyData.m_monPatrolMode.Add(item23);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("NPCType"));
			array = @string.Split(';');
			string[] array21 = array;
			foreach (string text9 in array21)
			{
				if (text9.Equals("0"))
				{
					continue;
				}
				item24.npcs = new List<int>();
				item24.type = -1;
				string[] array22 = text9.Split('_');
				if (array22.Length == 2)
				{
					string[] array23 = array22[0].Split(',');
					foreach (string value9 in array23)
					{
						item24.npcs.Add(Convert.ToInt32(value9));
					}
					item24.type = Convert.ToInt32(array22[1]);
					storyData.m_npcType.Add(item24);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("MonsCheck"));
			array = @string.Split(':');
			if (array.Length == 2)
			{
				string[] array3 = array[0].Split(';');
				string[] array24 = array3;
				foreach (string text10 in array24)
				{
					string[] array5 = text10.Split('_');
					if (array5.Length == 4)
					{
						CheckMons checkMons = new CheckMons();
						checkMons.existOrNot = (array5[0].Equals("1") ? true : false);
						if (array5[1].Split(',').Length == 3)
						{
							float x = Convert.ToSingle(array5[1].Split(',')[0]);
							float y = Convert.ToSingle(array5[1].Split(',')[1]);
							float z = Convert.ToSingle(array5[1].Split(',')[2]);
							checkMons.center = new Vector3(x, y, z);
						}
						else
						{
							checkMons.npcid = Convert.ToInt32(array5[1]);
						}
						checkMons.radius = Convert.ToInt32(array5[2]);
						checkMons.protoTypeid = Convert.ToInt32(array5[3]);
						storyData.m_checkMons.Add(checkMons);
					}
				}
				if (storyData.m_checkMons.Count > 0)
				{
					CheckMons checkMons2 = new CheckMons();
					checkMons2.missionOrPlot = (array[1].Split('_')[0].Equals("1") ? true : false);
					string text11 = array[1].Split('_')[1];
					string[] array25 = text11.Split(',');
					foreach (string value10 in array25)
					{
						checkMons2.trigerId.Add(Convert.ToInt32(value10));
					}
					storyData.m_checkMons.Add(checkMons2);
				}
			}
			storyData.m_increaseLangSkill = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("IncreaseLangSkill")));
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("SetAlert"));
			array = @string.Split(';');
			string[] array26 = array;
			foreach (string text12 in array26)
			{
				string[] array3 = text12.Split('_');
				if (array3.Length == 2)
				{
					item25.id = Convert.ToInt32(array3[0]);
					item25.isActive = Convert.ToInt32(array3[1]) == 1;
					storyData.m_campAlert.Add(item25);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("SetActiveCamp"));
			array = @string.Split(';');
			string[] array27 = array;
			foreach (string text13 in array27)
			{
				string[] array3 = text13.Split('_');
				if (array3.Length == 2)
				{
					item26.id = Convert.ToInt32(array3[0]);
					item26.isActive = Convert.ToInt32(array3[1]) == 1;
					storyData.m_campActive.Add(item26);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ComMission"));
			array = @string.Split(',');
			string[] array28 = array;
			foreach (string value11 in array28)
			{
				int num58 = Convert.ToInt32(value11);
				if (num58 != 0)
				{
					storyData.m_comMission.Add(num58);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("CantWhacked"));
			if (@string != "0")
			{
				array = @string.Split(';');
				for (int num59 = 0; num59 < array.Length; num59++)
				{
					ENpcBattleInfo item27 = default(ENpcBattleInfo);
					item27.npcId = new List<int>();
					string[] array3 = array[num59].Split('_');
					item27.type = Convert.ToInt32(array3[1]);
					string[] array5 = array3[0].Split(',');
					for (int num60 = 0; num60 < array5.Length; num60++)
					{
						item27.npcId.Add(Convert.ToInt32(array5[num60]));
					}
					storyData.m_whackedList.Add(item27);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("GetMission"));
			if (@string != "0")
			{
				array = @string.Split(',');
				for (int num61 = 0; num61 < array.Length; num61++)
				{
					storyData.m_getMission.Add(Convert.ToInt32(array[num61]));
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ShowTip"));
			storyData.m_showTip = Convert.ToInt32(@string);
			m_StoryRespository.Add(storyData.m_ID, storyData);
		}
	}

	public static AdStoryData GetAdStroyData(int id)
	{
		if (!m_AdStoryRespository.ContainsKey(id))
		{
			return null;
		}
		return m_AdStoryRespository[id];
	}

	public static void LoadData_Adventure()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("AdvPlot");
		while (sqliteDataReader.Read())
		{
			AdStoryData adStoryData = new AdStoryData();
			adStoryData.m_ID = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ID")));
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("CreNPC"));
			string[] array = @string.Split(';');
			for (int i = 0; i < array.Length; i++)
			{
				string[] array2 = array[i].Split('_');
				if (array2.Length == 4)
				{
					AdCreNPC adCreNPC = new AdCreNPC();
					adCreNPC.referToType = (ReferToType)Convert.ToInt32(array2[0]);
					adCreNPC.m_referToID = Convert.ToInt32(array2[1]);
					adCreNPC.m_radius = Convert.ToSingle(array2[2]);
					adCreNPC.m_NPCID = Convert.ToInt32(array2[3]);
					adStoryData.m_creNPC.Add(adCreNPC);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("NearPos"));
			array = @string.Split(';');
			for (int j = 0; j < array.Length; j++)
			{
				string[] array2 = array[j].Split('_');
				if (array2.Length == 4)
				{
					AdEnterArea adEnterArea = new AdEnterArea();
					adEnterArea.referToType = Convert.ToInt32(array2[0]);
					adEnterArea.m_referToID = Convert.ToInt32(array2[1]);
					adEnterArea.m_radius = Convert.ToSingle(array2[2]);
					string[] array3 = array2[3].Split(',');
					for (int k = 0; k < array3.Length; k++)
					{
						adEnterArea.m_plotID.Add(Convert.ToInt32(array3[j]));
					}
					adStoryData.m_enterArea.Add(adEnterArea);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("NPCMove"));
			array = @string.Split(';');
			for (int l = 0; l < array.Length; l++)
			{
				string[] array2 = array[l].Split('_');
				if (array2.Length == 4)
				{
					AdNPCMove adNPCMove = new AdNPCMove();
					adNPCMove.npcID = Convert.ToInt32(array2[0]);
					adNPCMove.referToType = (ReferToType)Convert.ToInt32(array2[1]);
					adNPCMove.m_referToID = Convert.ToInt32(array2[2]);
					adNPCMove.m_radius = Convert.ToSingle(array2[3]);
					adStoryData.m_npcMove.Add(adNPCMove);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("GetMission"));
			array = @string.Split(',');
			for (int m = 0; m < array.Length; m++)
			{
				int num = Convert.ToInt32(array[m]);
				if (num != 0)
				{
					adStoryData.m_getMissionID.Add(num);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ComMission"));
			array = @string.Split(',');
			for (int n = 0; n < array.Length; n++)
			{
				int num2 = Convert.ToInt32(array[n]);
				if (num2 != 0)
				{
					adStoryData.m_comMissionID.Add(num2);
				}
			}
			@string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("ShowTip"));
			adStoryData.m_showTip = Convert.ToInt32(@string);
			m_AdStoryRespository.Add(adStoryData.m_ID, adStoryData);
		}
	}

	public static void LoadData()
	{
		LoadData_story();
		LoadData_Adventure();
	}
}
