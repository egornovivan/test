using ItemAsset;
using Pathea;
using UnityEngine;

public class CSUI_ChargingGrid : MonoBehaviour
{
	public delegate void OnItemChangedDel(int index, ItemObject item);

	public delegate bool OnItemCheck(ItemObject item);

	public delegate void OnMultiOperation(int index, Grid_N grid);

	public bool m_bCanChargeLargedItem = true;

	[SerializeField]
	private Grid_N m_GridPrefab;

	[SerializeField]
	private UISlider m_Silder;

	[SerializeField]
	private UILabel m_Label;

	[SerializeField]
	private UISlicedSprite m_EmptySprite;

	private Grid_N m_Grid;

	public Energy energyItem;

	public int m_Index;

	public bool m_bUseMsgBox = true;

	public OnItemChangedDel onItemChanded;

	public OnItemCheck onItemCheck;

	public OnMultiOperation OnDropItemMulti;

	public OnMultiOperation OnLeftMouseClickedMulti;

	public OnMultiOperation OnRightMouseClickedMulti;

	public ItemObject Item => m_Grid.ItemObj;

	public bool IsChargeable(ItemObject itemObj)
	{
		if (itemObj == null)
		{
			return false;
		}
		Energy cmpt = itemObj.GetCmpt<Energy>();
		if (cmpt == null || itemObj.protoData.unchargeable)
		{
			if (m_bUseMsgBox)
			{
				PeTipMsg.Register(PELocalization.GetString(8000094), PeTipMsg.EMsgLevel.Error);
			}
			else
			{
				CSUI_MainWndCtrl.ShowStatusBar(PELocalization.GetString(8000094), Color.red);
			}
			return false;
		}
		if (!(cmpt is EnergySmall) && !m_bCanChargeLargedItem)
		{
			if (m_bUseMsgBox)
			{
				PeTipMsg.Register(PELocalization.GetString(8000095), PeTipMsg.EMsgLevel.Warning);
			}
			else
			{
				CSUI_MainWndCtrl.ShowStatusBar(PELocalization.GetString(8000095), Color.red);
			}
			return false;
		}
		return true;
	}

	public void SetItemUI(ItemObject itemObj)
	{
		m_Grid.SetItem(itemObj);
		if (itemObj != null)
		{
			energyItem = itemObj.GetCmpt<Energy>();
		}
		else
		{
			energyItem = null;
		}
	}

	public bool SetItem(ItemObject itemObj)
	{
		if (itemObj == null)
		{
			if (onItemChanded != null)
			{
				onItemChanded(m_Index, itemObj);
			}
			m_Grid.SetItem(itemObj);
			energyItem = null;
			return true;
		}
		if (!IsChargeable(itemObj))
		{
			return false;
		}
		if (onItemChanded != null)
		{
			onItemChanded(m_Index, itemObj);
		}
		m_Grid.SetItem(itemObj);
		energyItem = itemObj.GetCmpt<Energy>();
		return true;
	}

	private void OnDropItem(Grid_N grid)
	{
		if (SelectItem_N.Instance.Place != ItemPlaceType.IPT_Bag || (onItemCheck != null && !onItemCheck(grid.ItemObj)))
		{
			return;
		}
		if (grid.Item == null)
		{
			ItemPlaceType place = SelectItem_N.Instance.Place;
			if (place == ItemPlaceType.IPT_HotKeyBar)
			{
				SelectItem_N.Instance.SetItem(null);
			}
			else if (SetItem(SelectItem_N.Instance.ItemObj))
			{
				if (GameConfig.IsMultiMode && OnDropItemMulti != null)
				{
					OnDropItemMulti(m_Index, grid);
				}
				SelectItem_N.Instance.RemoveOriginItem();
				SelectItem_N.Instance.SetItem(null);
				if (!m_bUseMsgBox)
				{
					CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mStartToCharge.GetString(), grid.Item.protoData.GetName()));
				}
			}
			else if (!m_bUseMsgBox)
			{
				CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mNotToBeCharged.GetString(), SelectItem_N.Instance.ItemObj.protoData.GetName()), Color.red);
			}
			return;
		}
		ItemObject itemObj = grid.ItemObj;
		PlayerPackageCmpt cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
		if (cmpt.package.CanAdd(itemObj) && SetItem(SelectItem_N.Instance.ItemObj))
		{
			cmpt.package.AddItem(itemObj);
			SelectItem_N.Instance.RemoveOriginItem();
			SelectItem_N.Instance.SetItem(null);
			if (!m_bUseMsgBox && grid.Item != null)
			{
				CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mStartToCharge.GetString(), grid.Item.protoData.GetName()));
			}
		}
		else if (!m_bUseMsgBox)
		{
			CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mNotToBeCharged.GetString(), SelectItem_N.Instance.ItemObj.protoData.GetName()), Color.red);
		}
	}

	private void OnLeftMouseClicked(Grid_N grid)
	{
		if (GameConfig.IsMultiMode)
		{
			if (OnLeftMouseClickedMulti != null)
			{
				OnLeftMouseClickedMulti(m_Index, grid);
			}
		}
		else if ((onItemCheck == null || onItemCheck(grid.ItemObj)) && grid.Item != null)
		{
			SelectItem_N.Instance.SetItemGrid(grid);
		}
	}

	private void OnRightMouseClicked(Grid_N grid)
	{
		if (GameConfig.IsMultiMode)
		{
			if (OnRightMouseClickedMulti != null)
			{
				OnRightMouseClickedMulti(m_Index, grid);
			}
		}
		else if (onItemCheck == null || onItemCheck(grid.ItemObj))
		{
			GameUI.Instance.mItemPackageCtrl.Show();
			if (grid.ItemObj != null && ItemPackage.InvalidIndex != PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>().package.AddItem(grid.ItemObj))
			{
				GameUI.Instance.mItemPackageCtrl.ResetItem();
				SetItem(null);
			}
		}
	}

	private void OnRemoveOriginItem(Grid_N grid)
	{
		SetItem(null);
	}

	private void Awake()
	{
		m_Grid = Object.Instantiate(m_GridPrefab);
		m_Grid.transform.parent = base.transform;
		m_Grid.transform.localPosition = Vector3.zero;
		m_Grid.transform.localRotation = Quaternion.identity;
		m_Grid.transform.localScale = Vector3.one;
		m_Grid.onDropItem = OnDropItem;
		m_Grid.onLeftMouseClicked = OnLeftMouseClicked;
		m_Grid.onRightMouseClicked = OnRightMouseClicked;
		m_Grid.onRemoveOriginItem = OnRemoveOriginItem;
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (energyItem != null)
		{
			m_Silder.enabled = true;
			m_Label.enabled = true;
			m_EmptySprite.enabled = false;
			float percent = energyItem.energy.percent;
			m_Silder.sliderValue = percent;
			m_Label.text = Mathf.FloorToInt(percent * 100f) + "%";
		}
		else
		{
			m_Silder.enabled = false;
			m_Label.enabled = false;
			m_EmptySprite.enabled = true;
			m_Silder.sliderValue = 0f;
		}
	}
}
