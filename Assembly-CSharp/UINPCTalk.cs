using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using AiAsset;
using ItemAsset;
using Pathea;
using Pathea.PeEntityExt;
using Pathea.PeEntityExtMotion_Move;
using Pathea.PeEntityExtTrans;
using PETools;
using UnityEngine;

public class UINPCTalk : UIBaseWidget
{
	public enum NormalOrSp
	{
		Normal,
		SP,
		halfSP
	}

	public struct NpcTalkInfo
	{
		public int talkid;

		public Texture2D npcicon;

		public int npcid;

		public List<int> otherNpc;

		public string desc;

		public int soundid;

		public string clip;

		public bool isRadio;

		public int needLangSkill;

		public object talkToNpcidOrVecter3;

		public object moveToNpcidOrVecter3;

		public int moveType;

		public List<int> endOtherNpc;

		public int missionTrigger;

		public MissionManager.TakeMissionType type;
	}

	public UISlicedSprite mBg;

	public UILabel mContent;

	public UILabel mName;

	public UISprite mNpcBigHeadSp;

	public UITexture mNpcBigHeadTex;

	public UISprite mMicroPhoneSp;

	public UIPanel mTuition;

	public UITable mUITable;

	public GameObject mCloseBtn;

	public GameObject mTalkOnlyWnd;

	public MissionSelItem_N mPrefab;

	public NormalOrSp type;

	public bool canClose = true;

	public bool isPlayingTalk;

	private List<MissionSelItem_N> mSelList = new List<MissionSelItem_N>();

	private bool mUpdateList;

	public AudioController currentAudio;

	private bool spTalkEndByAudioTime;

	public UIScrollBar mScrollBar;

	public Collider mDragCollider;

	public static bool m_QuickZM;

	public List<NpcTalkInfo> m_NpcTalkList = new List<NpcTalkInfo>();

	private int m_CurTalkIdx;

	private AudioSource m_Audio;

	private bool m_bMutex;

	private List<int> m_SelectMissionList = new List<int>();

	public int m_selectMissionSource;

	public PeEntity m_CurTalkNpc;

	private List<PeEntity> movingNpc = new List<PeEntity>();

	private Dictionary<PeEntity, object[]> needTalkNpc = new Dictionary<PeEntity, object[]>();

	private Dictionary<int, int> npcid_targetid = new Dictionary<int, int>();

	public void UpdateNpcTalkInfo(List<int> tmpList, MissionCommonData data = null, bool IsClearTalkList = true)
	{
		if (IsClearTalkList)
		{
			ClearNpcTalkInfos();
		}
		MatchTalkInfo(tmpList, data, IsClearTalkList);
	}

	public void ClearNpcTalkInfos()
	{
		foreach (NpcTalkInfo npcTalk in m_NpcTalkList)
		{
			if (npcTalk.npcicon != null)
			{
				UnityEngine.Object.Destroy(npcTalk.npcicon);
			}
		}
		m_NpcTalkList.Clear();
	}

	private void SetNpcBigHeadSp(string spName)
	{
		mNpcBigHeadSp.spriteName = spName;
		mNpcBigHeadSp.MakePixelPerfect();
	}

	private void ShowNpcMicroPhoneSp(bool _show)
	{
		mMicroPhoneSp.enabled = _show;
	}

	private void GetTalkInfo2(MissionCommonData data, ref List<int> talklist)
	{
		for (int i = 0; i < data.m_TargetIDList.Count; i++)
		{
			TargetType targetType = MissionRepository.GetTargetType(data.m_TargetIDList[i]);
			if (targetType != TargetType.TargetType_Follow)
			{
				continue;
			}
			TypeFollowData typeFollowData = MissionManager.GetTypeFollowData(data.m_TargetIDList[i]);
			if (typeFollowData == null)
			{
				continue;
			}
			for (int j = 0; j < typeFollowData.m_TalkInfo.Count; j++)
			{
				for (int k = 0; k < typeFollowData.m_TalkInfo[j].talkid.Count; k++)
				{
					talklist.Add(typeFollowData.m_TalkInfo[j].talkid[k]);
				}
			}
		}
		for (int l = 0; l < data.m_TalkIN.Count; l++)
		{
			talklist.Add(data.m_TalkIN[l]);
		}
	}

	private void GetTalkInfo3(MissionCommonData data, int targetid, ref List<int> talklist)
	{
		if (data.m_Type == MissionType.MissionType_Mul)
		{
			int num = 0;
			for (int i = 0; i < data.m_TargetIDList.Count; i++)
			{
				if (MissionManager.Instance.IsReplyTarget(data.m_ID, data.m_TargetIDList[i]))
				{
					num++;
				}
			}
			num--;
			if (num >= 0 && data.m_TalkED.Count > num)
			{
				talklist.Add(data.m_TalkED[num]);
			}
			return;
		}
		switch (MissionRepository.GetTargetType(targetid))
		{
		case TargetType.TargetType_Follow:
		{
			TypeFollowData typeFollowData = MissionManager.GetTypeFollowData(targetid);
			if (typeFollowData != null)
			{
				for (int k = 0; k < typeFollowData.m_ComTalkID.Count; k++)
				{
					talklist.Add(typeFollowData.m_ComTalkID[k]);
				}
			}
			break;
		}
		case TargetType.TargetType_Discovery:
		{
			TypeSearchData typeSearchData = MissionManager.GetTypeSearchData(targetid);
			if (typeSearchData != null)
			{
				for (int j = 0; j < typeSearchData.m_TalkID.Count; j++)
				{
					talklist.Add(typeSearchData.m_TalkID[j]);
				}
			}
			break;
		}
		}
		for (int l = 0; l < data.m_TalkED.Count; l++)
		{
			talklist.Add(data.m_TalkED[l]);
		}
	}

	private void GetTalkInfo4(MissionCommonData data, int targetid, bool bFailed, ref List<int> talklist)
	{
		TypeUseItemData typeUseItemData = MissionManager.GetTypeUseItemData(targetid);
		if (typeUseItemData == null)
		{
			return;
		}
		if (!bFailed)
		{
			talklist = new List<int>();
			for (int i = 0; i < typeUseItemData.m_UsedPrompt.Count; i++)
			{
				talklist.Add(typeUseItemData.m_UsedPrompt[i]);
			}
			for (int j = 0; j < typeUseItemData.m_TalkID.Count; j++)
			{
				talklist.Add(typeUseItemData.m_TalkID[j]);
			}
		}
		else
		{
			talklist = typeUseItemData.m_FailPrompt;
		}
	}

	private void GetTalkInfo5(MissionCommonData data, int targetid, ref List<int> talklist)
	{
		switch (MissionRepository.GetTargetType(targetid))
		{
		case TargetType.TargetType_Follow:
		{
			TypeFollowData typeFollowData = MissionManager.GetTypeFollowData(targetid);
			if (typeFollowData != null)
			{
				for (int j = 0; j < typeFollowData.m_ComTalkID.Count; j++)
				{
					talklist.Add(typeFollowData.m_ComTalkID[j]);
				}
			}
			break;
		}
		case TargetType.TargetType_Discovery:
		{
			TypeSearchData typeSearchData = MissionManager.GetTypeSearchData(targetid);
			if (typeSearchData != null)
			{
				for (int i = 0; i < typeSearchData.m_TalkID.Count; i++)
				{
					talklist.Add(typeSearchData.m_TalkID[i]);
				}
			}
			break;
		}
		}
	}

	private List<int> GetTalkInfo(int MissionID, byte type, MissionCommonData data, int targetid, bool bFailed)
	{
		List<int> list = null;
		if (MissionID == -1)
		{
			list = new List<int>();
			int item = 1;
			list.Add(item);
		}
		else
		{
			switch (type)
			{
			case 1:
				list = data.m_TalkOP;
				break;
			case 2:
				list = new List<int>();
				GetTalkInfo2(data, ref list);
				break;
			case 3:
				list = new List<int>();
				GetTalkInfo3(data, targetid, ref list);
				break;
			case 4:
				list = new List<int>();
				GetTalkInfo4(data, targetid, bFailed, ref list);
				break;
			case 5:
				list = new List<int>();
				GetTalkInfo5(data, targetid, ref list);
				break;
			case 6:
				list = data.m_PromptOP;
				break;
			case 7:
				list = data.m_PromptIN;
				break;
			case 8:
				list = data.m_PromptED;
				break;
			default:
				list = new List<int>();
				break;
			}
		}
		return list;
	}

	public void GetTalkInfo(int talkid, ref NpcTalkInfo talkinfo, MissionCommonData data)
	{
		talkinfo.talkid = talkid;
		TalkData talkData = TalkRespository.GetTalkData(talkid);
		if (talkData != null)
		{
			talkinfo.soundid = talkData.m_SoundID;
			talkinfo.clip = talkData.m_ClipName;
			talkinfo.needLangSkill = talkData.needLangSkill;
			talkinfo.desc = ParseStrDefine(talkData.m_Content, data, talkinfo.needLangSkill);
			talkinfo.isRadio = talkData.isRadio;
			talkinfo.talkToNpcidOrVecter3 = talkData.talkToNpcidOrVecter3;
			talkinfo.moveToNpcidOrVecter3 = talkData.moveTonpcidOrvecter3;
			talkinfo.otherNpc = talkData.m_otherNpc;
			talkinfo.endOtherNpc = talkData.m_endOtherNpc;
			talkinfo.moveType = talkData.m_moveType;
			talkinfo.npcid = talkData.m_NpcID;
		}
	}

	public void ParseName(MissionCommonData data, ref NpcTalkInfo talkinfo)
	{
		if (talkinfo.npcid != -9999)
		{
			if (talkinfo.npcid != 0)
			{
				return;
			}
			if (data == null)
			{
				talkinfo.npcid = GameUI.Instance.mNpcWnd.m_CurSelNpc.Id;
			}
			else if (MissionManager.HasRandomMission(data.m_ID) || data.m_ID == 9137 || data.m_ID == 9138)
			{
				if (data.m_ID == GameUI.Instance.mNpcWnd.BtnClickMission)
				{
					if (GameUI.Instance.mNpcWnd.m_CurSelNpc != null)
					{
						talkinfo.npcid = GameUI.Instance.mNpcWnd.m_CurSelNpc.Id;
					}
					else if (data.m_iNpc != 0)
					{
						talkinfo.npcid = data.m_iNpc;
					}
					else if (data == null && PeSingleton<PeCreature>.Instance.mainPlayer != null)
					{
						talkinfo.npcid = PeSingleton<PeCreature>.Instance.mainPlayer.Id;
					}
				}
				else if (data.m_iNpc != 0)
				{
					talkinfo.npcid = data.m_iNpc;
				}
				else if (GameUI.Instance.mNpcWnd.m_CurSelNpc != null)
				{
					talkinfo.npcid = GameUI.Instance.mNpcWnd.m_CurSelNpc.Id;
				}
				else if (data == null && PeSingleton<PeCreature>.Instance.mainPlayer != null)
				{
					talkinfo.npcid = PeSingleton<PeCreature>.Instance.mainPlayer.Id;
				}
			}
			else if (data.m_iReplyNpc != 0)
			{
				talkinfo.npcid = data.m_iReplyNpc;
			}
			else if (data.m_iNpc != 0)
			{
				talkinfo.npcid = data.m_iNpc;
			}
			else if (GameUI.Instance.mNpcWnd.m_CurSelNpc != null)
			{
				talkinfo.npcid = GameUI.Instance.mNpcWnd.m_CurSelNpc.Id;
			}
			else if (data == null && PeSingleton<PeCreature>.Instance.mainPlayer != null)
			{
				talkinfo.npcid = PeSingleton<PeCreature>.Instance.mainPlayer.Id;
			}
		}
		else if (PeSingleton<PeCreature>.Instance.mainPlayer != null)
		{
			talkinfo.npcid = PeSingleton<PeCreature>.Instance.mainPlayer.Id;
		}
	}

	public string ParseStrDefine(string content, MissionCommonData data, int needLangSkill = 0)
	{
		if (content == null)
		{
			return string.Empty;
		}
		string text = "\"monsterid%\"";
		string text2 = "\"monsternum%\"";
		string text3 = "\"position%\"";
		string text4 = "\"npcid1%\"";
		string text5 = "\"npcid2%\"";
		string text6 = "\"npcid3%\"";
		string oldValue = "\"npclist%\"";
		string text7 = "\"npcnum%\"";
		string text8 = "\"itemid%\"";
		string text9 = "\"itemnum%\"";
		string text10 = "\"targetitemid%\"";
		string text11 = "\"givenpcid%\"";
		string text12 = "\"receivenpcid%\"";
		string text13 = "\"n-ri%\"";
		string text14 = "\"ri%\"";
		string text15 = "\"name%\"";
		string text16 = "\"seedname%\"";
		string text17 = "\"npcdead%\"";
		string text18 = "\"npc_name%\"";
		string value = "\"AdvNPC%\"";
		string value2 = "\"Town%\"";
		string value3 = "\"AI%\"";
		string text19 = "\"colonistnum%\"";
		string text20 = "\"EnemyCamp%\"";
		if (data != null)
		{
			for (int i = 0; i < data.m_TargetIDList.Count; i++)
			{
				TypeMonsterData typeMonsterData = MissionManager.GetTypeMonsterData(data.m_TargetIDList[i]);
				TypeCollectData typeCollectData = MissionManager.GetTypeCollectData(data.m_TargetIDList[i]);
				TypeFollowData typeFollowData = MissionManager.GetTypeFollowData(data.m_TargetIDList[i]);
				TypeSearchData typeSearchData = MissionManager.GetTypeSearchData(data.m_TargetIDList[i]);
				TypeUseItemData typeUseItemData = MissionManager.GetTypeUseItemData(data.m_TargetIDList[i]);
				TypeMessengerData typeMessengerData = MissionManager.GetTypeMessengerData(data.m_TargetIDList[i]);
				if (typeMonsterData != null)
				{
					if (content.Contains(text))
					{
						content = content.Replace(text, AiDataBlock.GetAIDataName(typeMonsterData.m_MonsterID));
					}
					if (content.Contains(text2))
					{
						content = content.Replace(text2, typeMonsterData.m_MonsterNum.ToString());
					}
				}
				else if (typeCollectData != null)
				{
					if (content.Contains(text8))
					{
						content = content.Replace(text8, ItemProto.GetName(typeCollectData.ItemID));
					}
					if (content.Contains(text9))
					{
						content = content.Replace(text9, typeCollectData.ItemNum.ToString());
					}
					if (content.Contains(text10))
					{
						content = content.Replace(text10, typeCollectData.m_TargetItemID.ToString());
					}
					string newValue = string.Empty;
					PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(data.m_iNpc);
					if (peEntity != null)
					{
						newValue = peEntity.ExtGetName();
					}
					if (content.Contains(text4))
					{
						content = content.Replace(text4, newValue);
					}
				}
				else if (typeFollowData != null)
				{
					if (content.Contains(text3))
					{
						content = content.Replace(text3, typeFollowData.m_DistPos.ToString());
					}
					for (int j = 0; j < typeFollowData.m_iNpcList.Count; j++)
					{
						if (j == 0 && content.Contains(text4))
						{
							PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(typeFollowData.m_iNpcList[j]);
							if (peEntity != null)
							{
								content = content.Replace(text4, peEntity.ExtGetName());
							}
						}
						else if (j == 1 && content.Contains(text5))
						{
							PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(typeFollowData.m_iNpcList[j]);
							if (peEntity != null)
							{
								content = content.Replace(text5, peEntity.ExtGetName());
							}
						}
						else if (j == 2 && content.Contains(text6))
						{
							PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(typeFollowData.m_iNpcList[j]);
							if (peEntity != null)
							{
								content = content.Replace(text6, peEntity.ExtGetName());
							}
						}
					}
				}
				else if (typeSearchData != null)
				{
					if (content.Contains(text3))
					{
						content = content.Replace(text3, typeSearchData.m_DistPos.ToString());
					}
					if (content.Contains(text4))
					{
						PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(typeSearchData.m_NpcID);
						if (peEntity != null)
						{
							content = content.Replace(text4, peEntity.ExtGetName());
						}
					}
				}
				else if (typeUseItemData != null)
				{
					if (content.Contains(text3))
					{
						content = content.Replace(text3, typeUseItemData.m_Pos.ToString());
					}
					if (content.Contains(text8))
					{
						content = content.Replace(text8, ItemProto.GetName(typeUseItemData.m_ItemID));
					}
					if (content.Contains(text9))
					{
						content = content.Replace(text9, typeUseItemData.m_UseNum.ToString());
					}
				}
				else
				{
					if (typeMessengerData == null)
					{
						continue;
					}
					if (content.Contains(text11))
					{
						PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(typeMessengerData.m_iNpc);
						if (peEntity != null)
						{
							content = content.Replace(text11, peEntity.ExtGetName());
						}
					}
					if (content.Contains(text12))
					{
						PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(typeMessengerData.m_iReplyNpc);
						if (peEntity != null)
						{
							content = content.Replace(text12, peEntity.ExtGetName());
						}
					}
					if (content.Contains(text8))
					{
						content = content.Replace(text8, ItemProto.GetName(typeMessengerData.m_ItemID));
					}
					if (content.Contains(text9))
					{
						content = content.Replace(text9, typeMessengerData.m_ItemNum.ToString());
					}
				}
			}
			if (data.m_Com_RewardItem.Count > 0)
			{
				if (content.Contains(text13))
				{
					content = content.Replace(text13, data.m_Com_RewardItem[0].num.ToString());
				}
				if (content.Contains(text14))
				{
					content = content.Replace(text14, ItemProto.GetName(data.m_Com_RewardItem[0].id));
				}
			}
			if (data.m_iColonyNpcList.Count > 1)
			{
				TalkData talkData = TalkRespository.GetTalkData(2174);
				if (talkData != null)
				{
					string text21 = " ";
					for (int k = 1; k < data.m_iColonyNpcList.Count; k++)
					{
						PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(data.m_iColonyNpcList[k]);
						if (!(peEntity == null) && !peEntity.IsRecruited())
						{
							text21 += peEntity.ExtGetName();
							if (k < data.m_iColonyNpcList.Count - 1)
							{
								text21 = ((k != data.m_iColonyNpcList.Count - 2) ? (text21 + ", ") : (text21 + "and "));
							}
						}
					}
					if (text21 != " ")
					{
						content += talkData.m_Content;
						content = content.Replace(oldValue, text21);
					}
				}
			}
		}
		if (PeGameMgr.IsAdventure)
		{
			if (content.Contains(value))
			{
				int num = content.IndexOf(value);
				if (content.Length >= num + 9 + 3)
				{
					string text22 = content.Substring(num + 9, 3);
					if (PEMath.IsNumeral(text22))
					{
						int key = Convert.ToInt32(text22);
						if (MissionManager.Instance.m_PlayerMission.adId_entityId.ContainsKey(key))
						{
							PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(MissionManager.Instance.m_PlayerMission.adId_entityId[key]);
							if (peEntity != null)
							{
								string oldValue2 = content.Substring(num, 12);
								string newValue2 = peEntity.name.Substring(0, peEntity.name.Length - 1 - Convert.ToString(peEntity.Id).Length);
								content = content.Replace(oldValue2, newValue2);
							}
						}
					}
				}
			}
			if (content.Contains(value2))
			{
				int num2 = content.IndexOf(value2);
				if (content.Length >= num2 + 7 + 3)
				{
					string text22 = content.Substring(num2 + 7, 3);
					if (PEMath.IsNumeral(text22))
					{
						int townId = Convert.ToInt32(text22);
						VArtifactUtil.GetTownName(townId, out var newValue3);
						content = content.Replace(content.Substring(num2, 10), newValue3);
					}
				}
			}
			if (content.Contains(value3))
			{
				int num3 = content.IndexOf(value3);
				if (content.Length >= num3 + 5 + 3)
				{
					content = content.Replace(content.Substring(num3, 8), "Puja");
				}
			}
		}
		if (content.Contains(text7))
		{
			content = content.Replace(text7, StroyManager.Instance.GetMgCampNpcCount().ToString());
		}
		if (content.Contains(text15))
		{
			content = content.Replace(text15, PeSingleton<PeCreature>.Instance.mainPlayer.ToString());
		}
		if (content.Contains(text16))
		{
			content = content.Replace(text16, RandomMapConfig.SeedString);
		}
		if (content.Contains(text17))
		{
			string tmp = string.Empty;
			StroyManager.deadNpcsName.ForEach(delegate(string s)
			{
				tmp = tmp + s + " ";
			});
			content = content.Replace(text17, tmp);
		}
		if (content.Contains(text18))
		{
			int num4 = content.IndexOf(text18);
			if (content.Length >= num4 + 15)
			{
				string text23 = content.Substring(num4 + 11, 4);
				if (PEMath.IsNumeral(text23))
				{
					PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(Convert.ToInt32(text23));
					if (peEntity != null)
					{
						content = content.Replace(text18 + text23, peEntity.name.Substring(0, peEntity.name.Length - 1 - Convert.ToString(peEntity.Id).Length));
					}
					else if (MissionManager.Instance.m_PlayerMission.recordNpcName.ContainsKey(Convert.ToInt32(text23)))
					{
						content = content.Replace(text18 + text23, MissionManager.Instance.m_PlayerMission.recordNpcName[Convert.ToInt32(text23)]);
					}
				}
			}
		}
		if (content.Contains(text19))
		{
			if (CSMain.GetAssemblyPos(out var v))
			{
				List<PeEntity> list = new List<PeEntity>(PeSingleton<EntityMgr>.Instance.All).FindAll((PeEntity e) => ((e.proto == EEntityProto.Npc || e.proto == EEntityProto.RandomNpc || e.proto == EEntityProto.Player) && Vector3.Distance(v, e.position) < 250f) ? true : false);
				content = content.Replace(text19, list.Count.ToString());
			}
			else
			{
				content = content.Replace(text19, "a few");
			}
		}
		if (content.Contains(text20))
		{
			string newValue4 = string.Empty;
			int playerID = Mathf.RoundToInt(PeSingleton<PeCreature>.Instance.mainPlayer.GetAttribute(AttribType.DefaultPlayerID));
			for (int l = 1; l < RandomMapConfig.allyCount; l++)
			{
				int playerId = VATownGenerator.Instance.GetPlayerId(l);
				if (PeSingleton<ReputationSystem>.Instance.GetReputationLevel(playerID, playerId) < ReputationSystem.ReputationLevel.Neutral)
				{
					newValue4 = PELocalization.GetString(VATownGenerator.Instance.GetAllyName(l));
					break;
				}
			}
			if (string.IsNullOrEmpty(text20))
			{
				for (int m = 0; m < RandomMapConfig.allyCount; m++)
				{
					if (VATownGenerator.Instance.GetAllyType(m) == AllyType.Npc)
					{
						newValue4 = PELocalization.GetString(VATownGenerator.Instance.GetAllyName(m));
						break;
					}
				}
			}
			content = content.Replace(text20, newValue4);
		}
		if (needLangSkill > MissionManager.Instance.m_PlayerMission.LanguegeSkill)
		{
			string value4 = "#";
			int num5 = needLangSkill - MissionManager.Instance.m_PlayerMission.LanguegeSkill;
			int num6 = content.Split(',', '.', '!', '?', '，', '。', '！', '？', '\'').Length - 1;
			int num7 = (content.Length - num6) * num5 / needLangSkill;
			System.Random random = new System.Random();
			if (!SystemSettingData.Instance.IsChinese)
			{
				Regex regex = new Regex("[A-Za-z]");
				if (MissionManager.Instance.m_PlayerMission.LanguegeSkill == 0)
				{
					for (int n = 0; n < content.Length; n++)
					{
						if (regex.IsMatch(Convert.ToString(content[n])))
						{
							content = content.Remove(n, 1);
							content = content.Insert(n, value4);
						}
					}
				}
				else
				{
					while (num7 > 0)
					{
						int num8 = random.Next(content.Length);
						if (regex.IsMatch(Convert.ToString(content[num8])))
						{
							content = content.Remove(num8, 1);
							content = content.Insert(num8, value4);
							num7--;
						}
					}
				}
			}
			else
			{
				Regex regex2 = new Regex("[一-龥]");
				if (MissionManager.Instance.m_PlayerMission.LanguegeSkill == 0)
				{
					for (int num9 = 0; num9 < content.Length; num9++)
					{
						if (regex2.IsMatch(Convert.ToString(content[num9])))
						{
							content = content.Remove(num9, 1);
							content = content.Insert(num9, value4);
						}
					}
				}
				else
				{
					while (num7 > 0)
					{
						int num8 = random.Next(content.Length);
						if (regex2.IsMatch(Convert.ToString(content[num8])))
						{
							content = content.Remove(num8, 1);
							content = content.Insert(num8, value4);
							num7--;
						}
					}
				}
			}
		}
		return content;
	}

	public void UpdateNpcTalkInfo(int MissionID, byte type, int targetid = 0, bool bFailed = false)
	{
		if (GameUI.Instance.mNpcWnd.isShow)
		{
			GameUI.Instance.mNpcWnd.Hide();
		}
		ClearNpcTalkInfos();
		MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(MissionID);
		if (missionCommonData == null)
		{
			Debug.LogError("MissionData Error: Couldn't find DataBlock");
			return;
		}
		List<int> list = GetTalkInfo(MissionID, type, missionCommonData, targetid, bFailed);
		if (list.Count != 0)
		{
			if (MissionID == 888 && PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<ServantLeaderCmpt>().GetServantNum() >= ServantLeaderCmpt.mMaxFollower)
			{
				list = new List<int>();
				list.Add(1573);
			}
			if (type > 5)
			{
				type -= 5;
			}
			MatchTalkInfo(list, missionCommonData, IsClearTalkList: true, MissionID, (MissionManager.TakeMissionType)type);
		}
	}

	public void AddNpcTalkInfo(int MissionID, byte type, int targetid = 0, bool bFailed = false, bool isClearTalkList = false)
	{
		if (GameUI.Instance.mNpcWnd.isShow)
		{
			GameUI.Instance.mNpcWnd.Hide();
		}
		MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(MissionID);
		if (missionCommonData == null)
		{
			Debug.LogError("MissionData Error: Couldn't find DataBlock");
			return;
		}
		List<int> talkInfo = GetTalkInfo(MissionID, type, missionCommonData, targetid, bFailed);
		if (talkInfo.Count != 0)
		{
			int num = type;
			num = ((num == 8) ? 3 : ((num <= 3) ? num : 0));
			MatchTalkInfo(talkInfo, missionCommonData, isClearTalkList, MissionID, (MissionManager.TakeMissionType)num);
		}
	}

	public void MatchTalkInfo(List<int> talklist, MissionCommonData data, bool IsClearTalkList = true, int triggerMission = -1, MissionManager.TakeMissionType type = MissionManager.TakeMissionType.TakeMissionType_Unkown)
	{
		for (int i = 0; i < talklist.Count; i++)
		{
			NpcTalkInfo talkinfo = default(NpcTalkInfo);
			GetTalkInfo(talklist[i], ref talkinfo, data);
			ParseName(data, ref talkinfo);
			if (i == talklist.Count - 1)
			{
				talkinfo.missionTrigger = triggerMission;
				talkinfo.type = type;
			}
			else
			{
				talkinfo.missionTrigger = -1;
				talkinfo.type = MissionManager.TakeMissionType.TakeMissionType_Unkown;
			}
			m_NpcTalkList.Add(talkinfo);
		}
		if (IsClearTalkList)
		{
			m_CurTalkIdx = 0;
		}
		m_bMutex = false;
		if (m_bMutex)
		{
			mTalkOnlyWnd.SetActive(value: false);
			return;
		}
		mTalkOnlyWnd.SetActive(value: true);
		if (m_NpcTalkList.Count > m_CurTalkIdx)
		{
			PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(m_NpcTalkList[m_CurTalkIdx].npcid);
			if (peEntity == null && data != null && MissionManager.HasRandomMission(data.m_ID))
			{
				if (PeGameMgr.IsStory)
				{
					peEntity = PeSingleton<EntityMgr>.Instance.Get(data.m_iNpc);
				}
				else if (PeGameMgr.IsAdventure)
				{
					peEntity = ((!MissionManager.Instance.m_PlayerMission.adId_entityId.ContainsKey(m_NpcTalkList[m_CurTalkIdx].npcid)) ? PeSingleton<EntityMgr>.Instance.Get(data.m_iNpc) : PeSingleton<EntityMgr>.Instance.Get(MissionManager.Instance.m_PlayerMission.adId_entityId[m_NpcTalkList[m_CurTalkIdx].npcid]));
				}
				if (peEntity != null)
				{
					for (int j = 0; j < m_NpcTalkList.Count; j++)
					{
						if (m_NpcTalkList[j].npcid == 0)
						{
							NpcTalkInfo value = m_NpcTalkList[j];
							value.npcid = peEntity.Id;
							m_NpcTalkList[j] = value;
						}
					}
				}
			}
			bool flag = true;
			switch (m_NpcTalkList[m_CurTalkIdx].npcid)
			{
			case -9998:
				mName.text = "Puja Commander";
				break;
			case -9997:
				mName.text = "Puja Soldier";
				break;
			case -9996:
				mName.text = "Tony";
				break;
			case -9995:
				mName.text = "Tips";
				break;
			case -9994:
				mName.text = "Paja";
				break;
			case -9993:
				mName.text = "Puja";
				break;
			case -9992:
			{
				if (!CSMain.HasCSAssembly())
				{
					break;
				}
				List<PeEntity> cSNpcs = CSMain.GetCSNpcs();
				if (cSNpcs.Count > 0)
				{
					SetNpcBigHeadSp(cSNpcs[0].ExtGetFaceIconBig());
					mName.text = cSNpcs[0].ExtGetName();
					break;
				}
				if (MissionManager.Instance.m_PlayerMission.adId_entityId.ContainsKey(1))
				{
					peEntity = PeSingleton<EntityMgr>.Instance.Get(MissionManager.Instance.m_PlayerMission.adId_entityId[1]);
				}
				if (peEntity != null)
				{
					mName.text = peEntity.ExtGetName();
					SetNpcBigHeadSp(peEntity.ExtGetFaceIconBig());
				}
				break;
			}
			case -9991:
				mName.text = "???";
				break;
			default:
				flag = false;
				break;
			}
			if (flag)
			{
				SetNpcBigHeadSp("npc_big_Unknown");
			}
			else if (peEntity != null)
			{
				mName.text = peEntity.ExtGetName();
				SetNpcBigHeadSp(peEntity.ExtGetFaceIconBig());
			}
			else
			{
				mName.text = "???";
				SetNpcBigHeadSp("npc_big_Unknown");
			}
			mContent.text = m_NpcTalkList[m_CurTalkIdx].desc;
			PeSingleton<NPCTalkHistroy>.Instance.AddHistroy(mName.text, mContent.text);
			InGameAidData.CheckNpcTalk(m_NpcTalkList[m_CurTalkIdx].talkid);
			if (null != GameUI.Instance)
			{
				GameUI.Instance.CheckTalkIDShowTutorial(m_NpcTalkList[m_CurTalkIdx].talkid);
			}
		}
		ClearMissionItem();
	}

	public void UpdateMoveNpc()
	{
		if (npcid_targetid.Count == 0)
		{
			return;
		}
		int num = 0;
		foreach (int key in npcid_targetid.Keys)
		{
			PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(key);
			PeEntity peEntity2 = PeSingleton<EntityMgr>.Instance.Get(npcid_targetid[key]);
			if (peEntity == null || peEntity2 == null || !(Vector3.Distance(peEntity.position, peEntity2.position) <= 7f))
			{
				continue;
			}
			num = key;
			NpcReachToTalk(peEntity);
			break;
		}
		if (num != 0)
		{
			npcid_targetid.Remove(num);
		}
	}

	public void NpcReachToTalk(PeEntity npc)
	{
		StroyManager.Instance.RemoveReq(npc, EReqType.TalkMove);
		if (needTalkNpc.ContainsKey(npc))
		{
			StroyManager.Instance.SetTalking(npc, (string)needTalkNpc[npc][0], needTalkNpc[npc][1]);
			needTalkNpc.Remove(npc);
		}
	}

	public void PreShow()
	{
		if (m_NpcTalkList.Count == 0)
		{
			return;
		}
		if (m_NpcTalkList[m_CurTalkIdx].moveToNpcidOrVecter3 is Vector3 || (m_NpcTalkList[m_CurTalkIdx].moveToNpcidOrVecter3 is int && (int)m_NpcTalkList[m_CurTalkIdx].moveToNpcidOrVecter3 != -1))
		{
			List<int> list = new List<int>();
			foreach (int item in m_NpcTalkList[m_CurTalkIdx].otherNpc)
			{
				list.Add(item);
			}
			if (m_NpcTalkList[m_CurTalkIdx].npcid > 0 && m_NpcTalkList[m_CurTalkIdx].npcid != PeSingleton<PeCreature>.Instance.mainPlayer.Id)
			{
				list.Add(m_NpcTalkList[m_CurTalkIdx].npcid);
			}
			List<int> list2 = new List<int>();
			bool flag = true;
			Vector3 vector = Vector3.zero;
			PeEntity peEntity = null;
			if (list.Count != 0)
			{
				if (m_NpcTalkList[m_CurTalkIdx].moveToNpcidOrVecter3 is Vector3)
				{
					vector = (Vector3)m_NpcTalkList[m_CurTalkIdx].moveToNpcidOrVecter3;
				}
				else if (m_NpcTalkList[m_CurTalkIdx].moveToNpcidOrVecter3 is int)
				{
					if ((int)m_NpcTalkList[m_CurTalkIdx].moveToNpcidOrVecter3 == 0 && PeSingleton<PeCreature>.Instance != null)
					{
						peEntity = PeSingleton<PeCreature>.Instance.mainPlayer;
					}
					else
					{
						PeEntity peEntity2 = PeSingleton<EntityMgr>.Instance.Get((int)m_NpcTalkList[m_CurTalkIdx].moveToNpcidOrVecter3);
						if (peEntity2 != null)
						{
							peEntity = peEntity2;
						}
					}
					if (peEntity != null)
					{
						vector = peEntity.position;
					}
				}
				if (vector == Vector3.zero || (vector.y < -5f && PeGameMgr.IsStory && (vector.x <= 0f || vector.z <= 0f)))
				{
					Debug.LogError("UINPCTalk: not found moveto entity!");
				}
				else
				{
					foreach (int item2 in list)
					{
						PeEntity peEntity2 = PeSingleton<EntityMgr>.Instance.Get(item2);
						if (!(peEntity2 == null))
						{
							if (Vector3.Distance(peEntity2.position, vector) > 13f)
							{
								flag = false;
							}
							else
							{
								list2.Add(item2);
							}
						}
					}
				}
			}
			if (!flag)
			{
				foreach (int item3 in list2)
				{
					if (list.Contains(item3))
					{
						list.Remove(item3);
					}
				}
				List<Vector3> meetingPosition = StroyManager.Instance.GetMeetingPosition(vector, list.Count, 5f);
				for (int i = 0; i < list.Count; i++)
				{
					PeEntity peEntity2 = PeSingleton<EntityMgr>.Instance.Get(list[i]);
					if (peEntity2 == null)
					{
						continue;
					}
					movingNpc.Add(peEntity2);
					object[] value = new object[2]
					{
						(!(m_NpcTalkList[m_CurTalkIdx].clip == "0")) ? m_NpcTalkList[m_CurTalkIdx].clip : string.Empty,
						m_NpcTalkList[m_CurTalkIdx].talkToNpcidOrVecter3
					};
					if (!needTalkNpc.ContainsKey(peEntity2))
					{
						needTalkNpc.Add(peEntity2, value);
					}
					StroyManager.Instance.TalkMoveTo(peEntity2, meetingPosition[i], 1f, bForce: true, (m_NpcTalkList[m_CurTalkIdx].moveType >= 2) ? ((SpeedState)m_NpcTalkList[m_CurTalkIdx].moveType) : SpeedState.Run);
					if (peEntity != null)
					{
						if (!npcid_targetid.ContainsKey(peEntity2.Id))
						{
							npcid_targetid.Add(peEntity2.Id, peEntity.Id);
						}
						else
						{
							npcid_targetid[peEntity2.Id] = peEntity.Id;
						}
					}
					if (peEntity2.NpcCmpt != null)
					{
						peEntity2.NpcCmpt.FixedPointPos = meetingPosition[i];
					}
				}
			}
		}
		Show();
	}

	public override void Show()
	{
		if (m_NpcTalkList.Count == 0)
		{
			return;
		}
		if (m_NpcTalkList[m_CurTalkIdx].missionTrigger == 888 && !CheckRandomNpc())
		{
			m_CurTalkNpc = GameUI.Instance.mNpcWnd.m_CurSelNpc;
			m_CurTalkNpc.CmdStartTalk();
			if (type == NormalOrSp.Normal)
			{
				StroyManager.Instance.SetTalking(m_CurTalkNpc, string.Empty);
			}
			base.Show();
			return;
		}
		ChangeNpcState(bStart: true);
		base.Show();
		if (m_bMutex)
		{
			if (m_CurTalkNpc == null)
			{
				m_CurTalkNpc = GameUI.Instance.mNpcWnd.m_CurSelNpc;
			}
			StroyManager.Instance.SetTalking(m_CurTalkNpc, string.Empty);
			mTalkOnlyWnd.SetActive(value: false);
			if (!mUpdateList)
			{
				mUpdateList = true;
				Invoke("UpdateList", 0.1f);
			}
			isPlayingTalk = true;
			return;
		}
		if (m_NpcTalkList.Count == 0)
		{
			Hide();
			return;
		}
		mTalkOnlyWnd.SetActive(value: true);
		mContent.text = m_NpcTalkList[m_CurTalkIdx].desc;
		InGameAidData.CheckNpcTalk(m_NpcTalkList[m_CurTalkIdx].talkid);
		if (null != GameUI.Instance)
		{
			GameUI.Instance.CheckTalkIDShowTutorial(m_NpcTalkList[m_CurTalkIdx].talkid);
		}
		PeEntity peEntity = ((m_NpcTalkList[m_CurTalkIdx].talkid != 1348) ? PeSingleton<EntityMgr>.Instance.Get(m_NpcTalkList[m_CurTalkIdx].npcid) : GameUI.Instance.mNpcWnd.m_CurSelNpc);
		if (peEntity != null)
		{
			mName.text = peEntity.ExtGetName();
			SetNpcBigHeadSp(peEntity.ExtGetFaceIconBig());
		}
		PeSingleton<NPCTalkHistroy>.Instance.AddHistroy(mName.text, mContent.text);
		peEntity = PeSingleton<EntityMgr>.Instance.Get(m_NpcTalkList[m_CurTalkIdx].npcid);
		if (PeGameMgr.IsAdventure && peEntity == null && MissionManager.Instance.m_PlayerMission.adId_entityId.ContainsKey(9901 + m_NpcTalkList[m_CurTalkIdx].npcid))
		{
			peEntity = PeSingleton<EntityMgr>.Instance.Get(MissionManager.Instance.m_PlayerMission.adId_entityId[9901 + m_NpcTalkList[m_CurTalkIdx].npcid]);
		}
		if (m_NpcTalkList[m_CurTalkIdx].npcid != 0)
		{
			isPlayingTalk = true;
		}
		if (peEntity == null)
		{
			if (type == NormalOrSp.SP || type == NormalOrSp.halfSP)
			{
				int length = m_NpcTalkList[m_CurTalkIdx].desc.Length;
				float time = ((length < 40) ? 2.5f : ((length < 80) ? 5f : ((length >= 120) ? 10f : 7.5f)));
				Invoke("SPTalkClose", time);
				mCloseBtn.SetActive(value: false);
			}
			else
			{
				mCloseBtn.SetActive(value: true);
			}
			return;
		}
		m_CurTalkNpc = peEntity;
		if (m_NpcTalkList[m_CurTalkIdx].isRadio)
		{
			ShowNpcMicroPhoneSp(_show: true);
		}
		else if (type == NormalOrSp.Normal || type == NormalOrSp.halfSP)
		{
			string text = m_NpcTalkList[m_CurTalkIdx].clip;
			if (m_NpcTalkList[m_CurTalkIdx].clip == "0")
			{
				System.Random random = new System.Random();
				text = random.Next(2) switch
				{
					0 => "Talk0", 
					1 => "Talk1", 
					_ => text, 
				};
			}
			if ((m_NpcTalkList[m_CurTalkIdx].talkToNpcidOrVecter3 is int || (m_NpcTalkList[m_CurTalkIdx].talkToNpcidOrVecter3 is Vector3 && (Vector3)m_NpcTalkList[m_CurTalkIdx].talkToNpcidOrVecter3 != Vector3.zero)) && !needTalkNpc.ContainsKey(m_CurTalkNpc))
			{
				StroyManager.Instance.SetTalking(m_CurTalkNpc, text, m_NpcTalkList[m_CurTalkIdx].talkToNpcidOrVecter3);
			}
			foreach (int item in m_NpcTalkList[m_CurTalkIdx].otherNpc)
			{
				PeEntity peEntity2 = PeSingleton<EntityMgr>.Instance.Get(item);
				if (!(peEntity2 == null) && !needTalkNpc.ContainsKey(peEntity2))
				{
					StroyManager.Instance.SetTalking(peEntity2, string.Empty, m_NpcTalkList[m_CurTalkIdx].talkToNpcidOrVecter3);
				}
			}
		}
		GameUI.Instance.PlayNpcTalkWndOpenAudioEffect();
		currentAudio = StroyManager.Instance.PlaySound(peEntity, m_NpcTalkList[m_CurTalkIdx].soundid);
		if (type == NormalOrSp.SP || type == NormalOrSp.halfSP)
		{
			if (!(currentAudio != null))
			{
				int length2 = m_NpcTalkList[m_CurTalkIdx].desc.Length;
				float time2 = ((length2 < 40) ? 2.5f : ((length2 < 80) ? 5f : ((length2 >= 120) ? 10f : 7.5f)));
				Invoke("SPTalkClose", time2);
			}
			mCloseBtn.SetActive(value: false);
		}
		else
		{
			mCloseBtn.SetActive(value: true);
		}
	}

	public bool CurTalkInfoIsRadio()
	{
		if (m_CurTalkIdx >= m_NpcTalkList.Count)
		{
			return false;
		}
		return m_NpcTalkList[m_CurTalkIdx].isRadio;
	}

	public void SPTalkClose()
	{
		canClose = true;
		OnClose();
	}

	public void SpTalkSymbol(bool spOrHalf)
	{
		if (!isPlayingTalk)
		{
			m_NpcTalkList.Clear();
		}
		NpcTalkInfo item = default(NpcTalkInfo);
		item.otherNpc = new List<int>();
		item.endOtherNpc = new List<int>();
		if (spOrHalf)
		{
			item.npcid = 9999;
		}
		else
		{
			item.npcid = 9998;
		}
		item.missionTrigger = -1;
		m_NpcTalkList.Add(item);
		m_NpcTalkList.Add(item);
	}

	public bool IsCanSkip()
	{
		if (type == NormalOrSp.Normal)
		{
			return true;
		}
		return false;
	}

	public void OpenmShowTuition()
	{
		mTuition.gameObject.SetActive(value: true);
	}

	public void OnMutexBtnClick(int missionId, string content)
	{
		ClearMissionItem();
		MissionManager.Instance.m_PlayerMission.SetMission(m_selectMissionSource);
		if (PeGameMgr.IsMulti)
		{
			PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_SetMission, m_selectMissionSource);
		}
		StroyManager.Instance.RemoveReq(m_CurTalkNpc, EReqType.Dialogue);
		if (null != PeSingleton<MainPlayer>.Instance.entity)
		{
			PeSingleton<NPCTalkHistroy>.Instance.AddHistroy(PeSingleton<MainPlayer>.Instance.entity.ExtGetName(), content);
		}
		if (MissionRepository.HaveTalkOP(missionId))
		{
			UpdateNpcTalkInfo(missionId, 1);
			if (m_NpcTalkList[0].npcid != -9999)
			{
				PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(m_NpcTalkList[0].npcid);
				if (peEntity == null)
				{
					Debug.LogError("npc is null");
					return;
				}
				peEntity.CmdFaceToPoint(PeSingleton<PeCreature>.Instance.mainPlayer.ExtGetPos());
				currentAudio = StroyManager.Instance.PlaySound(peEntity, m_NpcTalkList[m_CurTalkIdx].soundid);
			}
		}
		else
		{
			Hide();
			isPlayingTalk = false;
			MissionManager.Instance.SetGetTakeMission(missionId, m_CurTalkNpc, MissionManager.TakeMissionType.TakeMissionType_Get);
		}
	}

	protected override void OnClose()
	{
		mScrollBar.scrollValue = 0f;
		if ((type == NormalOrSp.SP || type == NormalOrSp.halfSP) && !canClose)
		{
			return;
		}
		if (m_NpcTalkList.Count <= m_CurTalkIdx)
		{
			OnHide();
			isPlayingTalk = false;
			return;
		}
		ShowNpcMicroPhoneSp(_show: false);
		canClose = false;
		if (currentAudio != null)
		{
			currentAudio.Delete();
			currentAudio = null;
			spTalkEndByAudioTime = false;
		}
		if (m_CurTalkNpc != null)
		{
			m_CurTalkNpc.CmdStopTalk();
			if (!GameUI.Instance.mNpcWnd.isShow && !GameUI.Instance.mShopWnd.isShopping)
			{
				StroyManager.Instance.RemoveReq(m_CurTalkNpc, EReqType.Dialogue);
			}
			if (needTalkNpc.ContainsKey(m_CurTalkNpc))
			{
				needTalkNpc.Remove(m_CurTalkNpc);
			}
		}
		foreach (int item in m_NpcTalkList[m_CurTalkIdx].endOtherNpc)
		{
			PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(item);
			if (!(peEntity == null))
			{
				StroyManager.Instance.RemoveReq(peEntity, EReqType.Dialogue);
				if (needTalkNpc.ContainsKey(peEntity))
				{
					needTalkNpc.Remove(peEntity);
				}
			}
		}
		if (m_bMutex)
		{
			return;
		}
		if (m_NpcTalkList.Count == 0)
		{
			isPlayingTalk = false;
			OnHide();
			return;
		}
		PlotLensAnimation.CheckIsStopCamera(m_NpcTalkList[m_CurTalkIdx].talkid);
		if (m_NpcTalkList.Count > m_CurTalkIdx && m_CurTalkIdx >= 0)
		{
			CheckTutorial(m_NpcTalkList[m_CurTalkIdx].talkid);
		}
		m_CurTalkIdx++;
		int num = -1;
		if (m_NpcTalkList[m_CurTalkIdx - 1].missionTrigger != -1)
		{
			foreach (PeEntity item2 in movingNpc)
			{
				StroyManager.Instance.RemoveReq(item2, EReqType.TalkMove);
			}
			movingNpc.Clear();
			needTalkNpc.Clear();
			npcid_targetid.Clear();
			if (PeSingleton<EntityMgr>.Instance.Get(m_NpcTalkList[m_CurTalkIdx - 1].npcid) != null)
			{
				MissionManager.Instance.SetGetTakeMission(m_NpcTalkList[m_CurTalkIdx - 1].missionTrigger, PeSingleton<EntityMgr>.Instance.Get(m_NpcTalkList[m_CurTalkIdx - 1].npcid), m_NpcTalkList[m_CurTalkIdx - 1].type);
			}
			else
			{
				if (m_CurTalkIdx >= m_NpcTalkList.Count)
				{
					if (m_CurTalkIdx - 1 < m_NpcTalkList.Count && m_CurTalkIdx >= 1)
					{
						num = m_NpcTalkList[m_CurTalkIdx - 1].npcid;
					}
				}
				else
				{
					num = m_NpcTalkList[m_CurTalkIdx].npcid;
				}
				if (num <= -9800 && num >= -9900 && MissionManager.Instance.m_PlayerMission.adId_entityId.ContainsKey(9901 + num))
				{
					num = MissionManager.Instance.m_PlayerMission.adId_entityId[9901 + num];
				}
				MissionManager.Instance.SetGetTakeMission(m_NpcTalkList[m_CurTalkIdx - 1].missionTrigger, PeSingleton<EntityMgr>.Instance.Get(num), m_NpcTalkList[m_CurTalkIdx - 1].type);
			}
		}
		int num2 = -1;
		if (m_CurTalkIdx >= m_NpcTalkList.Count)
		{
			movingNpc.Clear();
			needTalkNpc.Clear();
			npcid_targetid.Clear();
			isPlayingTalk = false;
			type = NormalOrSp.Normal;
			ChangeNpcState(bStart: false);
			OnHide();
			isPlayingTalk = false;
			if (m_CurTalkIdx - 1 < m_NpcTalkList.Count && m_CurTalkIdx >= 1)
			{
				num2 = m_NpcTalkList[m_CurTalkIdx - 1].npcid;
				m_CurTalkNpc = PeSingleton<EntityMgr>.Instance.Get(num2);
			}
			int missionTrigger = m_NpcTalkList[m_CurTalkIdx - 1].missionTrigger;
			MissionManager.TakeMissionType takeMissionType = m_NpcTalkList[m_CurTalkIdx - 1].type;
			GameUI.Instance.mNpcWnd.GetMutexID(missionTrigger, ref m_SelectMissionList);
			if (m_SelectMissionList.Count > 0)
			{
				m_bMutex = true;
				ResetMissionItem();
				m_CurTalkIdx = 0;
				GameUI.Instance.mNPCTalk.Show();
				if (!PeGameMgr.IsAdventure)
				{
					return;
				}
				for (int i = 0; i < m_SelectMissionList.Count; i++)
				{
					int num3 = num;
					if (num3 <= 0)
					{
						num3 = num2;
					}
					if (PeGameMgr.IsMulti)
					{
						PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_NewMission, m_SelectMissionList[i], num3);
					}
					else
					{
						AdRMRepository.CreateRandomMission(m_SelectMissionList[i]);
					}
				}
				return;
			}
			if (m_CurTalkNpc == null)
			{
				m_CurTalkNpc = GameUI.Instance.mNpcWnd.m_CurSelNpc;
				if (m_CurTalkNpc == null)
				{
					return;
				}
			}
			if (needTalkNpc.ContainsKey(m_CurTalkNpc))
			{
				needTalkNpc.Remove(m_CurTalkNpc);
			}
			if (MissionManager.IsTalkMission(missionTrigger) && missionTrigger != 505 && missionTrigger != 506 && missionTrigger != 888 && PeGameMgr.IsSingle)
			{
				MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(missionTrigger);
				if (missionCommonData.m_StoryInfo.Count <= 0 && (takeMissionType != MissionManager.TakeMissionType.TakeMissionType_Get || (missionCommonData.m_OPID.Count <= 0 && missionCommonData.m_EDID.Count <= 0)) && (takeMissionType != MissionManager.TakeMissionType.TakeMissionType_Complete || missionCommonData.m_EDID.Count <= 0) && MissionManager.IsTalkMission(missionTrigger) && missionTrigger != 191)
				{
					GameUI.Instance.mNpcWnd.ChangeHeadTex(m_CurTalkNpc);
					GameUI.Instance.mNpcWnd.SetCurSelNpc(m_CurTalkNpc);
					GameUI.Instance.mNpcWnd.Show();
				}
			}
			m_CurTalkIdx = 0;
			return;
		}
		while (m_NpcTalkList[m_CurTalkIdx].npcid == 9999 || m_NpcTalkList[m_CurTalkIdx].npcid == 9998)
		{
			canClose = false;
			if (m_NpcTalkList[m_CurTalkIdx].npcid == 9999)
			{
				type = NormalOrSp.SP;
			}
			else
			{
				type = NormalOrSp.halfSP;
			}
			m_CurTalkIdx++;
		}
		if (m_CurTalkIdx >= m_NpcTalkList.Count)
		{
			return;
		}
		num2 = m_NpcTalkList[m_CurTalkIdx].npcid;
		mContent.text = m_NpcTalkList[m_CurTalkIdx].desc;
		InGameAidData.CheckNpcTalk(m_NpcTalkList[m_CurTalkIdx].talkid);
		if (null != GameUI.Instance)
		{
			GameUI.Instance.CheckTalkIDShowTutorial(m_NpcTalkList[m_CurTalkIdx].talkid);
		}
		m_CurTalkNpc = PeSingleton<EntityMgr>.Instance.Get(num2);
		if (m_CurTalkNpc == null)
		{
			SetNpcBigHeadSp("npc_big_Unknown");
			switch (num2)
			{
			case -9998:
				mName.text = "Puja Commander";
				break;
			case -9997:
				mName.text = "Puja Soldier";
				break;
			case -9996:
				mName.text = "Tony";
				break;
			case -9995:
				mName.text = "Tips";
				break;
			case -9994:
				mName.text = "Paja";
				break;
			case -9993:
				mName.text = "Puja";
				break;
			case -9992:
			{
				if (!CSMain.HasCSAssembly())
				{
					break;
				}
				List<PeEntity> cSNpcs = CSMain.GetCSNpcs();
				if (cSNpcs.Count > 0)
				{
					mName.text = cSNpcs[0].ExtGetName();
					break;
				}
				PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(MissionManager.Instance.m_PlayerMission.adId_entityId[1]);
				if (peEntity != null)
				{
					mName.text = peEntity.ExtGetName();
				}
				break;
			}
			case -9991:
				mName.text = "???";
				break;
			default:
				if (num2 <= -9800 && num2 >= -9900 && MissionManager.Instance.m_PlayerMission.adId_entityId.ContainsKey(9901 + num2))
				{
					PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(MissionManager.Instance.m_PlayerMission.adId_entityId[9901 + num2]);
					if (peEntity != null)
					{
						mName.text = peEntity.ExtGetName();
					}
				}
				break;
			}
		}
		else
		{
			mName.text = m_CurTalkNpc.ExtGetName();
			SetNpcBigHeadSp(m_CurTalkNpc.ExtGetFaceIconBig());
		}
		PeSingleton<NPCTalkHistroy>.Instance.AddHistroy(mName.text, mContent.text);
		GameUI.Instance.mNPCTalk.PreShow();
	}

	private void ResetMissionItem()
	{
		ClearMissionItem();
		for (int i = 0; i < m_SelectMissionList.Count; i++)
		{
			MissionSelItem_N missionSelItem_N = UnityEngine.Object.Instantiate(mPrefab);
			missionSelItem_N.gameObject.name = "MissionItem" + i;
			missionSelItem_N.transform.parent = mUITable.transform;
			missionSelItem_N.transform.localPosition = Vector3.zero;
			missionSelItem_N.transform.localRotation = Quaternion.identity;
			missionSelItem_N.transform.localScale = Vector3.one;
			missionSelItem_N.SetMission(m_SelectMissionList[i], this);
			missionSelItem_N.ActiveMask();
			mSelList.Add(missionSelItem_N);
		}
		if (m_SelectMissionList.Count == 1)
		{
			string missionNpcListName = MissionRepository.GetMissionNpcListName(m_SelectMissionList[0], bspe: false);
			if (null != m_CurTalkNpc && string.IsNullOrEmpty(missionNpcListName))
			{
				PeSingleton<NPCTalkHistroy>.Instance.AddHistroy(mName.text, missionNpcListName);
			}
		}
		mUITable.repositionNow = true;
	}

	private void ClearMissionItem()
	{
		for (int num = mSelList.Count - 1; num >= 0; num--)
		{
			mSelList[num].transform.parent = null;
			UnityEngine.Object.Destroy(mSelList[num].gameObject);
		}
		mSelList.Clear();
		mUITable.Reposition();
	}

	public void NormalOrSP(int type)
	{
		this.type = (NormalOrSp)type;
	}

	private void OnBgClick()
	{
		if (!m_bMutex)
		{
			OnClose();
		}
	}

	private void Update()
	{
		UpdateMoveNpc();
		if (!m_bMutex && PeInput.Get(PeInput.LogicFunction.UI_SkipDialog1) && type == NormalOrSp.Normal)
		{
			OnClose();
		}
		if (currentAudio != null && (type == NormalOrSp.SP || type == NormalOrSp.halfSP) && !spTalkEndByAudioTime && currentAudio.length != 0f)
		{
			Invoke("SPTalkClose", currentAudio.length);
			spTalkEndByAudioTime = true;
		}
		if (mNpcBigHeadSp.spriteName == "A" || mNpcBigHeadSp.spriteName.Length <= 1)
		{
			SetNpcBigHeadSp("npc_big_Unknown");
		}
		if (mNpcBigHeadSp.spriteName == "npc_big_Unknown")
		{
			if (m_CurTalkIdx > -1 && m_CurTalkIdx < m_NpcTalkList.Count)
			{
				if (m_NpcTalkList[m_CurTalkIdx].npcicon == null)
				{
					PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(m_NpcTalkList[m_CurTalkIdx].npcid);
					if (PeGameMgr.IsSingle)
					{
						if (peEntity == null && MissionManager.Instance.m_PlayerMission.adId_entityId.ContainsKey(9901 + m_NpcTalkList[m_CurTalkIdx].npcid))
						{
							peEntity = PeSingleton<EntityMgr>.Instance.Get(MissionManager.Instance.m_PlayerMission.adId_entityId[9901 + m_NpcTalkList[m_CurTalkIdx].npcid]);
						}
					}
					else if (peEntity == null && MissionManager.Instance.m_PlayerMission.adId_entityId.ContainsKey(9901 + m_NpcTalkList[m_CurTalkIdx].npcid))
					{
						peEntity = PeSingleton<EntityMgr>.Instance.Get(MissionManager.Instance.m_PlayerMission.adId_entityId[9901 + m_NpcTalkList[m_CurTalkIdx].npcid]);
					}
					if (peEntity == null && m_NpcTalkList[m_CurTalkIdx].npcid == -9992 && CSMain.HasCSAssembly())
					{
						List<PeEntity> cSNpcs = CSMain.GetCSNpcs();
						peEntity = ((cSNpcs.Count <= 0) ? PeSingleton<EntityMgr>.Instance.Get(MissionManager.Instance.m_PlayerMission.adId_entityId[1]) : cSNpcs[0]);
					}
					if (peEntity != null && peEntity.proto != EEntityProto.Npc)
					{
						BiologyViewCmpt biologyViewCmpt = peEntity.biologyViewCmpt;
						NpcTalkInfo value = m_NpcTalkList[m_CurTalkIdx];
						value.npcicon = PeViewStudio.TakePhoto(biologyViewCmpt, 150, 150, PeViewStudio.s_HeadPhotoPos, PeViewStudio.s_HeadPhotoRot);
						mName.text = peEntity.ExtGetName();
						m_NpcTalkList[m_CurTalkIdx] = value;
						if (null == m_NpcTalkList[m_CurTalkIdx].npcicon)
						{
							mNpcBigHeadSp.gameObject.SetActive(value: true);
							mNpcBigHeadTex.gameObject.SetActive(value: false);
						}
						else
						{
							mNpcBigHeadSp.gameObject.SetActive(value: false);
							mNpcBigHeadTex.gameObject.SetActive(value: true);
							mNpcBigHeadTex.mainTexture = m_NpcTalkList[m_CurTalkIdx].npcicon;
						}
						mNpcBigHeadTex.gameObject.SetActive(value: true);
					}
					else
					{
						mNpcBigHeadSp.gameObject.SetActive(value: true);
						mNpcBigHeadTex.gameObject.SetActive(value: false);
					}
				}
				else
				{
					mNpcBigHeadSp.gameObject.SetActive(value: false);
					mNpcBigHeadTex.gameObject.SetActive(value: true);
				}
			}
		}
		else
		{
			mNpcBigHeadSp.gameObject.SetActive(value: true);
			mNpcBigHeadTex.gameObject.SetActive(value: false);
		}
		mDragCollider.enabled = mScrollBar.foreground.gameObject.activeSelf;
	}

	private void UpdateList()
	{
		mUpdateList = false;
		mUITable.Reposition();
	}

	private bool CheckRandomNpc()
	{
		bool flag = true;
		NpcMissionData npcMissionData = GameUI.Instance.mNpcWnd.m_CurSelNpc.GetUserData() as NpcMissionData;
		if (npcMissionData != null)
		{
			if (MissionManager.Instance.HasMission(997) && npcMissionData.m_MissionList.Contains(997))
			{
				flag = false;
			}
			if (MissionManager.Instance.HasMission(998) && npcMissionData.m_MissionList.Contains(998))
			{
				flag = false;
			}
			if (MissionManager.Instance.HasMission(999) && npcMissionData.m_MissionList.Contains(999))
			{
				flag = false;
			}
		}
		if (!flag)
		{
			GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(889, 1);
			return false;
		}
		if (npcMissionData != null && npcMissionData.mCurComMisNum < npcMissionData.mCompletedMissionCount && !m_QuickZM)
		{
			UpdateNpcTalkInfo(889, 1);
			return false;
		}
		return true;
	}

	private void ChangeNpcState(bool bStart)
	{
		for (int i = 0; i < m_NpcTalkList.Count; i++)
		{
			int npcid = m_NpcTalkList[i].npcid;
			m_CurTalkNpc = PeSingleton<EntityMgr>.Instance.Get(npcid);
			if (!(m_CurTalkNpc == null))
			{
				if (bStart)
				{
					m_CurTalkNpc.CmdStartIdle();
				}
				else
				{
					m_CurTalkNpc.CmdStopIdle();
				}
			}
		}
	}

	private bool CheckTutorial(int id)
	{
		switch (id)
		{
		case 1261:
			TutorialData.AddActiveTutorialID(3);
			TutorialData.AddActiveTutorialID(2);
			TutorialData.AddActiveTutorialID(1);
			TutorialData.AddActiveTutorialID(16);
			TutorialData.AddActiveTutorialID(12);
			TutorialData.AddActiveTutorialID(11);
			GameUI.Instance.mPhoneWnd.mUIHelp.ChangeSelect(3);
			GameUI.Instance.mPhoneWnd.Show();
			return true;
		case 19:
			TutorialData.AddActiveTutorialID(3);
			TutorialData.AddActiveTutorialID(2);
			TutorialData.AddActiveTutorialID(1);
			TutorialData.AddActiveTutorialID(16);
			TutorialData.AddActiveTutorialID(12);
			TutorialData.AddActiveTutorialID(11);
			GameUI.Instance.mPhoneWnd.mUIHelp.ChangeSelect(3);
			GameUI.Instance.mPhoneWnd.Hide();
			return true;
		case 1490:
			TutorialData.AddActiveTutorialID(4, execEvent: false);
			TutorialData.AddActiveTutorialID(20);
			break;
		case 1491:
			TutorialData.AddActiveTutorialID(5);
			break;
		case 1713:
			TutorialData.AddActiveTutorialID(6);
			break;
		case 1492:
			TutorialData.AddActiveTutorialID(7);
			break;
		case 1997:
		{
			for (int i = 17; i < 20; i++)
			{
				TutorialData.AddActiveTutorialID(i, execEvent: false);
			}
			TutorialData.AddActiveTutorialID(10);
			break;
		}
		case 2273:
			TutorialData.AddActiveTutorialID(13);
			TutorialData.AddActiveTutorialID(14);
			GameUI.Instance.mPhoneWnd.mUIHelp.ChangeSelect(13);
			return true;
		}
		return false;
	}
}
