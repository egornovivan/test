using System.Collections.Generic;
using ItemAsset;
using Pathea;
using UnityEngine;

public class DragItemMousePickPlant : DragItemMousePick
{
	private const double VarPerOp = 30.0;

	private int mGrowTimeIndex;

	private bool mIsDead;

	private GameObject mModel;

	private byte mTerrainType;

	public GameObject mLowerWaterTex;

	public GameObject mLowerCleanTex;

	private FarmPlantLogic mPlant;

	private FarmPlantLogic plant
	{
		get
		{
			if (mPlant == null)
			{
				mPlant = GetComponentInParent<FarmPlantLogic>();
			}
			return mPlant;
		}
	}

	protected override string tipsText
	{
		get
		{
			if (mIsDead)
			{
				return base.tipsText + "\n" + UIMsgBoxInfo.ClearPlant.GetString();
			}
			if (plant.IsRipe)
			{
				return base.tipsText + "\n" + UIMsgBoxInfo.GetPlant.GetString();
			}
			if (plant.NeedWater)
			{
				return base.tipsText + "\n" + UIMsgBoxInfo.WaterPlant.GetString();
			}
			if (plant.NeedClean)
			{
				return base.tipsText + "\n" + UIMsgBoxInfo.CleanPlant.GetString();
			}
			return base.tipsText;
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
		ItemScript_Plant component = GetComponent<ItemScript_Plant>();
		if (component != null)
		{
			component.plantUpdated = PlantUpdated;
		}
	}

	private void PlantUpdated()
	{
		CollectColliders();
	}

	protected override void InitCmd(CmdList cmdList)
	{
		cmdList.Clear();
		if (plant == null)
		{
			return;
		}
		base.InitCmd(cmdList);
		if (!plant.mDead)
		{
			if (!plant.IsRipe)
			{
				cmdList.Remove("Get");
				if (plant.NeedWater)
				{
					cmdList.Add("Water", OnWaterBtn);
				}
				if (plant.NeedClean)
				{
					cmdList.Add("Clean", OnCleanBtn);
				}
			}
		}
		else
		{
			cmdList.Remove("Get");
		}
		cmdList.Add("Remove", OnClearBtn);
	}

	public override void DoGetItem()
	{
		if (!GameConfig.IsMultiMode)
		{
			plant.UpdateStatus();
			int num = (int)((float)((int)(plant.mLife / 20.0) + 1) * 0.2f * (float)plant.mPlantInfo.mItemGetNum);
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			for (int i = 0; i < num; i++)
			{
				float num2 = Random.Range(0f, 1f);
				for (int j = 0; j < plant.mPlantInfo.mItemGetPro.Count; j++)
				{
					if (num2 < plant.mPlantInfo.mItemGetPro[j].m_probablity)
					{
						if (!dictionary.ContainsKey(plant.mPlantInfo.mItemGetPro[j].m_id))
						{
							dictionary[plant.mPlantInfo.mItemGetPro[j].m_id] = 0;
						}
						Dictionary<int, int> dictionary2;
						Dictionary<int, int> dictionary3 = (dictionary2 = dictionary);
						int key;
						int key2 = (key = plant.mPlantInfo.mItemGetPro[j].m_id);
						key = dictionary2[key];
						dictionary3[key2] = key + 1;
					}
				}
			}
			List<MaterialItem> list = new List<MaterialItem>();
			foreach (int key3 in dictionary.Keys)
			{
				MaterialItem materialItem = new MaterialItem();
				materialItem.protoId = key3;
				materialItem.count = dictionary[key3];
				list.Add(materialItem);
			}
			if (base.pkg == null || !base.pkg.CanAdd(list))
			{
				PeTipMsg.Register(PELocalization.GetString(9500312), PeTipMsg.EMsgLevel.Warning);
				return;
			}
			base.pkg.Add(list);
			FarmManager.Instance.RemovePlant(base.itemObjectId);
			DragArticleAgent.Destory(base.id);
			GameUI.Instance.mItemPackageCtrl.ResetItem();
			HideItemOpGui();
		}
		else
		{
			if (null != PlayerNetwork.mainPlayer)
			{
				PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_Plant_GetBack, base.itemObjectId);
			}
			GameUI.Instance.mItemPackageCtrl.ResetItem();
			HideItemOpGui();
		}
	}

	public void OnWaterBtn()
	{
		if (!GameConfig.IsMultiMode)
		{
			plant.UpdateStatus();
			int b = (int)(((double)plant.mPlantInfo.mWaterLevel[1] - plant.mWater) / 30.0);
			PlayerPackageCmpt cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
			int itemCount = cmpt.GetItemCount(1003);
			if (itemCount <= 0)
			{
				MessageBox_N.ShowOkBox(PELocalization.GetString(8000090));
			}
			else
			{
				plant.mWater += 30.0 * (double)Mathf.Min(itemCount, b);
				cmpt.Destory(1003, Mathf.Min(itemCount, b));
				plant.UpdateStatus();
			}
		}
		else if (null != PlayerNetwork.mainPlayer)
		{
			PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_Plant_Water, plant.mPlantInstanceId);
		}
		HideItemOpGui();
	}

	public void OnCleanBtn()
	{
		if (!GameConfig.IsMultiMode)
		{
			plant.UpdateStatus();
			int b = (int)(((double)plant.mPlantInfo.mCleanLevel[1] - plant.mClean) / 30.0);
			PlayerPackageCmpt cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
			int itemCount = cmpt.GetItemCount(1002);
			if (itemCount <= 0)
			{
				MessageBox_N.ShowOkBox(PELocalization.GetString(8000091));
			}
			else
			{
				plant.mClean += 30.0 * (double)Mathf.Min(itemCount, b);
				cmpt.Destory(1002, Mathf.Min(itemCount, b));
				plant.UpdateStatus();
			}
		}
		else if (null != PlayerNetwork.mainPlayer)
		{
			PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_Plant_Clean, plant.mPlantInstanceId);
		}
		HideItemOpGui();
	}

	public void OnClearBtn()
	{
		if (!plant.mDead)
		{
			MessageBox_N.ShowYNBox(UIMsgBoxInfo.mRemovePlantConfirm.GetString(), OnClear);
		}
		else
		{
			OnClear();
		}
	}

	public void OnClear()
	{
		if (!GameConfig.IsMultiMode)
		{
			FarmManager.Instance.RemovePlant(base.itemObjectId);
			DragArticleAgent.Destory(base.id);
		}
		else if (null != PlayerNetwork.mainPlayer)
		{
			PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_Plant_Clear, plant.mPlantInstanceId);
		}
		HideItemOpGui();
	}

	protected override void CheckOperate()
	{
		base.CheckOperate();
		if (CanCmd() && !PeInput.Get(PeInput.LogicFunction.OpenItemMenu) && PeInput.Get(PeInput.LogicFunction.InteractWithItem))
		{
			if (plant.mDead)
			{
				OnClear();
			}
			else if (plant.IsRipe)
			{
				OnGetBtn();
			}
			else if (plant.NeedWater)
			{
				OnWaterBtn();
			}
			else if (plant.NeedClean)
			{
				OnCleanBtn();
			}
			PeSingleton<MousePicker>.Instance.UpdateTis();
		}
	}
}
