using System;
using System.Collections.Generic;
using ItemAsset;
using ItemAsset.PackageHelper;
using Pathea;
using UnityEngine;

public class CSUI_Assembly : MonoBehaviour
{
	[Serializable]
	public class UpgradePart
	{
		public UISlider m_Slider;

		public UILabel m_TimeLb;

		public N_ImageButton m_Button;

		public UIGrid m_Root;

		public CSUI_MaterialGrid m_MatGridPrefab;

		public UILabel m_LevelVal;
	}

	[Serializable]
	public class BuildingNumPart
	{
		public UITable m_Root;

		public CSUI_BuildingNum m_BuildingNumPrefab;
	}

	public CSAssembly m_Assembly;

	public CSEntity m_Entity;

	private PeEntity player;

	private PlayerPackageCmpt playerPackageCmpt;

	[SerializeField]
	private UpgradePart m_Upgrade;

	private List<CSUI_MaterialGrid> m_MatGrids = new List<CSUI_MaterialGrid>();

	private bool m_InitMatGrids;

	[SerializeField]
	private BuildingNumPart m_BuildingNum;

	private List<CSUI_BuildingNum> m_BuildingNums = new List<CSUI_BuildingNum>();

	[SerializeField]
	private UICheckbox m_HideShieldCB;

	public void SetEntity(CSEntity enti)
	{
		if (enti == null)
		{
			Debug.LogWarning("Reference Entity is null.");
			return;
		}
		if (m_Assembly != null)
		{
			m_Assembly.RemoveEventListener(AssemblyEventHandler);
		}
		m_Assembly = enti as CSAssembly;
		m_Assembly.AddEventListener(AssemblyEventHandler);
		if (m_Assembly == null)
		{
			Debug.LogWarning("Reference Entity is not a Assembly Entity.");
			return;
		}
		m_Entity = enti;
		UpdateUpgradeMat();
		UpdateBuildingNum();
		m_HideShieldCB.isChecked = !m_Assembly.bShowShield;
	}

	private void UpdateUpgradeMat()
	{
		int[] levelUpItem = m_Assembly.GetLevelUpItem();
		int[] levelUpItemCnt = m_Assembly.GetLevelUpItemCnt();
		if (!m_InitMatGrids)
		{
			InitMatGrids();
		}
		for (int i = 0; i < m_MatGrids.Count; i++)
		{
			if (i < levelUpItem.Length)
			{
				m_MatGrids[i].ItemID = levelUpItem[i];
				m_MatGrids[i].NeedCnt = levelUpItemCnt[i];
			}
			else
			{
				m_MatGrids[i].ItemID = 0;
			}
		}
	}

	private void RepositionNow()
	{
		m_BuildingNum.m_Root.repositionNow = true;
	}

	private void UpdateBuildingNum()
	{
		foreach (CSUI_BuildingNum buildingNum in m_BuildingNums)
		{
			UnityEngine.Object.DestroyImmediate(buildingNum.gameObject);
		}
		m_BuildingNums.Clear();
		foreach (KeyValuePair<CSConst.ObjectType, List<CSCommon>> item in m_Assembly.m_BelongObjectsMap)
		{
			CSUI_BuildingNum cSUI_BuildingNum = UnityEngine.Object.Instantiate(m_BuildingNum.m_BuildingNumPrefab);
			cSUI_BuildingNum.transform.parent = m_BuildingNum.m_Root.transform;
			cSUI_BuildingNum.transform.localRotation = Quaternion.identity;
			cSUI_BuildingNum.transform.localPosition = Vector3.zero;
			cSUI_BuildingNum.transform.localScale = Vector3.one;
			cSUI_BuildingNum.m_Description = CSUtils.GetEntityName((int)item.Key);
			cSUI_BuildingNum.m_Count = item.Value.Count;
			cSUI_BuildingNum.m_LimitCnt = m_Assembly.GetLimitCnt(item.Key);
			m_BuildingNums.Add(cSUI_BuildingNum);
		}
		Invoke("RepositionNow", 0.1f);
	}

	private void UpdateUpgradeTime()
	{
		if (m_Assembly.isUpgrading)
		{
			float num = m_Assembly.Data.m_UpgradeTime - m_Assembly.Data.m_CurUpgradeTime;
			float sliderValue = m_Assembly.Data.m_CurUpgradeTime / m_Assembly.Data.m_UpgradeTime;
			m_Upgrade.m_Slider.sliderValue = sliderValue;
			m_Upgrade.m_TimeLb.text = CSUtils.GetRealTimeMS((int)num);
		}
		else
		{
			m_Upgrade.m_Slider.sliderValue = 0f;
			m_Upgrade.m_TimeLb.text = CSUtils.GetRealTimeMS(0);
		}
	}

	private void OnUpgradeBtn()
	{
		if (PeSingleton<PeCreature>.Instance.mainPlayer == null)
		{
			return;
		}
		ItemPackage playerPak = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>().package._playerPak;
		bool flag = true;
		bool flag2 = true;
		foreach (CSUI_MaterialGrid matGrid in m_MatGrids)
		{
			if (matGrid.ItemID != 0)
			{
				matGrid.ItemNum = playerPackageCmpt.package.GetCount(matGrid.ItemID);
				if (matGrid.ItemNum < matGrid.NeedCnt)
				{
					flag2 = false;
					new PeTipMsg(PELocalization.GetString(821000001), PeTipMsg.EMsgLevel.Warning);
					break;
				}
			}
		}
		flag = ((flag2 && !m_Assembly.isUpgrading && !m_Assembly.isDeleting) ? true : false);
		if (m_Assembly.Level == m_Assembly.GetMaxLevel())
		{
			flag = false;
		}
		if (!flag)
		{
			return;
		}
		if (!GameConfig.IsMultiMode)
		{
			foreach (CSUI_MaterialGrid matGrid2 in m_MatGrids)
			{
				if (matGrid2.ItemID > 0)
				{
					playerPak.Destroy(matGrid2.ItemID, matGrid2.NeedCnt);
					CSUI_MainWndCtrl.CreatePopupHint(matGrid2.transform.position, base.transform, new Vector3(10f, -2f, -5f), " - " + matGrid2.NeedCnt, bGreen: false);
				}
			}
			m_Assembly.StartUpgradeCounter();
			CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mStartToUpgrade.GetString(), m_Entity.Name));
		}
		else
		{
			m_Assembly._ColonyObj._Network.ASB_LevelUp();
		}
	}

	private void OnHideShield(bool active)
	{
		if (m_Assembly != null)
		{
			if (!GameConfig.IsMultiMode)
			{
				m_Assembly.bShowShield = !active;
			}
			else
			{
				m_Assembly._ColonyObj._Network.ASB_HideShield(!active);
			}
		}
	}

	private void AssemblyEventHandler(int event_id, CSEntity enti, object arg)
	{
		if (event_id == 2003)
		{
			UpdateBuildingNum();
			UpdateUpgradeMat();
		}
	}

	private void InitMatGrids()
	{
		for (int i = 0; i < 5; i++)
		{
			CSUI_MaterialGrid cSUI_MaterialGrid = UnityEngine.Object.Instantiate(m_Upgrade.m_MatGridPrefab);
			cSUI_MaterialGrid.transform.parent = m_Upgrade.m_Root.transform;
			cSUI_MaterialGrid.transform.localRotation = Quaternion.identity;
			cSUI_MaterialGrid.transform.localPosition = Vector3.zero;
			cSUI_MaterialGrid.transform.localScale = Vector3.one;
			cSUI_MaterialGrid.ItemID = -1;
			cSUI_MaterialGrid.NeedCnt = 0;
			m_MatGrids.Add(cSUI_MaterialGrid);
		}
		m_Upgrade.m_Root.repositionNow = true;
		m_InitMatGrids = true;
	}

	private void Update()
	{
		if (m_Assembly == null)
		{
			return;
		}
		UpdateUpgradeTime();
		if (!(PeSingleton<PeCreature>.Instance.mainPlayer != null))
		{
			return;
		}
		if (playerPackageCmpt == null || player == null || player != PeSingleton<PeCreature>.Instance.mainPlayer)
		{
			player = PeSingleton<PeCreature>.Instance.mainPlayer;
			playerPackageCmpt = player.GetCmpt<PlayerPackageCmpt>();
		}
		bool flag = true;
		foreach (CSUI_MaterialGrid matGrid in m_MatGrids)
		{
			if (matGrid.ItemID > 0)
			{
				matGrid.ItemNum = playerPackageCmpt.package.GetCount(matGrid.ItemID);
				if (matGrid.ItemNum < matGrid.NeedCnt)
				{
					flag = false;
				}
			}
		}
		if (flag && !m_Assembly.isUpgrading && !m_Assembly.isDeleting && m_Assembly.Level != m_Assembly.GetMaxLevel())
		{
			m_Upgrade.m_Button.isEnabled = true;
		}
		else
		{
			m_Upgrade.m_Button.isEnabled = false;
		}
		int num = m_Assembly.Level + 1;
		if (m_Assembly.Level == m_Assembly.GetMaxLevel())
		{
			m_Upgrade.m_LevelVal.text = "LV " + num + " (MAX)";
		}
		else
		{
			m_Upgrade.m_LevelVal.text = "LV " + num;
		}
	}

	public void UpgradeStartSuccuss(CSAssembly entity, string rolename)
	{
		if (m_Assembly == null && m_Assembly != entity)
		{
			return;
		}
		if (PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<EntityInfoCmpt>().characterName.givenName == rolename)
		{
			foreach (CSUI_MaterialGrid matGrid in m_MatGrids)
			{
				if (matGrid.ItemID > 0)
				{
					CSUI_MainWndCtrl.CreatePopupHint(matGrid.transform.position, base.transform, new Vector3(10f, -2f, -5f), " - " + matGrid.NeedCnt, bGreen: false);
				}
			}
		}
		CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mStartToUpgrade.GetString(), m_Entity.Name));
	}
}
