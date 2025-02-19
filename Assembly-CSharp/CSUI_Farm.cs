using System;
using System.Collections.Generic;
using ItemAsset;
using Pathea;
using UnityEngine;

public class CSUI_Farm : MonoBehaviour
{
	[Serializable]
	public class PlantsPart
	{
		public UIGrid m_Root;

		public CSUI_PlantGrid m_PlantGridPrefab;
	}

	private CSFarm m_Farm;

	public CSEntity m_Entity;

	[SerializeField]
	private PlantsPart m_PlantPart;

	private List<CSUI_PlantGrid> m_PlantGrids = new List<CSUI_PlantGrid>(1);

	[SerializeField]
	private CSUI_Grid m_GridPrefab;

	[SerializeField]
	private UIGrid m_SeedsRoot;

	[SerializeField]
	private UIGrid m_ToolsRoot;

	[SerializeField]
	private UICheckbox m_SequentialPlantingCB;

	private List<CSUI_Grid> m_SeedsGrids = new List<CSUI_Grid>();

	private List<CSUI_Grid> m_ToolsGrids = new List<CSUI_Grid>();

	private List<CSUI_NPCGrid> m_NpcGrids = new List<CSUI_NPCGrid>();

	public CSFarm Farm => m_Farm;

	public void SetFarm(CSEntity enti)
	{
		if (enti == null)
		{
			Debug.LogWarning("Reference Entity is null.");
			return;
		}
		m_Entity = enti;
		CSFarm cSFarm = enti as CSFarm;
		m_SequentialPlantingCB.isChecked = cSFarm.Data.m_SequentialPlanting;
		if (cSFarm == m_Farm)
		{
			return;
		}
		if (m_Farm != null)
		{
			m_Farm.CreatePlantEvent -= OnCreatePlant;
			m_Farm.RemovePlantEvent -= OnRemovePlant;
			m_Farm.CreatePlantListEvent -= OnCreatAllPlants;
			m_Farm.ClearAllPlantEvent -= OnClearAllPlants;
			m_Farm.RemoveEventListener(OnEntityEvent);
		}
		m_Farm = cSFarm;
		m_Farm.CreatePlantEvent += OnCreatePlant;
		m_Farm.RemovePlantEvent += OnRemovePlant;
		m_Farm.CreatePlantListEvent += OnCreatAllPlants;
		m_Farm.ClearAllPlantEvent += OnClearAllPlants;
		m_Farm.AddEventListener(OnEntityEvent);
		int num = 0;
		foreach (FarmPlantLogic value in cSFarm.Plants.Values)
		{
			if (num >= m_PlantGrids.Count)
			{
				CSUI_PlantGrid cSUI_PlantGrid = _createPlantGrid(value);
				UICheckbox component = cSUI_PlantGrid.gameObject.GetComponent<UICheckbox>();
				if (num == 0)
				{
					component.startsChecked = true;
				}
			}
			num++;
		}
		int num2 = num;
		while (num2 < m_PlantGrids.Count)
		{
			UnityEngine.Object.Destroy(m_PlantGrids[num2].gameObject);
			m_PlantGrids.RemoveAt(num2);
		}
		m_PlantPart.m_Root.repositionNow = true;
		for (int i = 0; i < 12; i++)
		{
			ItemObject plantSeed = m_Farm.GetPlantSeed(i);
			m_SeedsGrids[i].m_Grid.SetItem(plantSeed);
		}
		for (int j = 0; j < 2; j++)
		{
			ItemObject plantTool = m_Farm.GetPlantTool(j);
			m_ToolsGrids[j].m_Grid.SetItem(plantTool);
		}
	}

	private void OnPlantGridDestroySelf(CSUI_PlantGrid plantGrid)
	{
		m_PlantGrids.Remove(plantGrid);
		m_PlantPart.m_Root.repositionNow = true;
	}

	public void Init()
	{
		if (m_SeedsGrids.Count != 0)
		{
			return;
		}
		for (int i = 0; i < 12; i++)
		{
			CSUI_Grid cSUI_Grid = _createGrid(m_SeedsRoot.transform, i);
			cSUI_Grid.onCheckItem = OnGridCheckItem;
			cSUI_Grid.OnItemChanged = OnSeedGridItemChanged;
			m_SeedsGrids.Add(cSUI_Grid);
			if (GameConfig.IsMultiMode)
			{
				cSUI_Grid.OnDropItemMulti = OnSeedsDropItemMulti;
				cSUI_Grid.OnLeftMouseClickedMulti = OnSeedsLeftMouseClickedMulti;
				cSUI_Grid.OnRightMouseClickedMulti = OnSeedsRightMouseClickedMulti;
			}
		}
		m_SeedsRoot.repositionNow = true;
		for (int j = 0; j < 2; j++)
		{
			CSUI_Grid cSUI_Grid2 = _createGrid(m_ToolsRoot.transform, j);
			switch (j)
			{
			case 0:
				cSUI_Grid2.onCheckItem = OnGridCheckItem_ToolWater;
				cSUI_Grid2.m_Grid.mScript.spriteName = "blackico_water";
				break;
			case 1:
				cSUI_Grid2.onCheckItem = OnGridCheckItem_ToolWeed;
				cSUI_Grid2.m_Grid.mScript.spriteName = "blackico_herbicide";
				break;
			}
			cSUI_Grid2.OnItemChanged = OnToolsGridItemChanged;
			m_ToolsGrids.Add(cSUI_Grid2);
			if (GameConfig.IsMultiMode)
			{
				cSUI_Grid2.OnDropItemMulti = OnToolsDropItemMulti;
				cSUI_Grid2.OnLeftMouseClickedMulti = OnToolsLeftMouseClickedMulti;
				cSUI_Grid2.OnRightMouseClickedMulti = OnToolsRightMouseClickedMulti;
			}
		}
		m_ToolsRoot.repositionNow = true;
	}

	private CSUI_Grid _createGrid(Transform parent, int index = -1)
	{
		CSUI_Grid cSUI_Grid = UnityEngine.Object.Instantiate(m_GridPrefab);
		cSUI_Grid.transform.parent = parent;
		CSUtils.ResetLoacalTransform(cSUI_Grid.transform);
		cSUI_Grid.m_Index = index;
		return cSUI_Grid;
	}

	private CSUI_PlantGrid _createPlantGrid(FarmPlantLogic p)
	{
		CSUI_PlantGrid cSUI_PlantGrid = UnityEngine.Object.Instantiate(m_PlantPart.m_PlantGridPrefab);
		cSUI_PlantGrid.transform.parent = m_PlantPart.m_Root.transform;
		CSUtils.ResetLoacalTransform(cSUI_PlantGrid.transform);
		cSUI_PlantGrid.m_Plant = p;
		cSUI_PlantGrid.OnDestroySelf = OnPlantGridDestroySelf;
		string[] icon = PeSingleton<ItemProto.Mgr>.Instance.Get(cSUI_PlantGrid.m_Plant.protoTypeId).icon;
		if (icon.Length != 0)
		{
			cSUI_PlantGrid.IconSpriteName = icon[0];
		}
		else
		{
			cSUI_PlantGrid.IconSpriteName = string.Empty;
		}
		m_PlantGrids.Add(cSUI_PlantGrid);
		UICheckbox component = cSUI_PlantGrid.gameObject.GetComponent<UICheckbox>();
		component.radioButtonRoot = m_PlantPart.m_Root.transform;
		component.startsChecked = false;
		return cSUI_PlantGrid;
	}

	public void UpdateNpcGrids()
	{
		for (int i = 0; i < m_NpcGrids.Count; i++)
		{
			CSPersonnel npc = m_Farm.Worker(i);
			m_NpcGrids[i].m_Npc = npc;
		}
	}

	private bool OnGridCheckItem(ItemObject item, CSUI_Grid.ECheckItemType check_type)
	{
		if (item == null)
		{
			return true;
		}
		if (PlantInfo.GetPlantInfoByItemId(item.protoId) != null)
		{
			return true;
		}
		CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mNotPlantSeed.GetString(), item.protoData.GetName()), Color.red);
		return false;
	}

	private void OnSeedGridItemChanged(ItemObject item, ItemObject oldItem, int index)
	{
		m_Farm.SetPlantSeed(index, item);
		if (oldItem != null)
		{
			if (item == null)
			{
				CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mTakeAwayFromMachine.GetString(), oldItem.protoData.GetName(), CSUtils.GetEntityName(7)));
			}
			else if (item == oldItem)
			{
				CSUI_MainWndCtrl.ShowStatusBar(UIMsgBoxInfo.mNotEnoughGrid.GetString(), Color.red);
			}
			else
			{
				CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mPutIntoMachine.GetString(), item.protoData.GetName(), CSUtils.GetEntityName(7)));
			}
		}
		else if (item != null)
		{
			CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mPutIntoMachine.GetString(), item.protoData.GetName(), CSUtils.GetEntityName(7)));
		}
	}

	private bool OnGridCheckItem_ToolWater(ItemObject item, CSUI_Grid.ECheckItemType check_type)
	{
		if (item == null)
		{
			return true;
		}
		if (item.protoId == 1003)
		{
			return true;
		}
		CSUI_MainWndCtrl.ShowStatusBar(UIMsgBoxInfo.mOnlyCanPutWater.GetString(), Color.red);
		return false;
	}

	private bool OnGridCheckItem_ToolWeed(ItemObject item, CSUI_Grid.ECheckItemType check_type)
	{
		if (item == null)
		{
			return true;
		}
		if (item.protoId == 1002)
		{
			return true;
		}
		CSUI_MainWndCtrl.ShowStatusBar(UIMsgBoxInfo.mOnlyCanPutInsecticider.GetString(), Color.red);
		return false;
	}

	private void OnToolsGridItemChanged(ItemObject item, ItemObject oldItem, int index)
	{
		m_Farm.SetPlantTool(index, item);
		if (oldItem != null)
		{
			if (item == null)
			{
				CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mTakeAwayFromMachine.GetString(), oldItem.protoData.GetName(), CSUtils.GetEntityName(7)));
			}
			else if (item == oldItem)
			{
				CSUI_MainWndCtrl.ShowStatusBar(UIMsgBoxInfo.mNotEnoughGrid.GetString(), Color.red);
			}
			else
			{
				CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mPutIntoMachine.GetString(), item.protoData.GetName(), CSUtils.GetEntityName(7)));
			}
		}
		else if (item != null)
		{
			CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mPutIntoMachine.GetString(), item.protoData.GetName(), CSUtils.GetEntityName(7)));
		}
	}

	private void OnCreatePlant(FarmPlantLogic plant)
	{
		if (plant != null)
		{
			_createPlantGrid(plant);
			m_PlantPart.m_Root.repositionNow = true;
		}
	}

	private void OnRemovePlant(FarmPlantLogic plant)
	{
		if (plant != null)
		{
			int num = m_PlantGrids.FindIndex((CSUI_PlantGrid item0) => item0.m_Plant == plant);
			if (num != -1)
			{
				UnityEngine.Object.DestroyImmediate(m_PlantGrids[num].gameObject);
				m_PlantGrids.RemoveAt(num);
				m_PlantPart.m_Root.repositionNow = true;
			}
		}
	}

	private void OnClearAllPlants()
	{
		for (int i = 0; i < m_PlantGrids.Count; i++)
		{
			UnityEngine.Object.DestroyImmediate(m_PlantGrids[i].gameObject);
		}
		m_PlantGrids.Clear();
		m_PlantPart.m_Root.repositionNow = true;
	}

	private void OnCreatAllPlants(List<FarmPlantLogic> _list)
	{
		if (_list == null)
		{
			return;
		}
		for (int i = 0; i < _list.Count; i++)
		{
			if (_list[i] != null)
			{
				_createPlantGrid(_list[i]);
			}
		}
		m_PlantPart.m_Root.repositionNow = true;
	}

	private void OnAutoSettleBtn()
	{
		if (m_Farm != null)
		{
			m_Farm.AutoSettleWorkers();
		}
	}

	private void OnDisbandAllBtn()
	{
		if (m_Farm != null)
		{
			m_Farm.ClearWorkers();
		}
	}

	private void OnSequentialActive(bool active)
	{
		if (GameConfig.IsMultiMode)
		{
			if (m_Entity != null)
			{
				m_Entity._ColonyObj._Network.SetSequentialActive(active);
			}
		}
		else if (m_Farm != null)
		{
			m_Farm.Data.m_SequentialPlanting = active;
			if (active)
			{
				CSUI_MainWndCtrl.ShowStatusBar(UIMsgBoxInfo.mPlantSequence.GetString());
			}
		}
	}

	private void OnEntityEvent(int event_type, CSEntity cse, object arg)
	{
		if (event_type == 6001)
		{
			int index = (int)arg;
			if (m_SeedsGrids[index].m_Grid.Item != null && m_SeedsGrids[index].m_Grid.Item.GetCount() <= 0)
			{
				m_SeedsGrids[index].m_Grid.SetItem(null);
			}
		}
	}

	private void OnEnable()
	{
		SetFarm(m_Farm);
	}

	private void Awake()
	{
	}

	private void Start()
	{
	}

	private void Update()
	{
		UpdateNpcGrids();
		m_ToolsGrids[0].m_Grid.mScript.spriteName = "blackico_water";
		m_ToolsGrids[1].m_Grid.mScript.spriteName = "blackico_herbicide";
	}

	public void OnSeedsDropItemMulti(Grid_N grid, int m_Index)
	{
		ItemObject itemObj = SelectItem_N.Instance.ItemObj;
		if (itemObj != null && m_Entity != null && m_Entity._ColonyObj != null && null != m_Entity._ColonyObj._Network)
		{
			m_Entity._ColonyObj._Network.SetPlantSeed(m_Index, itemObj.instanceId);
		}
	}

	public void OnSeedsLeftMouseClickedMulti(Grid_N grid, int m_Index)
	{
	}

	public void OnSeedsRightMouseClickedMulti(Grid_N grid, int m_Index)
	{
		m_Entity._ColonyObj._Network.FetchSeedItem(m_Index);
	}

	public void OnToolsDropItemMulti(Grid_N grid, int m_Index)
	{
		ItemObject itemObj = SelectItem_N.Instance.ItemObj;
		if (itemObj != null && m_Entity != null && m_Entity._ColonyObj != null && null != m_Entity._ColonyObj._Network)
		{
			m_Entity._ColonyObj._Network.SetPlantTool(m_Index, itemObj.instanceId);
		}
	}

	public void OnToolsLeftMouseClickedMulti(Grid_N grid, int m_Index)
	{
	}

	public void OnToolsRightMouseClickedMulti(Grid_N grid, int m_Index)
	{
		m_Entity._ColonyObj._Network.FetchToolItem(m_Index);
	}

	public void SetPlantToolResult(bool success, int objId, int index, CSEntity entity)
	{
		if (success && m_Entity == entity)
		{
			ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(objId);
			ItemObject itemObj = m_ToolsGrids[index].m_Grid.ItemObj;
			CSUI_Grid cSUI_Grid = m_ToolsGrids[index];
			cSUI_Grid.m_Grid.SetItem(itemObject);
			if (cSUI_Grid.OnItemChanged != null)
			{
				cSUI_Grid.OnItemChanged(itemObject, itemObj, cSUI_Grid.m_Index);
			}
		}
	}

	public void SetPlantSeedResult(bool success, int objId, int index, CSEntity entity)
	{
		if (success && m_Entity == entity)
		{
			ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(objId);
			ItemObject itemObj = m_SeedsGrids[index].m_Grid.ItemObj;
			CSUI_Grid cSUI_Grid = m_SeedsGrids[index];
			cSUI_Grid.m_Grid.SetItem(itemObject);
			if (cSUI_Grid.OnItemChanged != null)
			{
				cSUI_Grid.OnItemChanged(itemObject, itemObj, cSUI_Grid.m_Index);
			}
		}
	}

	public void FetchSeedResult(bool success, int index, CSEntity entity)
	{
		if (success && m_Entity == entity)
		{
			ItemObject itemObject = null;
			ItemObject itemObj = m_SeedsGrids[index].m_Grid.ItemObj;
			CSUI_Grid cSUI_Grid = m_SeedsGrids[index];
			GameUI.Instance.mItemPackageCtrl.ResetItem();
			cSUI_Grid.m_Grid.SetItem(itemObject);
			if (cSUI_Grid.OnItemChanged != null)
			{
				cSUI_Grid.OnItemChanged(itemObject, itemObj, cSUI_Grid.m_Index);
			}
		}
	}

	public void FetchToolResult(bool success, int index, CSEntity entity)
	{
		if (success && m_Entity == entity)
		{
			ItemObject itemObject = null;
			ItemObject itemObj = m_ToolsGrids[index].m_Grid.ItemObj;
			CSUI_Grid cSUI_Grid = m_ToolsGrids[index];
			GameUI.Instance.mItemPackageCtrl.ResetItem();
			cSUI_Grid.m_Grid.SetItem(itemObject);
			if (cSUI_Grid.OnItemChanged != null)
			{
				cSUI_Grid.OnItemChanged(itemObject, itemObj, cSUI_Grid.m_Index);
			}
		}
	}

	public void SetSequentialActiveResult(bool active, CSEntity entity)
	{
		if (m_Entity == entity)
		{
			m_SequentialPlantingCB.isChecked = m_Farm.Data.m_SequentialPlanting;
			if (active)
			{
				CSUI_MainWndCtrl.ShowStatusBar(UIMsgBoxInfo.mPlantSequence.GetString());
			}
		}
	}

	public void DeleteSeedResult(CSEntity entity, int objId, int index)
	{
		if (m_Entity != entity)
		{
			return;
		}
		for (int i = 0; i < m_SeedsGrids.Count; i++)
		{
			if (m_SeedsGrids[i].m_Grid.ItemObj != null && m_ToolsGrids[i].m_Grid.ItemObj.instanceId == objId)
			{
				m_SeedsGrids[i].m_Grid.SetItem(null);
			}
		}
		ItemObject plantSeed = m_Farm.GetPlantSeed(index);
		if (plantSeed != null)
		{
			plantSeed.DecreaseStackCount(1);
			if (plantSeed.GetCount() <= 0)
			{
				m_Farm.ExcuteEvent(6001, index);
			}
		}
		m_Farm.SetPlantSeed(index, null);
	}

	public void DeleteToolResult(CSEntity entity, int objId)
	{
		if (m_Entity != entity)
		{
			return;
		}
		for (int i = 0; i < m_ToolsGrids.Count; i++)
		{
			if (m_ToolsGrids[i].m_Grid.ItemObj != null && m_ToolsGrids[i].m_Grid.ItemObj.instanceId == objId)
			{
				m_ToolsGrids[i].m_Grid.SetItem(null);
			}
		}
	}

	public void RefreshTools()
	{
		if (m_ToolsGrids.Count != 0 && m_Farm != null)
		{
			for (int i = 0; i < 2; i++)
			{
				ItemObject plantTool = m_Farm.GetPlantTool(i);
				m_ToolsGrids[i].m_Grid.SetItem(plantTool);
			}
		}
	}
}
