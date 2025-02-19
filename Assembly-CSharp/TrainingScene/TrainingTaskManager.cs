using System;
using System.Collections;
using System.Collections.Generic;
using Pathea;
using UnityEngine;

namespace TrainingScene;

public class TrainingTaskManager : MonoBehaviour
{
	private static TrainingTaskManager s_instance;

	private GameObject mMissionArrow;

	[HideInInspector]
	private MissionManager mm;

	[HideInInspector]
	public TrainingTaskType currentMission;

	[HideInInspector]
	public static bool isNewGame;

	[HideInInspector]
	public GameObject mWareHouse;

	[SerializeField]
	private GameObject mWareHouseObj;

	public static TrainingTaskManager Instance => s_instance;

	private void Awake()
	{
		s_instance = this;
		mWareHouse = UnityEngine.Object.Instantiate(mWareHouseObj);
		mMissionArrow = GameUI.Instance.mUIMinMapCtrl.mSubInfoPanel.gameObject;
	}

	private void OnDestroy()
	{
		if (UICompoundWndControl.OnShow != null)
		{
			UICompoundWndControl.OnShow = (Action)Delegate.Remove(UICompoundWndControl.OnShow, new Action(Replicator));
		}
		if (GameUI.Instance != null && GameUI.Instance.mBuildBlock.onSaveIsoClick != null)
		{
			UIBuildBlock mBuildBlock = GameUI.Instance.mBuildBlock;
			mBuildBlock.onSaveIsoClick = (Action)Delegate.Remove(mBuildBlock.onSaveIsoClick, new Action(CompleteMission));
		}
		if (BSIsoBrush.onBrushDo != null)
		{
			BSIsoBrush.onBrushDo = (Action)Delegate.Remove(BSIsoBrush.onBrushDo, new Action(CompleteMission));
		}
	}

	public void CloseMissionArrow()
	{
		if (!(mMissionArrow == null) && !(mMissionArrow.transform.FindChild("MissionArrow(Clone)") == null))
		{
			Transform transform = mMissionArrow.transform.FindChild("MissionArrow(Clone)");
			transform.gameObject.SetActive(value: false);
		}
	}

	public void CloseMonsterPoint()
	{
		if (mMissionArrow == null)
		{
			return;
		}
		UISprite[] componentsInChildren = mMissionArrow.GetComponentsInChildren<UISprite>();
		if (componentsInChildren.Length == 0)
		{
			return;
		}
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].spriteName == "sign_monster")
			{
				componentsInChildren[i].transform.parent.gameObject.SetActive(value: false);
			}
		}
	}

	private void SetNPCsAtrribType()
	{
		List<int> list = new List<int>();
		list.Add(9004);
		list.Add(9005);
		list.Add(9006);
		list.Add(9009);
		list.Add(9020);
		list.Add(9031);
		list.Add(9041);
		List<int> list2 = list;
		foreach (int item in list2)
		{
			PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(item);
			if (!(peEntity == null))
			{
				peEntity.skEntity.SetAttribute(92, 0f);
				peEntity.skEntity.SetAttribute(95, 0f);
			}
		}
	}

	private void Start()
	{
		SetNPCsAtrribType();
		StartCoroutine(MissionStart());
		CreationMgr.Init();
		Singleton<ForceSetting>.Instance.Load(Resources.Load("ForceSetting/ForceSettings_Story") as TextAsset);
		mWareHouse.SetActive(value: false);
	}

	private void MissionStartSelf()
	{
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(9041);
		if (!(peEntity == null))
		{
			GameUI.Instance.mNpcWnd.SetCurSelNpc(peEntity);
			GameUI.Instance.mNpcWnd.OnMutexBtnClick(739);
		}
	}

	public void CompleteMission(int missionId)
	{
		switch (missionId)
		{
		case 741:
			GameUI.Instance.mWarehouse.Hide();
			break;
		case 744:
			GameUI.Instance.mItemPackageCtrl.Hide();
			break;
		case 749:
			GameUI.Instance.mWarehouse.Hide();
			break;
		case 745:
			GameUI.Instance.mItemPackageCtrl.Hide();
			break;
		case 757:
			GameUI.Instance.mItemPackageCtrl.Hide();
			break;
		case 721:
			GameUI.Instance.mCompoundWndCtrl.Hide();
			break;
		case 751:
			TerrainDigTask.Instance.DestroyScene();
			break;
		case 752:
			TerrainDigTask.Instance.DestroyScene();
			break;
		case 753:
			EmitlineTask.Instance.CloseMission_buildPoint();
			EmitlineTask.Instance.CloseMission_buildCreateISO();
			break;
		}
	}

	private void Replicator()
	{
		GameUI.Instance.mNPCTalk.NormalOrSP(0);
		GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(new List<int> { 300241 });
		GameUI.Instance.mNPCTalk.PreShow();
		UICompoundWndControl.OnShow = (Action)Delegate.Remove(UICompoundWndControl.OnShow, new Action(Replicator));
	}

	public void InitMission(int missionId)
	{
		switch (missionId)
		{
		case 739:
			NewMoveTask.Instance.InitFirstMission();
			currentMission = TrainingTaskType.ChangeControlMode;
			break;
		case 747:
			NewMoveTask.Instance.InitBaseControl();
			currentMission = TrainingTaskType.BaseControl;
			break;
		case 740:
			NewMoveTask.Instance.InitMoveScene();
			currentMission = TrainingTaskType.Move;
			break;
		case 741:
			NewMoveTask.Instance.InitGetMedicineScene();
			currentMission = TrainingTaskType.GetMedicine;
			break;
		case 742:
			currentMission = TrainingTaskType.BackToAndy;
			break;
		case 743:
			UseItemTask.Instance.InitOpenPackageScene();
			currentMission = TrainingTaskType.OpenPack;
			break;
		case 744:
			UseItemTask.Instance.InitPutMedScene();
			currentMission = TrainingTaskType.PutMed;
			break;
		case 745:
			UseItemTask.Instance.InitEquipKnifeScene();
			currentMission = TrainingTaskType.EquipKnife;
			break;
		case 746:
			currentMission = TrainingTaskType.Fighting;
			break;
		case 719:
			HoloherbTask.Instance.InitScene();
			currentMission = TrainingTaskType.GATHER;
			break;
		case 748:
			HolotreeTask.Instance.InitScene();
			currentMission = TrainingTaskType.CUT;
			break;
		case 721:
			UICompoundWndControl.OnShow = (Action)Delegate.Combine(UICompoundWndControl.OnShow, new Action(Replicator));
			break;
		case 751:
			TerrainDigTask.Instance.InitScene();
			currentMission = TrainingTaskType.DIG;
			break;
		case 750:
			TerrainDigTask.Instance.InitMenuBtn();
			currentMission = TrainingTaskType.BuildMenu;
			break;
		case 752:
			EmitlineTask.Instance.InitInBuildScene();
			currentMission = TrainingTaskType.BUILDIn;
			break;
		case 753:
			EmitlineTask.Instance.InitScene();
			currentMission = TrainingTaskType.BuildPoint;
			break;
		case 756:
			currentMission = TrainingTaskType.CreateIso;
			break;
		case 757:
			currentMission = TrainingTaskType.Replicator;
			break;
		case 754:
		{
			UIBuildBlock mBuildBlock = GameUI.Instance.mBuildBlock;
			mBuildBlock.onSaveIsoClick = (Action)Delegate.Combine(mBuildBlock.onSaveIsoClick, new Action(CompleteMission));
			currentMission = TrainingTaskType.BuildSaveIso;
			break;
		}
		case 755:
			EmitlineTask.Instance.CreateExportIsoCube();
			BSIsoBrush.onBrushDo = (Action)Delegate.Combine(BSIsoBrush.onBrushDo, new Action(CompleteMission));
			currentMission = TrainingTaskType.BuildExpotIso;
			break;
		case 718:
			currentMission = TrainingTaskType.MissionPlane;
			break;
		}
		if (null != GameUI.Instance)
		{
			GameUI.Instance.CheckMissionIDShowTutorial(missionId);
		}
	}

	public void CompleteMission()
	{
		if (currentMission == TrainingTaskType.ChangeControlMode)
		{
			if (PeGameMgr.IsMulti)
			{
				mm.RequestCompleteMission(739);
			}
			else
			{
				mm.CompleteMission(739);
			}
		}
		else if (currentMission == TrainingTaskType.BaseControl)
		{
			if (PeGameMgr.IsMultiStory)
			{
				mm.RequestCompleteMission(747);
			}
			else
			{
				mm.CompleteMission(747);
			}
		}
		else if (currentMission == TrainingTaskType.Move)
		{
			if (PeGameMgr.IsMultiStory)
			{
				mm.RequestCompleteMission(740);
			}
			else
			{
				mm.CompleteMission(740);
			}
		}
		else if (currentMission == TrainingTaskType.BackToAndy)
		{
			if (PeGameMgr.IsMulti)
			{
				mm.RequestCompleteMission(742);
			}
			else
			{
				mm.CompleteMission(742);
			}
		}
		else if (currentMission == TrainingTaskType.OpenPack)
		{
			if (PeGameMgr.IsMulti)
			{
				mm.RequestCompleteMission(743);
			}
			else
			{
				mm.CompleteMission(743);
			}
		}
		else if (currentMission == TrainingTaskType.PutMed)
		{
			if (PeGameMgr.IsMulti)
			{
				mm.RequestCompleteMission(744);
			}
			else
			{
				mm.CompleteMission(744);
			}
		}
		else if (currentMission == TrainingTaskType.EquipKnife)
		{
			if (PeGameMgr.IsMulti)
			{
				mm.RequestCompleteMission(745);
			}
			else
			{
				mm.CompleteMission(745);
			}
		}
		else if (currentMission == TrainingTaskType.GATHER)
		{
			if (PeGameMgr.IsMulti)
			{
				mm.RequestCompleteMission(719);
			}
			else
			{
				mm.CompleteMission(719);
			}
		}
		else if (currentMission == TrainingTaskType.CUT)
		{
			if (PeGameMgr.IsMulti)
			{
				mm.RequestCompleteMission(748);
			}
			else
			{
				mm.CompleteMission(748);
			}
		}
		else if (currentMission == TrainingTaskType.DIG)
		{
			if (PeGameMgr.IsMulti)
			{
				mm.RequestCompleteMission(751);
			}
			else
			{
				mm.CompleteMission(751);
			}
		}
		else if (currentMission == TrainingTaskType.BUILDIn)
		{
			if (PeGameMgr.IsMulti)
			{
				mm.RequestCompleteMission(752);
			}
			else
			{
				mm.CompleteMission(752);
			}
		}
		else if (currentMission == TrainingTaskType.BuildPoint)
		{
			if (PeGameMgr.IsMulti)
			{
				mm.RequestCompleteMission(753);
			}
			else
			{
				mm.CompleteMission(753);
			}
		}
		else if (currentMission == TrainingTaskType.BuildMenu)
		{
			if (PeGameMgr.IsMultiStory)
			{
				mm.RequestCompleteMission(750);
			}
			else
			{
				mm.CompleteMission(750);
			}
		}
		else if (currentMission == TrainingTaskType.BuildSaveIso)
		{
			UIBuildBlock mBuildBlock = GameUI.Instance.mBuildBlock;
			mBuildBlock.onSaveIsoClick = (Action)Delegate.Remove(mBuildBlock.onSaveIsoClick, new Action(CompleteMission));
			if (PeGameMgr.IsMulti)
			{
				mm.RequestCompleteMission(754);
			}
			else
			{
				mm.CompleteMission(754);
			}
		}
		else if (currentMission == TrainingTaskType.BuildExpotIso)
		{
			BSIsoBrush.onBrushDo = (Action)Delegate.Remove(BSIsoBrush.onBrushDo, new Action(CompleteMission));
			if (PeGameMgr.IsMulti)
			{
				mm.RequestCompleteMission(755);
			}
			else
			{
				mm.CompleteMission(755);
			}
		}
		else if (currentMission == TrainingTaskType.Replicator)
		{
			if (PeGameMgr.IsMulti)
			{
				mm.RequestCompleteMission(757);
			}
			else
			{
				mm.CompleteMission(757);
			}
		}
	}

	private IEnumerator MissionStart()
	{
		while (!PeSingleton<PeCreature>.Instance.mainPlayer)
		{
			yield return new WaitForSeconds(0.2f);
		}
		mm = MissionManager.Instance;
		GameUI.Instance.mNPCTalk.NormalOrSP(0);
		GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(739, 1);
		GameUI.Instance.mNPCTalk.PreShow();
	}
}
