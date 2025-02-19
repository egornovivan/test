using System;
using System.Collections.Generic;
using ItemAsset;
using ItemAsset.PackageHelper;
using Pathea;
using UnityEngine;

public class CSUI_PPCoal : MonoBehaviour
{
	[Serializable]
	public class WorkPart
	{
		public UILabel m_TimePer;

		public UILabel m_TimeLeft;

		public N_ImageButton m_Button;

		public UIGrid m_Root;

		public CSUI_MaterialGrid m_MatGridPrefab;
	}

	[Serializable]
	public class SupplyPart
	{
		public UIGrid m_Root;

		public CSUI_BuildingIcon m_IconPrefab;
	}

	[Serializable]
	public class ChargePart
	{
		public UIGrid m_Root;

		public CSUI_ChargingGrid m_ChargeGridPrefab;
	}

	private PeEntity player;

	private PlayerPackageCmpt playerPackageCmpt;

	public CSPPCoal m_PPCoal;

	public CSEntity m_Entity;

	[SerializeField]
	private WorkPart m_Work;

	private List<CSUI_MaterialGrid> m_MatGrids = new List<CSUI_MaterialGrid>();

	[SerializeField]
	private SupplyPart m_Supply;

	private List<CSUI_BuildingIcon> m_Icons = new List<CSUI_BuildingIcon>();

	[SerializeField]
	private UICheckbox m_HideElectricLine;

	[SerializeField]
	private ChargePart m_Charging;

	private List<CSUI_ChargingGrid> m_ChargingGrids = new List<CSUI_ChargingGrid>();

	public UIGrid mIcoGrid;

	public CSUI_CommonIcon m_CommonIcoPrefab;

	private List<CSUI_CommonIcon> mUIPPCoalIcoList = new List<CSUI_CommonIcon>();

	public void SetEntityList(List<CSEntity> entityList, CSEntity selectEntity)
	{
		if (entityList == null || mUIPPCoalIcoList == null)
		{
			return;
		}
		for (int i = 0; i < mUIPPCoalIcoList.Count; i++)
		{
			if (i < entityList.Count)
			{
				mUIPPCoalIcoList[i].gameObject.SetActive(value: true);
				mUIPPCoalIcoList[i].Common = entityList[i] as CSCommon;
			}
			else
			{
				mUIPPCoalIcoList[i].gameObject.SetActive(value: false);
				mUIPPCoalIcoList[i].Common = null;
			}
		}
		CSEntity cSEntity = null;
		if (selectEntity != null)
		{
			cSEntity = selectEntity;
		}
		else if (m_Entity != null)
		{
			cSEntity = m_Entity;
		}
		if (cSEntity == null || !entityList.Contains(cSEntity))
		{
			cSEntity = entityList[0];
		}
		CSUI_CommonIcon cSUI_CommonIcon = mUIPPCoalIcoList.Find((CSUI_CommonIcon a) => a.Common == selectEntity);
		if (null != cSUI_CommonIcon)
		{
			SetEntity(cSEntity);
			cSUI_CommonIcon.mCheckBox.isChecked = true;
		}
		mIcoGrid.repositionNow = true;
	}

	private bool IsEntiyContain(List<CSEntity> entityList)
	{
		return entityList.Contains(m_Entity);
	}

	private void OnClickIcoItem(CSEntity enti)
	{
		SetEntity(enti);
	}

	public void SetEntity(CSEntity enti)
	{
		if (enti == null)
		{
			Debug.LogWarning("Reference Entity is null.");
			return;
		}
		m_PPCoal = enti as CSPPCoal;
		if (m_PPCoal == null)
		{
			Debug.LogWarning("Reference Entity is not a PowerPlant Entity.");
			return;
		}
		m_Entity = enti;
		CSUI_MainWndCtrl.Instance.mSelectedEnntity = enti;
		SetFuelMaterial();
		SetSupplies();
		foreach (CSUI_ChargingGrid chargingGrid in m_ChargingGrids)
		{
			chargingGrid.SetItem(m_PPCoal.GetChargingItem(chargingGrid.m_Index));
		}
		m_HideElectricLine.isChecked = !m_PPCoal.bShowElectric;
	}

	private void OnHideElectric(bool active)
	{
		if (m_PPCoal != null)
		{
			m_PPCoal.bShowElectric = !active;
		}
	}

	private void SetFuelMaterial()
	{
		for (int i = 0; i < 1; i++)
		{
			if (i < m_PPCoal.Info.m_WorkedTimeItemID.Count)
			{
				int itemID = m_PPCoal.Info.m_WorkedTimeItemID[i];
				int maxCnt = m_PPCoal.Info.m_WorkedTimeItemCnt[i];
				m_MatGrids[i].ItemID = itemID;
				m_MatGrids[i].MaxCnt = maxCnt;
			}
			else
			{
				m_MatGrids[i].ItemID = 0;
				m_MatGrids[i].MaxCnt = 0;
			}
		}
	}

	private void UpdateWorkingTime()
	{
		float restTime = m_PPCoal.RestTime;
		float restPercent = m_PPCoal.RestPercent;
		m_Work.m_TimePer.text = (int)(restPercent * 100f) + " %";
		m_Work.m_TimeLeft.text = CSUtils.GetRealTimeMS((int)restTime);
		bool isEnabled = true;
		foreach (CSUI_MaterialGrid matGrid in m_MatGrids)
		{
			if (matGrid.ItemID != 0)
			{
				matGrid.NeedCnt = Mathf.Max(1, Mathf.RoundToInt((float)matGrid.MaxCnt * (1f - restPercent)));
				matGrid.ItemNum = playerPackageCmpt.package.GetCount(matGrid.ItemID);
				if (matGrid.NeedCnt > matGrid.ItemNum)
				{
					isEnabled = false;
				}
			}
			else
			{
				matGrid.ItemNum = -1;
			}
		}
		m_Work.m_Button.isEnabled = isEnabled;
	}

	private void SetSupplies()
	{
		foreach (CSUI_BuildingIcon icon in m_Icons)
		{
			UnityEngine.Object.DestroyImmediate(icon.gameObject);
		}
		m_Icons.Clear();
		int num = 0;
		int num2 = 0;
		CSUI_BuildingIcon cSUI_BuildingIcon = null;
		CSUI_BuildingIcon cSUI_BuildingIcon2 = null;
		foreach (CSElectric electric in m_PPCoal.m_Electrics)
		{
			if (electric.m_Type == 2)
			{
				if (num == 0)
				{
					cSUI_BuildingIcon = _createIcons(electric);
				}
				num++;
			}
			else if (electric.m_Type == 21)
			{
				if (num2 == 0)
				{
					cSUI_BuildingIcon2 = _createIcons(electric);
				}
				num2++;
			}
			else
			{
				_createIcons(electric);
			}
		}
		m_Supply.m_Root.repositionNow = true;
		if (cSUI_BuildingIcon != null)
		{
			CSUI_BuildingIcon cSUI_BuildingIcon3 = cSUI_BuildingIcon;
			cSUI_BuildingIcon3.Description = cSUI_BuildingIcon3.Description + " X " + num;
		}
		if (cSUI_BuildingIcon2 != null)
		{
			CSUI_BuildingIcon cSUI_BuildingIcon4 = cSUI_BuildingIcon2;
			cSUI_BuildingIcon4.Description = cSUI_BuildingIcon4.Description + " X " + cSUI_BuildingIcon2.ToString();
		}
	}

	private CSUI_BuildingIcon _createIcons(CSElectric cse)
	{
		CSUI_BuildingIcon cSUI_BuildingIcon = UnityEngine.Object.Instantiate(m_Supply.m_IconPrefab);
		cSUI_BuildingIcon.transform.parent = m_Supply.m_Root.transform;
		cSUI_BuildingIcon.transform.localPosition = Vector3.zero;
		cSUI_BuildingIcon.transform.localRotation = Quaternion.identity;
		cSUI_BuildingIcon.transform.localScale = Vector3.one;
		string[] iconName = ItemProto.GetIconName(cse.ItemID);
		if (iconName.Length != 0)
		{
			cSUI_BuildingIcon.IconName = iconName[0];
		}
		else
		{
			cSUI_BuildingIcon.IconName = string.Empty;
		}
		cSUI_BuildingIcon.Description = cse.Name;
		m_Icons.Add(cSUI_BuildingIcon);
		return cSUI_BuildingIcon;
	}

	private void OnAddFuelBtn()
	{
		if (PeSingleton<PeCreature>.Instance == null || PeSingleton<PeCreature>.Instance.mainPlayer == null || m_PPCoal == null || m_Entity == null)
		{
			return;
		}
		if (!GameConfig.IsMultiMode)
		{
			PlayerPackageCmpt cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
			if (null == cmpt)
			{
				return;
			}
			ItemPackage playerPak = cmpt.package._playerPak;
			if (playerPak == null)
			{
				return;
			}
			foreach (CSUI_MaterialGrid matGrid in m_MatGrids)
			{
				if (null != matGrid && matGrid.ItemID != 0)
				{
					playerPak.Destroy(matGrid.ItemID, matGrid.NeedCnt);
					CSUI_MainWndCtrl.CreatePopupHint(matGrid.transform.position, base.transform, new Vector3(10f, -2f, -5f), " - " + matGrid.NeedCnt, bGreen: false);
				}
			}
			m_PPCoal.StartWorkingCounter();
			CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mFullFuelTips.GetString(), m_Entity.Name));
		}
		else
		{
			m_PPCoal._ColonyObj._Network.PPC_AddFuel();
		}
	}

	private void OnChargingItemChanged(int index, ItemObject item)
	{
		if (m_PPCoal != null)
		{
			m_PPCoal.SetChargingItem(index, item);
		}
	}

	private bool OnChargingItemItemCheck(ItemObject item)
	{
		if (!CSUI_MainWndCtrl.IsWorking())
		{
			return false;
		}
		return true;
	}

	private void Awake()
	{
		for (int i = 0; i < 1; i++)
		{
			CSUI_MaterialGrid cSUI_MaterialGrid = UnityEngine.Object.Instantiate(m_Work.m_MatGridPrefab);
			cSUI_MaterialGrid.transform.parent = m_Work.m_Root.transform;
			cSUI_MaterialGrid.transform.localPosition = Vector3.zero;
			cSUI_MaterialGrid.transform.localRotation = Quaternion.identity;
			cSUI_MaterialGrid.transform.localScale = Vector3.one;
			cSUI_MaterialGrid.ItemID = 0;
			m_MatGrids.Add(cSUI_MaterialGrid);
		}
		for (int j = 0; j < 8; j++)
		{
			CSUI_CommonIcon cSUI_CommonIcon = UnityEngine.Object.Instantiate(m_CommonIcoPrefab);
			cSUI_CommonIcon.transform.parent = mIcoGrid.transform;
			cSUI_CommonIcon.transform.localPosition = Vector3.zero;
			cSUI_CommonIcon.transform.localRotation = Quaternion.identity;
			cSUI_CommonIcon.transform.localScale = Vector3.one;
			cSUI_CommonIcon.e_OnClickIco += OnClickIcoItem;
			cSUI_CommonIcon.gameObject.SetActive(value: true);
			mUIPPCoalIcoList.Add(cSUI_CommonIcon);
		}
		mIcoGrid.repositionNow = true;
	}

	private void Start()
	{
		if (m_PPCoal == null)
		{
			return;
		}
		for (int i = 0; i < m_PPCoal.GetChargingItemsCnt(); i++)
		{
			CSUI_ChargingGrid cSUI_ChargingGrid = UnityEngine.Object.Instantiate(m_Charging.m_ChargeGridPrefab);
			cSUI_ChargingGrid.transform.parent = m_Charging.m_Root.transform;
			cSUI_ChargingGrid.transform.localPosition = Vector3.zero;
			cSUI_ChargingGrid.transform.localRotation = Quaternion.identity;
			cSUI_ChargingGrid.transform.localScale = Vector3.one;
			cSUI_ChargingGrid.m_Index = i;
			cSUI_ChargingGrid.m_bCanChargeLargedItem = true;
			cSUI_ChargingGrid.m_bUseMsgBox = false;
			cSUI_ChargingGrid.onItemChanded = OnChargingItemChanged;
			cSUI_ChargingGrid.onItemCheck = OnChargingItemItemCheck;
			if (GameConfig.IsMultiMode)
			{
				cSUI_ChargingGrid.OnDropItemMulti = OnDropItemMulti;
				cSUI_ChargingGrid.OnLeftMouseClickedMulti = OnLeftMouseClickedMulti;
				cSUI_ChargingGrid.OnRightMouseClickedMulti = OnRightMouseClickedMulti;
			}
			m_ChargingGrids.Add(cSUI_ChargingGrid);
			cSUI_ChargingGrid.SetItem(m_PPCoal.GetChargingItem(cSUI_ChargingGrid.m_Index));
		}
		m_Charging.m_Root.repositionNow = true;
	}

	private void Update()
	{
		if (m_PPCoal != null && !(PeSingleton<PeCreature>.Instance.mainPlayer == null))
		{
			if (playerPackageCmpt == null || player == null || player != PeSingleton<PeCreature>.Instance.mainPlayer)
			{
				player = PeSingleton<PeCreature>.Instance.mainPlayer;
				playerPackageCmpt = player.GetCmpt<PlayerPackageCmpt>();
			}
			UpdateWorkingTime();
		}
	}

	public void AddFuelSuccess(CSPPCoal m_PPCoal)
	{
		if (this.m_PPCoal != m_PPCoal)
		{
			return;
		}
		foreach (CSUI_MaterialGrid matGrid in m_MatGrids)
		{
			if (matGrid.ItemID != 0)
			{
				CSUI_MainWndCtrl.CreatePopupHint(matGrid.transform.position, base.transform, new Vector3(10f, -2f, -5f), " - " + matGrid.NeedCnt, bGreen: false);
			}
		}
		CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mFullFuelTips.GetString(), m_Entity.Name));
	}

	public void OnDropItemMulti(int index, Grid_N grid)
	{
		CSUI_ChargingGrid cSUI_ChargingGrid = m_ChargingGrids[index];
		ItemObject itemObj = SelectItem_N.Instance.ItemObj;
		if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_HotKeyBar)
		{
			SelectItem_N.Instance.SetItem(null);
			return;
		}
		if (!cSUI_ChargingGrid.IsChargeable(itemObj))
		{
			SelectItem_N.Instance.SetItem(null);
			return;
		}
		m_PPCoal._ColonyObj._Network.POW_AddChargItem(index, itemObj);
		SelectItem_N.Instance.SetItem(null);
	}

	public void OnLeftMouseClickedMulti(int index, Grid_N grid)
	{
		if (grid.Item != null)
		{
			SelectItem_N.Instance.SetItemGrid(grid);
		}
	}

	public void OnRightMouseClickedMulti(int index, Grid_N grid)
	{
		GameUI.Instance.mItemPackageCtrl.Show();
		if (grid.ItemObj != null)
		{
			m_PPCoal._ColonyObj._Network.POW_RemoveChargItem(grid.ItemObj.instanceId);
		}
	}

	public void AddChargeItemResult(bool success, int index, int objId, CSPPCoal entity)
	{
		if (entity != m_PPCoal)
		{
			return;
		}
		CSUI_ChargingGrid cSUI_ChargingGrid = m_ChargingGrids[index];
		ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(objId);
		if (success)
		{
			cSUI_ChargingGrid.SetItemUI(itemObject);
			if (!cSUI_ChargingGrid.m_bUseMsgBox)
			{
				CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mStartToCharge.GetString(), itemObject.protoData.GetName()));
			}
		}
		else if (!cSUI_ChargingGrid.m_bUseMsgBox)
		{
			CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mNotToBeCharged.GetString(), itemObject.protoData.GetName()), Color.red);
		}
	}

	public void GetChargItemResult(bool success, int objId, CSPPCoal entity)
	{
		if (!success || entity != m_PPCoal)
		{
			return;
		}
		for (int i = 0; i < m_ChargingGrids.Count; i++)
		{
			if (m_ChargingGrids[i].Item != null && m_ChargingGrids[i].Item.instanceId == objId)
			{
				m_ChargingGrids[i].SetItemUI(null);
				break;
			}
		}
	}
}
