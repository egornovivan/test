using ItemAsset;
using Pathea;
using UnityEngine;

public class CSUI_Grid : MonoBehaviour
{
	public enum ECheckItemType
	{
		OnDrop,
		LeftMouseClick,
		RightMounseClick
	}

	public delegate bool CheckItemDelegate(ItemObject item, ECheckItemType checkType);

	public delegate void ItemChangedDelegate(ItemObject item, ItemObject oldItem, int index);

	public delegate void OnMultiOperation(Grid_N grid, int index);

	[SerializeField]
	private Grid_N m_GridPrefab;

	[HideInInspector]
	public Grid_N m_Grid;

	[SerializeField]
	private TweenPosition m_GlowTween;

	public UIButtonTween m_TargetBT;

	public UILabel m_ZeroNumLab;

	public bool bUseZeroLab;

	public int m_Index = -1;

	public bool m_Active = true;

	public bool m_UseDefaultExchangeDel = true;

	public CheckItemDelegate onCheckItem;

	public ItemChangedDelegate OnItemChanged;

	public ItemChangedDelegate ItemExchanged;

	public OnMultiOperation OnDropItemMulti;

	public OnMultiOperation OnLeftMouseClickedMulti;

	public OnMultiOperation OnRightMouseClickedMulti;

	public void PlayGlow(bool forward)
	{
		m_GlowTween.gameObject.SetActive(value: true);
		m_GlowTween.Reset();
		m_GlowTween.Play(forward: true);
	}

	private void OnTweenFinished(UITweener tween)
	{
		m_GlowTween.gameObject.SetActive(value: false);
	}

	private void OnDropItem(Grid_N grid)
	{
		if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_HotKeyBar && SelectItem_N.Instance.Place == ItemPlaceType.IPT_Null)
		{
			SelectItem_N.Instance.SetItem(null);
		}
		else
		{
			if (onCheckItem != null && !onCheckItem(SelectItem_N.Instance.ItemObj, ECheckItemType.OnDrop))
			{
				return;
			}
			if (GameConfig.IsMultiMode)
			{
				OnDropItemMulti(grid, m_Index);
				return;
			}
			ItemObject itemObj = SelectItem_N.Instance.ItemObj;
			if (itemObj == null)
			{
				return;
			}
			ItemObject itemObj2 = grid.ItemObj;
			ItemObject itemObj3 = grid.ItemObj;
			if (itemObj2 == null)
			{
				grid.SetItem(itemObj);
				if (OnItemChanged != null)
				{
					OnItemChanged(itemObj, itemObj3, m_Index);
				}
				SelectItem_N.Instance.RemoveOriginItem();
				SelectItem_N.Instance.SetItem(null);
				return;
			}
			PlayerPackageCmpt cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
			ItemPackage.ESlotType slotType = ItemPackage.GetSlotType(itemObj2.protoId);
			ItemPackage.ESlotType slotType2 = ItemPackage.GetSlotType(itemObj.protoId);
			if (slotType == slotType2 && null != SelectItem_N.Instance.Grid)
			{
				if (SelectItem_N.Instance.Grid.onGridsExchangeItem != null)
				{
					SelectItem_N.Instance.Grid.onGridsExchangeItem(SelectItem_N.Instance.Grid, itemObj2);
					grid.SetItem(itemObj);
					SelectItem_N.Instance.SetItem(null);
					if (OnItemChanged != null)
					{
						OnItemChanged(itemObj, itemObj3, m_Index);
					}
				}
			}
			else if (cmpt.package.CanAdd(itemObj2))
			{
				cmpt.package.AddItem(itemObj2);
				grid.SetItem(itemObj);
				SelectItem_N.Instance.RemoveOriginItem();
				SelectItem_N.Instance.SetItem(null);
			}
			if (OnItemChanged != null)
			{
				OnItemChanged(itemObj, itemObj3, m_Index);
			}
		}
	}

	private void OnGridsExchangeItem(Grid_N grid, ItemObject item)
	{
		grid.SetItem(item);
	}

	private void OnLeftMouseClicked(Grid_N grid)
	{
		if (grid.Item != null && (onCheckItem == null || onCheckItem(null, ECheckItemType.LeftMouseClick)))
		{
			if (GameConfig.IsMultiMode)
			{
				OnLeftMouseClickedMulti(grid, m_Index);
			}
			else
			{
				SelectItem_N.Instance.SetItemGrid(grid);
			}
		}
	}

	private void OnRightMouseClicked(Grid_N grid)
	{
		if (grid.Item == null || (onCheckItem != null && !onCheckItem(null, ECheckItemType.RightMounseClick)))
		{
			return;
		}
		if (GameConfig.IsMultiMode)
		{
			OnRightMouseClickedMulti(grid, m_Index);
			return;
		}
		ItemObject itemObj = grid.ItemObj;
		PlayerPackageCmpt cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
		if (cmpt.package.CanAdd(grid.ItemObj))
		{
			cmpt.Add(grid.ItemObj);
			GameUI.Instance.mItemPackageCtrl.ResetItem();
			grid.SetItem(null);
			if (OnItemChanged != null)
			{
				OnItemChanged(null, itemObj, m_Index);
			}
		}
		else
		{
			CSUI_MainWndCtrl.ShowStatusBar(UIMsgBoxInfo.mNotEnoughGrid.GetString(), Color.red);
		}
	}

	private void OnRemoveOriginItem(Grid_N grid)
	{
		ItemObject itemObj = grid.ItemObj;
		grid.SetItem(null);
		if (OnItemChanged != null)
		{
			OnItemChanged(null, itemObj, m_Index);
		}
	}

	private void Awake()
	{
		m_Grid = Object.Instantiate(m_GridPrefab);
		m_Grid.transform.parent = base.transform;
		m_Grid.transform.localPosition = Vector3.zero;
		m_Grid.transform.localRotation = Quaternion.identity;
	}

	private void Start()
	{
		if (m_TargetBT == null)
		{
			m_GlowTween.onFinished = OnTweenFinished;
		}
		m_ZeroNumLab.enabled = false;
		if (m_Active)
		{
			m_Grid.SetItemPlace(ItemPlaceType.IPT_Null, 0);
			m_Grid.onDropItem = OnDropItem;
			m_Grid.onLeftMouseClicked = OnLeftMouseClicked;
			m_Grid.onRightMouseClicked = OnRightMouseClicked;
			m_Grid.onRemoveOriginItem = OnRemoveOriginItem;
		}
		if (m_UseDefaultExchangeDel)
		{
			m_Grid.onGridsExchangeItem = OnGridsExchangeItem;
		}
	}

	private void Update()
	{
		if (bUseZeroLab)
		{
			if (m_Grid.Item != null)
			{
				m_ZeroNumLab.enabled = m_Grid.Item.stackCount == 0;
			}
			else
			{
				m_ZeroNumLab.enabled = false;
			}
		}
		else
		{
			m_ZeroNumLab.enabled = false;
		}
	}
}
