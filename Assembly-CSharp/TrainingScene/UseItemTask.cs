using Pathea;
using UnityEngine;

namespace TrainingScene;

public class UseItemTask : MonoBehaviour
{
	private static UseItemTask mInstance;

	public static UseItemTask Instance => mInstance;

	private void Awake()
	{
		mInstance = this;
	}

	private void OnDestroy()
	{
		if (GameUI.Instance != null && GameUI.Instance.mItemPackageCtrl != null)
		{
			GameUI.Instance.mItemPackageCtrl.e_OnOpenPackage -= OnOpenPackage;
		}
		if (UIMainMidCtrl.Instance != null)
		{
			UIMainMidCtrl.Instance.e_OnDropItemTask -= OnUIMainMidDropItem;
		}
		if (PeSingleton<PeCreature>.Instance != null && PeSingleton<PeCreature>.Instance.mainPlayer != null)
		{
			PeSingleton<PeCreature>.Instance.mainPlayer.equipmentCmpt.changeEventor.Unsubscribe(OnEquip);
		}
	}

	public void InitOpenPackageScene()
	{
		if (GameUI.Instance != null && GameUI.Instance.mItemPackageCtrl != null)
		{
			GameUI.Instance.mItemPackageCtrl.e_OnOpenPackage += OnOpenPackage;
		}
		if (TrainingTaskManager.Instance != null)
		{
			TrainingTaskManager.Instance.CloseMissionArrow();
		}
	}

	private void OnOpenPackage()
	{
		GameUI.Instance.mItemPackageCtrl.e_OnOpenPackage -= OnOpenPackage;
		TrainingTaskManager.Instance.CompleteMission();
	}

	public void InitPutMedScene()
	{
		if (UIMainMidCtrl.Instance != null)
		{
			UIMainMidCtrl.Instance.e_OnDropItemTask += OnUIMainMidDropItem;
		}
		if (TrainingTaskManager.Instance != null)
		{
			TrainingTaskManager.Instance.CloseMissionArrow();
		}
	}

	private void OnUIMainMidDropItem()
	{
		if (UIMainMidCtrl.Instance != null)
		{
			UIMainMidCtrl.Instance.e_OnDropItemTask -= OnUIMainMidDropItem;
		}
		TrainingTaskManager.Instance.CompleteMission();
	}

	public void InitEquipKnifeScene()
	{
		if (TrainingTaskManager.Instance != null)
		{
			TrainingTaskManager.Instance.CloseMissionArrow();
		}
		PeSingleton<PeCreature>.Instance.mainPlayer.equipmentCmpt.changeEventor.Subscribe(OnEquip);
	}

	private void OnEquip(object sender, EquipmentCmpt.EventArg arg)
	{
		if (arg.isAdd && arg.itemObj.protoData.weaponInfo != null)
		{
			TrainingTaskManager.Instance.CompleteMission();
			PeSingleton<PeCreature>.Instance.mainPlayer.equipmentCmpt.changeEventor.Unsubscribe(OnEquip);
		}
	}
}
