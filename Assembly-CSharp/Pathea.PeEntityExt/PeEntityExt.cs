using AppearBlendShape;
using CustomCharactor;
using UnityEngine;

namespace Pathea.PeEntityExt;

public static class PeEntityExt
{
	public static void SetViewModelPath(this PeEntity entity, string value)
	{
		if (!(null == entity))
		{
			BiologyViewCmpt biologyViewCmpt = entity.biologyViewCmpt;
			if (!(null == biologyViewCmpt))
			{
				biologyViewCmpt.SetViewPath(value);
			}
		}
	}

	public static void SetAvatarNpcModelPath(this PeEntity entity, string value)
	{
		if (!(null == entity))
		{
			AvatarCmpt cmpt = entity.GetCmpt<AvatarCmpt>();
			if (!(null == cmpt))
			{
				AvatarData avatarData = new AvatarData();
				avatarData.SetPart(AvatarData.ESlot.HairF, value);
				cmpt.SetData(new AppearData(), avatarData);
			}
		}
	}

	public static void ExtSetSex(this PeEntity entity, PeSex sex)
	{
		CommonCmpt cmpt = entity.GetCmpt<CommonCmpt>();
		if (!(null == cmpt))
		{
			cmpt.sex = sex;
		}
	}

	public static void ExtSetVoiceType(this PeEntity entity, int voiceType)
	{
		NpcCmpt cmpt = entity.GetCmpt<NpcCmpt>();
		if (!(null == cmpt))
		{
			cmpt.voiceType = voiceType;
		}
	}

	public static PeSex ExtGetSex(this PeEntity entity)
	{
		CommonCmpt cmpt = entity.GetCmpt<CommonCmpt>();
		if (null == cmpt)
		{
			return PeSex.Max;
		}
		return cmpt.sex;
	}

	public static void ExtSetFaceIcon(this PeEntity entity, string icon)
	{
		EntityInfoCmpt cmpt = entity.GetCmpt<EntityInfoCmpt>();
		if (!(null == cmpt))
		{
			cmpt.faceIcon = icon;
		}
	}

	public static string ExtGetFaceIcon(this PeEntity entity)
	{
		EntityInfoCmpt cmpt = entity.GetCmpt<EntityInfoCmpt>();
		if (null == cmpt)
		{
			return null;
		}
		return (!string.IsNullOrEmpty(cmpt.faceIcon)) ? cmpt.faceIcon : "npc_big_Unknown";
	}

	public static void ExtSetFaceIconBig(this PeEntity entity, string icon)
	{
		EntityInfoCmpt cmpt = entity.GetCmpt<EntityInfoCmpt>();
		if (!(null == cmpt))
		{
			cmpt.faceIconBig = icon;
		}
	}

	public static string ExtGetFaceIconBig(this PeEntity entity)
	{
		EntityInfoCmpt cmpt = entity.GetCmpt<EntityInfoCmpt>();
		if (null == cmpt)
		{
			return null;
		}
		return (!string.IsNullOrEmpty(cmpt.faceIconBig)) ? cmpt.faceIconBig : "npc_big_Unknown";
	}

	public static void ExtSetFaceTex(this PeEntity entity, Texture icon)
	{
		EntityInfoCmpt cmpt = entity.GetCmpt<EntityInfoCmpt>();
		if (!(null == cmpt))
		{
			cmpt.faceTex = icon;
		}
	}

	public static Texture ExtGetFaceTex(this PeEntity entity)
	{
		EntityInfoCmpt cmpt = entity.GetCmpt<EntityInfoCmpt>();
		if (null == cmpt)
		{
			return null;
		}
		return cmpt.faceTex;
	}

	public static void SetShopIcon(this PeEntity entity, string icon)
	{
		EntityInfoCmpt cmpt = entity.GetCmpt<EntityInfoCmpt>();
		if (!(null == cmpt))
		{
			cmpt.shopIcon = icon;
		}
	}

	public static void ExtSetName(this PeEntity entity, CharacterName name)
	{
		EntityInfoCmpt cmpt = entity.GetCmpt<EntityInfoCmpt>();
		if (!(null == cmpt))
		{
			cmpt.characterName = name;
		}
	}

	public static string ExtGetName(this PeEntity entity)
	{
		EntityInfoCmpt cmpt = entity.GetCmpt<EntityInfoCmpt>();
		if (null == cmpt)
		{
			return null;
		}
		return cmpt.characterName.fullName;
	}

	public static void SetAvatarData(this PeEntity entity, AppearData appearData, AvatarData nudeAvatarData)
	{
		AvatarCmpt cmpt = entity.GetCmpt<AvatarCmpt>();
		if (null != cmpt)
		{
			cmpt.SetData(appearData, nudeAvatarData);
		}
	}

	public static void TrapInSpiderWeb(this PeEntity entity, bool btrue, float delayTime)
	{
	}

	public static void SetAiActive(this PeEntity entity, bool value)
	{
	}

	public static void SetCamp(this PeEntity entity, int iCamp)
	{
	}

	public static void SetState(this PeEntity entity, NpcMissionState state)
	{
		EntityInfoCmpt cmpt = entity.GetCmpt<EntityInfoCmpt>();
		if (cmpt != null)
		{
			cmpt.SetMissionState(state);
		}
	}

	public static void SetInjuredLevel(this PeEntity entity, float injuredLevel)
	{
	}

	public static void ApplyDamage(this PeEntity entity, float damage)
	{
	}

	public static bool IsInvincible(this PeEntity entity)
	{
		CommonCmpt cmpt = entity.GetCmpt<CommonCmpt>();
		if (cmpt == null)
		{
			return false;
		}
		return cmpt.invincible;
	}

	public static void SetInvincible(this PeEntity entity, bool value)
	{
		CommonCmpt cmpt = entity.GetCmpt<CommonCmpt>();
		if (!(cmpt == null))
		{
			cmpt.invincible = value;
		}
	}

	public static bool IsDead(this PeEntity entity)
	{
		SkAliveEntity cmpt = entity.GetCmpt<SkAliveEntity>();
		if (cmpt == null)
		{
			return false;
		}
		return cmpt.isDead;
	}

	public static bool IsOnCarrier(this PeEntity entity)
	{
		PassengerCmpt cmpt = entity.GetCmpt<PassengerCmpt>();
		if (cmpt == null)
		{
			return false;
		}
		return cmpt.IsOnCarrier();
	}

	public static void GetOffCarrier(this PeEntity entity)
	{
		PassengerCmpt cmpt = entity.GetCmpt<PassengerCmpt>();
		if (!(cmpt == null))
		{
			cmpt.GetOffCarrier();
		}
	}

	public static bool IsRandomNpc(this PeEntity entity)
	{
		if (entity == null)
		{
			return false;
		}
		CommonCmpt cmpt = entity.GetCmpt<CommonCmpt>();
		if (cmpt == null)
		{
			return false;
		}
		return cmpt.entityProto.proto == EEntityProto.RandomNpc;
	}

	public static bool IsRecruited(this PeEntity entity)
	{
		if (CSMain.IsColonyNpc(entity.Id))
		{
			return true;
		}
		return false;
	}

	public static bool Recruit(this PeEntity entity)
	{
		return false;
	}

	public static bool Dismiss(this PeEntity entity)
	{
		return false;
	}

	public static EAttackMode GetAttackMode(this PeEntity entity)
	{
		return EAttackMode.Max;
	}

	public static void SetAttackMode(this PeEntity entity, EAttackMode mode)
	{
	}

	public static object GetUserData(this PeEntity entity)
	{
		if (entity == null)
		{
			return null;
		}
		CommonCmpt cmpt = entity.GetCmpt<CommonCmpt>();
		if (cmpt == null)
		{
			return null;
		}
		return cmpt.userData;
	}

	public static void SetUserData(this PeEntity entity, object obj)
	{
		if (!(entity == null))
		{
			CommonCmpt cmpt = entity.GetCmpt<CommonCmpt>();
			if (!(cmpt == null))
			{
				cmpt.userData = obj;
				CompatibleMissionData(entity);
			}
		}
	}

	private static void CompatibleMissionData(PeEntity entity)
	{
		if (entity == null || !entity.commonCmpt || !(entity.commonCmpt.userData is NpcMissionData npcMissionData))
		{
			return;
		}
		if (entity.Id == 9007)
		{
			if (!npcMissionData.m_MissionList.Contains(10054))
			{
				npcMissionData.m_MissionList.Add(10054);
			}
			if (!npcMissionData.m_MissionList.Contains(10055))
			{
				npcMissionData.m_MissionList.Add(10055);
			}
		}
		if (entity.Id == 9041)
		{
			if (!npcMissionData.m_MissionList.Contains(10053))
			{
				npcMissionData.m_MissionList.Add(10053);
			}
			if (!npcMissionData.m_MissionListReply.Contains(10053))
			{
				npcMissionData.m_MissionListReply.Add(10053);
			}
		}
		if (entity.Id == 9037)
		{
			if (!npcMissionData.m_MissionList.Contains(10055))
			{
				npcMissionData.m_MissionList.Add(10055);
			}
			if (!npcMissionData.m_MissionList.Contains(10056))
			{
				npcMissionData.m_MissionList.Add(10056);
			}
			if (!npcMissionData.m_MissionList.Contains(10057))
			{
				npcMissionData.m_MissionList.Add(10057);
			}
			if (!npcMissionData.m_MissionList.Contains(10058))
			{
				npcMissionData.m_MissionList.Add(10058);
			}
			if (!npcMissionData.m_MissionList.Contains(10059))
			{
				npcMissionData.m_MissionList.Add(10059);
			}
			if (!npcMissionData.m_MissionList.Contains(10060))
			{
				npcMissionData.m_MissionList.Add(10060);
			}
			if (!npcMissionData.m_MissionList.Contains(10061))
			{
				npcMissionData.m_MissionList.Add(10061);
			}
			if (!npcMissionData.m_MissionList.Contains(10066))
			{
				npcMissionData.m_MissionList.Add(10066);
			}
		}
	}

	public static void CmdStartIdle(this PeEntity entity)
	{
	}

	public static void CmdStopIdle(this PeEntity entity)
	{
	}

	public static bool GetTalkEnable(this PeEntity entity)
	{
		return true;
	}

	public static void CmdStartTalk(this PeEntity entity)
	{
	}

	public static void CmdStopTalk(this PeEntity entity)
	{
	}

	public static bool IsTalking(this PeEntity entity)
	{
		return false;
	}

	public static bool CmdPlayAnimation(this PeEntity entity, string parameter, float autoReturnTime)
	{
		return false;
	}

	public static bool CmdPlayAnimation(this PeEntity entity, string parameter, bool flag)
	{
		return false;
	}

	public static bool IsFollower(this PeEntity entity)
	{
		if (entity.NpcCmpt == null)
		{
			return false;
		}
		if (PeGameMgr.IsMulti && (entity.GetComponent<NpcCmpt>().Net as AiAdNpcNetwork).bForcedServant)
		{
			return false;
		}
		return entity.GetComponent<NpcCmpt>().IsServant;
	}

	public static bool SetFollower(this PeEntity entity, bool bFlag, int index = -1)
	{
		ServantLeaderCmpt cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<ServantLeaderCmpt>();
		NpcCmpt npcCmpt = entity.NpcCmpt;
		if (!bFlag)
		{
			return cmpt.RemoveServant(npcCmpt);
		}
		entity.NpcCmpt.AddTalkInfo(ENpcTalkType.Conscribe_succeed, ENpcSpeakType.TopHead);
		if (index == -1)
		{
			return cmpt.AddServant(npcCmpt);
		}
		return cmpt.AddServant(npcCmpt, index);
	}

	public static void GetOnTrain(this PeEntity entity, int id, bool checkState = true)
	{
		PassengerCmpt cmpt = entity.GetCmpt<PassengerCmpt>();
		if (!(cmpt == null))
		{
			cmpt.GetOn(id, checkState);
		}
	}

	public static void GetOffTrain(this PeEntity entity, Vector3 pos)
	{
		PassengerCmpt cmpt = entity.GetCmpt<PassengerCmpt>();
		if (!(cmpt == null))
		{
			cmpt.GetOff(pos);
		}
	}

	public static bool IsOnTrain(this PeEntity entity)
	{
		PassengerCmpt cmpt = entity.GetCmpt<PassengerCmpt>();
		if (cmpt == null)
		{
			return false;
		}
		return cmpt.IsOnRail;
	}

	public static void SayHiRandom(this PeEntity entity)
	{
	}

	public static void SayByeRandom(this PeEntity entity)
	{
	}
}
