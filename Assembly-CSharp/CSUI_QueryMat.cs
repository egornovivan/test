using System.Collections.Generic;
using ItemAsset;
using Pathea;
using UnityEngine;

public class CSUI_QueryMat : MonoBehaviour
{
	public enum FuncType
	{
		Delete,
		Repair
	}

	public FuncType funcType;

	[SerializeField]
	private UIButton m_OkBtn;

	[SerializeField]
	private UIGrid m_MatGridRoot;

	[SerializeField]
	private UILabel m_TipLabel;

	[SerializeField]
	private CSUI_MaterialGrid m_MaterialItemPrefab;

	private List<CSUI_MaterialGrid> m_MatItemList;

	private PlayerPackageCmpt m_PlayerPackageCmpt;

	public CSEntity m_Entity;

	[SerializeField]
	private int[] m_DefItemCostsId;

	[SerializeField]
	private int[] m_DefItemCostsCnt;

	public string Tip
	{
		get
		{
			return m_TipLabel.text;
		}
		set
		{
			m_TipLabel.text = value;
		}
	}

	private void OnOKBtn()
	{
		if (m_Entity == null)
		{
			return;
		}
		if (!GameConfig.IsMultiMode)
		{
			if (funcType == FuncType.Repair)
			{
				foreach (CSUI_MaterialGrid matItem in m_MatItemList)
				{
					m_PlayerPackageCmpt.package.Destroy(matItem.ItemID, matItem.NeedCnt);
				}
				m_Entity.StartRepairCounter();
				CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mStartToRepair.GetString(), m_Entity.Name));
			}
			else
			{
				m_Entity.ClearDeleteGetsItem();
				foreach (CSUI_MaterialGrid matItem2 in m_MatItemList)
				{
					m_Entity.AddDeleteGetsItem(matItem2.ItemID, matItem2.NeedCnt);
				}
				m_Entity.StartDeleteCounter();
				CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mStartToDelete.GetString(), m_Entity.Name));
			}
		}
		else if (funcType == FuncType.Repair)
		{
			m_Entity._ColonyObj.Repair();
		}
		else
		{
			m_Entity._ColonyObj.RecycleItems();
		}
		Object.Destroy(base.gameObject);
	}

	private void OnCancelBtn()
	{
		Object.Destroy(base.gameObject);
	}

	private void CreateGrid(int itemId, int hasCount, int needCount)
	{
		CSUI_MaterialGrid cSUI_MaterialGrid = Object.Instantiate(m_MaterialItemPrefab);
		cSUI_MaterialGrid.transform.parent = m_MatGridRoot.transform;
		cSUI_MaterialGrid.transform.localPosition = Vector3.zero;
		cSUI_MaterialGrid.transform.localRotation = Quaternion.identity;
		cSUI_MaterialGrid.transform.localScale = Vector3.one;
		cSUI_MaterialGrid.bUseColors = funcType == FuncType.Repair;
		cSUI_MaterialGrid.ItemID = itemId;
		cSUI_MaterialGrid.ItemNum = hasCount;
		cSUI_MaterialGrid.NeedCnt = needCount;
		m_MatItemList.Add(cSUI_MaterialGrid);
	}

	private void Start()
	{
		m_MatItemList = new List<CSUI_MaterialGrid>();
		if (m_Entity == null)
		{
			return;
		}
		if (PeSingleton<PeCreature>.Instance != null && null != PeSingleton<PeCreature>.Instance.mainPlayer)
		{
			m_PlayerPackageCmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
		}
		Tip = PELocalization.GetString((funcType != 0) ? 82201082 : 82201081);
		Replicator.Formula formula = PeSingleton<Replicator.Formula.Mgr>.Instance.FindByProductId(m_Entity.ItemID);
		List<MaterialItem> repairMaterialList = ItemProto.GetRepairMaterialList(m_Entity.ItemID);
		float num = 0f;
		if (funcType == FuncType.Repair)
		{
			num = (m_Entity.m_Info.m_Durability - m_Entity.BaseData.m_Durability) / m_Entity.m_Info.m_Durability;
		}
		else if (funcType == FuncType.Delete)
		{
			num = m_Entity.BaseData.m_Durability / m_Entity.m_Info.m_Durability;
		}
		if (formula != null)
		{
			if (funcType == FuncType.Repair && repairMaterialList != null && repairMaterialList.Count > 0)
			{
				foreach (MaterialItem item in repairMaterialList)
				{
					int num2 = Mathf.CeilToInt((float)item.count * num);
					int count = m_PlayerPackageCmpt.package.GetCount(item.protoId);
					CreateGrid(item.protoId, count, num2);
					if (count < num2 && funcType == FuncType.Repair)
					{
						m_OkBtn.isEnabled = false;
					}
				}
				return;
			}
			if (repairMaterialList == null || repairMaterialList.Count == 0)
			{
				Debug.LogError("no ItemProto.repairMaterialList:" + m_Entity.ItemID);
			}
			List<Replicator.Formula.Material> materials = formula.materials;
			foreach (Replicator.Formula.Material item2 in materials)
			{
				int num3 = Mathf.CeilToInt((float)item2.itemCount * num);
				int count2 = m_PlayerPackageCmpt.package.GetCount(item2.itemId);
				CreateGrid(item2.itemId, count2, num3);
				if (count2 < num3 && funcType == FuncType.Repair)
				{
					m_OkBtn.isEnabled = false;
				}
			}
			m_MatGridRoot.repositionNow = true;
			return;
		}
		for (int i = 0; i < m_DefItemCostsId.Length; i++)
		{
			int num4 = Mathf.CeilToInt((float)m_DefItemCostsCnt[i] * num);
			int count3 = m_PlayerPackageCmpt.package.GetCount(m_DefItemCostsId[i]);
			CreateGrid(m_DefItemCostsId[i], count3, num4);
			if (count3 < num4 && funcType == FuncType.Repair)
			{
				m_OkBtn.isEnabled = false;
			}
		}
		m_MatGridRoot.repositionNow = true;
	}
}
