using Pathea;
using Pathea.PeEntityExt;
using UnityEngine;

public class NpcCreator : MonoBehaviour
{
	[SerializeField]
	private int m_npcProtoId = 11;

	private void Start()
	{
		int id = PeSingleton<WorldInfoMgr>.Instance.FetchNonRecordAutoId();
		CreateWithProto(id, m_npcProtoId);
	}

	private void CreateWithProto(int id, int protoId)
	{
		PeEntity peEntity = PeSingleton<PeEntityCreator>.Instance.CreateNpc(id, protoId, base.transform.position, Quaternion.identity, Vector3.one);
		PeMousePickCmpt cmpt = peEntity.GetCmpt<PeMousePickCmpt>();
		if (null != cmpt)
		{
			cmpt.mousePick.eventor.Subscribe(NpcMouseEventHandler);
		}
		NpcMissionData npcMissionData = new NpcMissionData();
		npcMissionData.m_bRandomNpc = true;
		npcMissionData.m_Rnpc_ID = protoId;
		npcMissionData.m_QCID = 1;
		npcMissionData.m_MissionList.Add(191);
		peEntity.SetUserData(npcMissionData);
	}

	public Vector3 GetPlayerPos()
	{
		PeTrans peTrans = null;
		if (peTrans == null)
		{
			peTrans = PeSingleton<PeCreature>.Instance.mainPlayer.peTrans;
		}
		if (peTrans == null)
		{
			return Vector3.zero;
		}
		return peTrans.position;
	}

	public bool IsRandomNpc(PeEntity npc)
	{
		if (npc == null)
		{
			return false;
		}
		if (!(npc.GetUserData() is NpcMissionData npcMissionData))
		{
			return false;
		}
		return npcMissionData.m_bRandomNpc;
	}

	public void NpcMouseEventHandler(object sender, MousePickable.RMouseClickEvent e)
	{
		PeEntity component = e.mousePickable.GetComponent<PeEntity>();
		if (component == null)
		{
			return;
		}
		float num = Vector3.Distance(component.position, GetPlayerPos());
		if (num > 7f)
		{
			return;
		}
		if (IsRandomNpc(component) && component.IsDead())
		{
			if (component.Id != 9203 && component.Id != 9204 && ((component.Id != 9214 && component.Id != 9215) || MissionManager.Instance.HasMission(242)))
			{
				if (GameConfig.IsMultiMode)
				{
				}
				if (component.IsRecruited())
				{
					GameUI.Instance.mRevive.ShowServantRevive(component);
				}
			}
		}
		else
		{
			if ((IsRandomNpc(component) && component.IsFollower()) || !component.GetTalkEnable())
			{
				return;
			}
			if (IsRandomNpc(component) && !component.IsDead() && component.GetUserData() is NpcMissionData npcMissionData && !MissionManager.Instance.HasMission(npcMissionData.m_RandomMission))
			{
				if (PeGameMgr.IsStory)
				{
					RMRepository.CreateRandomMission(npcMissionData.m_RandomMission);
				}
				else if (PeGameMgr.IsMultiAdventure || PeGameMgr.IsMultiBuild)
				{
					PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_NewMission, npcMissionData.m_RandomMission, component.Id);
				}
				else
				{
					AdRMRepository.CreateRandomMission(npcMissionData.m_RandomMission);
				}
			}
			GameUI.Instance.mNpcWnd.SetCurSelNpc(component);
			GameUI.Instance.mNpcWnd.Show();
		}
	}
}
