using System;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using Pathea;
using UnityEngine;

namespace TrainingScene;

public class NewMoveTask : MonoBehaviour
{
	private static NewMoveTask mInstance;

	public NewMoveTaskAppearance appearance1;

	public NewMoveTaskAppearance1 appearance2;

	private HoloCameraControl hcc;

	private Vector3 mTarPos;

	private Vector3 mBackPos;

	private PlayerPackageCmpt ppc;

	private ItemPackage mItemPac;

	private int num;

	private bool isMoveComplete;

	private bool isBackComplete;

	private bool isGetMedComplete;

	private bool isFirstComplete;

	private PeEntity player;

	private MainPlayerCmpt playerCmpt;

	private int completeCount;

	public static NewMoveTask Instance => mInstance;

	private void Awake()
	{
		mInstance = this;
		mTarPos = new Vector3(7f, 1.5f, 12f);
		mBackPos = new Vector3(32f, 4f, 12f);
	}

	private void Start()
	{
		hcc = HoloCameraControl.Instance;
		ppc = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
		mItemPac = ppc.package._playerPak;
	}

	private void OnDestroy()
	{
		if (UIWarehouse.OnShow != null)
		{
			UIWarehouse.OnShow = (Action)Delegate.Remove(UIWarehouse.OnShow, new Action(OpenWareHouseTalk));
		}
		PeCamera.onControlModeChange = (Action<PeCamera.ControlMode>)Delegate.Remove(PeCamera.onControlModeChange, new Action<PeCamera.ControlMode>(OnControlModeChange));
	}

	private void OnItemAdd(object sender, PlayerPackageCmpt.GetItemEventArg evt)
	{
		if (evt.protoId == 916)
		{
			num++;
			num++;
			if (num >= 2)
			{
				num = 0;
				DestroyGetMedicineScene();
				TrainingTaskManager.Instance.CompleteMission();
				ppc.getItemEventor.Unsubscribe(OnItemAdd);
			}
		}
	}

	private bool HasItemObj()
	{
		if (mItemPac.FindItemByProtoId(916) != null)
		{
			return true;
		}
		return false;
	}

	private void Update()
	{
		if (null != GameUI.Instance.mMainPlayer && !isMoveComplete && Vector3.Distance(GameUI.Instance.mMainPlayer.position, mTarPos) <= 3f && MissionManager.Instance.HasMission(740))
		{
			isMoveComplete = true;
			TrainingTaskManager.Instance.CompleteMission();
			DestroyMoveScene();
		}
		if (HasItemObj() && !isGetMedComplete)
		{
			isGetMedComplete = true;
			DestroyGetMedicineScene();
			TrainingTaskManager.Instance.CompleteMission();
		}
		if (TrainingTaskManager.Instance.currentMission != TrainingTaskType.BaseControl)
		{
			return;
		}
		UIMissionMgr.MissionView missionView = UIMissionMgr.Instance.GetMissionView(747);
		if (missionView == null)
		{
			return;
		}
		if (player.motionMgr.IsActionRunning(PEActionType.Move))
		{
			UIMissionMgr.TargetShow targetShow = missionView.mTargetList.Find((UIMissionMgr.TargetShow ite) => UIMissionMgr.MissionView.MatchID(ite, 4116));
			if (targetShow != null)
			{
				targetShow.mComplete = true;
			}
		}
		if (player.motionMgr.IsActionRunning(PEActionType.Jump))
		{
			UIMissionMgr.TargetShow targetShow = missionView.mTargetList.Find((UIMissionMgr.TargetShow ite) => UIMissionMgr.MissionView.MatchID(ite, 4117));
			if (targetShow != null)
			{
				targetShow.mComplete = true;
			}
		}
		if (player.motionMgr.IsActionRunning(PEActionType.Sprint))
		{
			UIMissionMgr.TargetShow targetShow = missionView.mTargetList.Find((UIMissionMgr.TargetShow ite) => UIMissionMgr.MissionView.MatchID(ite, 4118));
			if (targetShow != null)
			{
				targetShow.mComplete = true;
			}
		}
		if (playerCmpt.AutoRun)
		{
			UIMissionMgr.TargetShow targetShow = missionView.mTargetList.Find((UIMissionMgr.TargetShow ite) => UIMissionMgr.MissionView.MatchID(ite, 4119));
			if (targetShow != null)
			{
				targetShow.mComplete = true;
			}
		}
		bool flag = true;
		foreach (UIMissionMgr.TargetShow mTarget in missionView.mTargetList)
		{
			if (!mTarget.mComplete)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			TrainingTaskManager.Instance.CompleteMission();
		}
	}

	private void OnBoxOpClose(bool OpOrClose)
	{
		if (null != TrainingTaskManager.Instance.mWareHouse)
		{
			TrainingTaskManager.Instance.mWareHouse.SetActive(OpOrClose);
		}
	}

	public void InitBaseControl()
	{
		player = PeSingleton<PeCreature>.Instance.mainPlayer;
		playerCmpt = player.GetComponent<MainPlayerCmpt>();
	}

	public void InitFirstMission()
	{
		PeCamera.onControlModeChange = (Action<PeCamera.ControlMode>)Delegate.Combine(PeCamera.onControlModeChange, new Action<PeCamera.ControlMode>(OnControlModeChange));
		completeCount = 0;
	}

	private void OnControlModeChange(PeCamera.ControlMode mode)
	{
		UIMissionMgr.MissionView missionView = UIMissionMgr.Instance.GetMissionView(739);
		if (missionView == null)
		{
			MissionManager.Instance.CompleteMission(739);
			PeCamera.onControlModeChange = (Action<PeCamera.ControlMode>)Delegate.Remove(PeCamera.onControlModeChange, new Action<PeCamera.ControlMode>(OnControlModeChange));
		}
		switch (mode)
		{
		case PeCamera.ControlMode.MMOControl:
		{
			UIMissionMgr.TargetShow targetShow = missionView.mTargetList.Find((UIMissionMgr.TargetShow ite) => UIMissionMgr.MissionView.MatchID(ite, 4114));
			if (targetShow != null)
			{
				targetShow.mComplete = true;
			}
			completeCount++;
			if (MissionRepository.HaveTalkOP(722))
			{
				GameUI.Instance.mNPCTalk.NormalOrSP(0);
				GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(722, 1);
				GameUI.Instance.mNPCTalk.PreShow();
			}
			else if (MissionManager.Instance.IsGetTakeMission(722))
			{
				MissionManager.Instance.SetGetTakeMission(722, null, MissionManager.TakeMissionType.TakeMissionType_Get);
			}
			break;
		}
		case PeCamera.ControlMode.FirstPerson:
		{
			UIMissionMgr.TargetShow targetShow = missionView.mTargetList.Find((UIMissionMgr.TargetShow ite) => UIMissionMgr.MissionView.MatchID(ite, 4115));
			if (targetShow != null)
			{
				targetShow.mComplete = true;
			}
			completeCount++;
			if (MissionRepository.HaveTalkOP(723))
			{
				GameUI.Instance.mNPCTalk.NormalOrSP(0);
				GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(723, 1);
				GameUI.Instance.mNPCTalk.PreShow();
			}
			else if (MissionManager.Instance.IsGetTakeMission(723))
			{
				MissionManager.Instance.SetGetTakeMission(723, null, MissionManager.TakeMissionType.TakeMissionType_Get);
			}
			break;
		}
		}
		if (completeCount > 1)
		{
			TrainingTaskManager.Instance.CompleteMission();
			PeCamera.onControlModeChange = (Action<PeCamera.ControlMode>)Delegate.Remove(PeCamera.onControlModeChange, new Action<PeCamera.ControlMode>(OnControlModeChange));
		}
	}

	public void InitMoveScene()
	{
	}

	public void DestroyMoveScene()
	{
		Debug.Log("关闭NewMoveMove场景");
		DestroyTerrian();
	}

	public void InitGetMedicineScene()
	{
		if (TrainingTaskManager.Instance.mWareHouse != null)
		{
			TrainingTaskManager.Instance.mWareHouse.SetActive(value: true);
		}
		UIWarehouse.OnShow = (Action)Delegate.Combine(UIWarehouse.OnShow, new Action(OpenWareHouseTalk));
	}

	private void OpenWareHouseTalk()
	{
		GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(new List<int> { 300219 });
		GameUI.Instance.mNPCTalk.PreShow();
		UIWarehouse.OnShow = (Action)Delegate.Remove(UIWarehouse.OnShow, new Action(OpenWareHouseTalk));
	}

	public void DestroyGetMedicineScene()
	{
		if (TrainingTaskManager.Instance.mWareHouse != null)
		{
			TrainingTaskManager.Instance.mWareHouse.SetActive(value: false);
		}
	}

	private void CloseMission()
	{
		Debug.Log("开始NewMove任务");
	}

	private void CreatTerrian()
	{
		BSVoxel voxel = default(BSVoxel);
		voxel.blockType = BSVoxel.MakeBlockType(1, 0);
		if (BSBlockMatMap.s_ItemToMat.ContainsKey(281))
		{
			voxel.materialType = (byte)BSBlockMatMap.s_ItemToMat[281];
		}
		else
		{
			voxel.materialType = 0;
		}
		for (int i = 9; i <= 14; i++)
		{
			Vector3 vector = new Vector3(10f, 2f, i);
			vector = new Vector3(vector.x * (float)BuildingMan.Blocks.ScaleInverted, vector.y * (float)BuildingMan.Blocks.ScaleInverted, vector.z * (float)BuildingMan.Blocks.ScaleInverted);
			BuildingMan.Blocks.SafeWrite(voxel, (int)vector.x, (int)vector.y, (int)vector.z);
			Vector3 vector2 = new Vector3(10f, 2f, (float)i + 0.5f);
			vector2 = new Vector3(vector2.x * (float)BuildingMan.Blocks.ScaleInverted, vector2.y * (float)BuildingMan.Blocks.ScaleInverted, vector2.z * (float)BuildingMan.Blocks.ScaleInverted);
			BuildingMan.Blocks.SafeWrite(voxel, (int)vector2.x, (int)vector2.y, (int)vector2.z);
			Vector3 vector3 = new Vector3(10f, 1.5f, i);
			vector3 = new Vector3(vector3.x * (float)BuildingMan.Blocks.ScaleInverted, vector3.y * (float)BuildingMan.Blocks.ScaleInverted, vector3.z * (float)BuildingMan.Blocks.ScaleInverted);
			BuildingMan.Blocks.SafeWrite(voxel, (int)vector3.x, (int)vector3.y, (int)vector3.z);
			Vector3 vector4 = new Vector3(10f, 1.5f, (float)i + 0.5f);
			vector4 = new Vector3(vector4.x * (float)BuildingMan.Blocks.ScaleInverted, vector4.y * (float)BuildingMan.Blocks.ScaleInverted, vector4.z * (float)BuildingMan.Blocks.ScaleInverted);
			BuildingMan.Blocks.SafeWrite(voxel, (int)vector4.x, (int)vector4.y, (int)vector4.z);
		}
	}

	private void DestroyTerrian()
	{
		BSVoxel voxel = default(BSVoxel);
		voxel.blockType = 0;
		voxel.materialType = 0;
		for (int i = 9; i <= 14; i++)
		{
			Vector3 vector = new Vector3(10f, 2f, i);
			vector = new Vector3(vector.x * (float)BuildingMan.Blocks.ScaleInverted, vector.y * (float)BuildingMan.Blocks.ScaleInverted, vector.z * (float)BuildingMan.Blocks.ScaleInverted);
			BuildingMan.Blocks.SafeWrite(voxel, (int)vector.x, (int)vector.y, (int)vector.z);
			Vector3 vector2 = new Vector3(10f, 2f, (float)i + 0.5f);
			vector2 = new Vector3(vector2.x * (float)BuildingMan.Blocks.ScaleInverted, vector2.y * (float)BuildingMan.Blocks.ScaleInverted, vector2.z * (float)BuildingMan.Blocks.ScaleInverted);
			BuildingMan.Blocks.SafeWrite(voxel, (int)vector2.x, (int)vector2.y, (int)vector2.z);
			Vector3 vector3 = new Vector3(10f, 1.5f, i);
			vector3 = new Vector3(vector3.x * (float)BuildingMan.Blocks.ScaleInverted, vector3.y * (float)BuildingMan.Blocks.ScaleInverted, vector3.z * (float)BuildingMan.Blocks.ScaleInverted);
			BuildingMan.Blocks.SafeWrite(voxel, (int)vector3.x, (int)vector3.y, (int)vector3.z);
			Vector3 vector4 = new Vector3(10f, 1.5f, (float)i + 0.5f);
			vector4 = new Vector3(vector4.x * (float)BuildingMan.Blocks.ScaleInverted, vector4.y * (float)BuildingMan.Blocks.ScaleInverted, vector4.z * (float)BuildingMan.Blocks.ScaleInverted);
			BuildingMan.Blocks.SafeWrite(voxel, (int)vector4.x, (int)vector4.y, (int)vector4.z);
		}
	}

	private IEnumerator FindTerrian()
	{
		yield return new WaitForSeconds(1f);
		Transform terrain1 = GameObject.Find("b45Chnk_8_0_12_0").transform;
		Transform terrain2 = GameObject.Find("b45Chnk_8_0_8_0").transform;
		if (terrain1 != null)
		{
			appearance1.orgterrain = terrain1;
			terrain1.gameObject.SetActive(value: false);
		}
		appearance1.gameObject.SetActive(value: true);
		appearance1.orgterrain.gameObject.SetActive(value: true);
		appearance1.orgterrain.GetComponent<MeshCollider>().GetComponent<Collider>().enabled = true;
		hcc.renderObjs2.Add(appearance1.orgterrain.GetComponent<MeshRenderer>());
		appearance1.produce = true;
		if (terrain2 != null)
		{
			appearance2.orgterrain = terrain2;
			terrain2.gameObject.SetActive(value: false);
		}
		appearance2.gameObject.SetActive(value: true);
		appearance2.orgterrain.gameObject.SetActive(value: true);
		appearance2.orgterrain.GetComponent<MeshCollider>().GetComponent<Collider>().enabled = true;
		hcc.renderObjs2.Add(appearance2.orgterrain.GetComponent<MeshRenderer>());
		appearance2.produce = true;
	}

	public void ChangeRenderTarget(MeshRenderer newTarget, bool org = true)
	{
		if (org)
		{
			hcc.renderObjs2.Clear();
			hcc.renderObjs2.Add(newTarget);
		}
		else
		{
			hcc.renderObjs3.Clear();
			hcc.renderObjs3.Add(newTarget);
		}
	}
}
