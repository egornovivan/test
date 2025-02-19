using System.Collections;
using System.IO;
using Pathea;
using Pathea.PeEntityExt;
using UnityEngine;

public class MisRepositoryArchiveMgr : ArchivableSingleton<MisRepositoryArchiveMgr>
{
	private const string ArchiveKey = "ArchiveKeyMisRepository";

	private static readonly int[] equipment = new int[11]
	{
		97, 115, 133, 151, 75, 85, 172, 181, 194, 212,
		55
	};

	protected override bool GetYird()
	{
		return false;
	}

	protected override string GetArchiveKey()
	{
		return "ArchiveKeyMisRepository";
	}

	public override void New()
	{
		base.New();
		if (PeGameMgr.IsStory)
		{
			if (PeSingleton<PeCreature>.Instance == null || PeSingleton<PeCreature>.Instance.mainPlayer == null)
			{
				Debug.LogError("storymode error,cant load missionmgr");
				GlobalBehaviour.RegisterEvent(WaitForAssets);
				return;
			}
			MotionMgrCmpt motionMgr = PeSingleton<PeCreature>.Instance.mainPlayer.motionMgr;
			if (!(motionMgr == null))
			{
				motionMgr.DoAction(PEActionType.Lie);
				if (PeGameMgr.IsMultiStory)
				{
					MissionManager.Instance.Invoke("MultiGetUp", 5f);
				}
				GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(1, 1);
				GameUI.Instance.mNPCTalk.PreShow();
			}
		}
		else if (PeGameMgr.IsSingleAdventure)
		{
			MissionManager.Instance.StartCoroutine(AdventureInit());
		}
	}

	private IEnumerator AdventureInit()
	{
		while (VArtifactTownManager.Instance == null)
		{
			yield return 0;
		}
		while (VArtifactTownManager.Instance.missionStartNpcEntityId < 0)
		{
			yield return 0;
		}
		while (!PeSingleton<EntityMgr>.Instance.Get(VArtifactTownManager.Instance.missionStartNpcEntityId))
		{
			yield return 0;
		}
		PeEntity npc = PeSingleton<EntityMgr>.Instance.Get(VArtifactTownManager.Instance.missionStartNpcEntityId);
		GameUI.Instance.mNpcWnd.m_CurSelNpc = npc;
		GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(9027, 1);
		GameUI.Instance.mNPCTalk.PreShow();
		MissionManager.Instance.m_PlayerMission.UpdateAllNpcMisTex();
		AdventureCampNpc();
	}

	private void AdventureCampNpc()
	{
		for (int i = 1; i < RandomMapConfig.allyCount; i++)
		{
			AllyType allyType = VATownGenerator.Instance.GetAllyType(i);
			PeEntity peEntity = null;
			NpcMissionData npcMissionData = new NpcMissionData();
			switch (allyType)
			{
			case AllyType.Puja:
				peEntity = PeSingleton<PeCreature>.Instance.CreateNpc(20000 - i, 68, Vector3.zero, Quaternion.identity, Vector3.one);
				break;
			case AllyType.Paja:
				peEntity = PeSingleton<PeCreature>.Instance.CreateNpc(20000 - i, 69, Vector3.zero, Quaternion.identity, Vector3.one);
				break;
			case AllyType.Npc:
				peEntity = PeSingleton<PeCreature>.Instance.CreateNpc(20000 - i, 70, Vector3.zero, Quaternion.identity, Vector3.one);
				PeEntityCreator.InitEquipment(peEntity, equipment);
				break;
			default:
				continue;
			}
			int playerId = VATownGenerator.Instance.GetPlayerId(i);
			peEntity.SetAttribute(AttribType.DefaultPlayerID, playerId);
			string @string = PELocalization.GetString(VATownGenerator.Instance.GetAllyName(i));
			peEntity.ExtSetName(new CharacterName(@string));
			npcMissionData.m_MissionList.Add(9135);
			npcMissionData.m_MissionList.Add(9136);
			npcMissionData.m_MissionList.Add(9137);
			npcMissionData.m_MissionList.Add(9138);
			npcMissionData.m_MissionListReply.Add(9137);
			npcMissionData.m_MissionListReply.Add(9138);
			peEntity.SetUserData(npcMissionData);
			NpcMissionDataRepository.AddMissionData(peEntity.Id, npcMissionData);
		}
	}

	private void MultiGetUp()
	{
		PeSingleton<PeCreature>.Instance.mainPlayer.motionMgr.EndAction(PEActionType.Lie);
	}

	public static bool WaitForAssets()
	{
		if (PeSingleton<PeCreature>.Instance == null || PeSingleton<PeCreature>.Instance.mainPlayer == null)
		{
			return false;
		}
		MotionMgrCmpt cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<MotionMgrCmpt>();
		if (cmpt == null)
		{
			return false;
		}
		if (PeGameMgr.IsMultiStory && PlayerNetwork.mainPlayer != null && PlayerNetwork.mainPlayer._initOk)
		{
			if (PlayerNetwork.mainPlayer._gameStarted)
			{
				return true;
			}
			GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(1, 1);
			GameUI.Instance.mNPCTalk.PreShow();
			if (PlayerNetwork.mainPlayer != null)
			{
				PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_GameStarted);
			}
			return true;
		}
		return false;
	}

	protected override void SetData(byte[] data)
	{
		if (data != null)
		{
			MissionManager.Instance.m_PlayerMission.Import(data);
		}
	}

	protected override void WriteData(BinaryWriter bw)
	{
		if (!(PeSingleton<PeCreature>.Instance.mainPlayer == null))
		{
			MissionManager.Instance.m_PlayerMission.Export(bw);
		}
	}
}
