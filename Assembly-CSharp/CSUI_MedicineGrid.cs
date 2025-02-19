using ItemAsset;
using UnityEngine;

public class CSUI_MedicineGrid : MonoBehaviour
{
	public delegate void RealOp(Grid_N grid);

	public delegate void MedicineDragDel(Grid_N grid);

	public bool m_CanDragMedicine = true;

	[SerializeField]
	private Grid_N m_GridPrefab;

	public Grid_N m_Grid;

	[SerializeField]
	public UILabel m_NeedCntLb;

	private int m_NeedCnt;

	public int NeedCnt
	{
		get
		{
			return m_NeedCnt;
		}
		set
		{
			m_NeedCnt = value;
			m_NeedCntLb.text = "X " + m_NeedCnt;
		}
	}

	public int ItemNum
	{
		get
		{
			if (m_Grid.Item == null)
			{
				return 0;
			}
			return m_Grid.Item.stackCount;
		}
		set
		{
			if (m_Grid.Item != null)
			{
				m_Grid.Item.stackCount = value;
			}
		}
	}

	public int ItemID
	{
		get
		{
			if (m_Grid.Item == null)
			{
				return 0;
			}
			return m_Grid.Item.protoId;
		}
		set
		{
			if (value <= 0)
			{
				m_Grid.SetItem(null);
			}
			else
			{
				m_Grid.SetItem(new ItemSample(value, 0));
			}
		}
	}

	public event RealOp mRealOp;

	private void OnDropItem(Grid_N grid)
	{
		if (m_CanDragMedicine && this.mRealOp != null)
		{
			this.mRealOp(grid);
		}
	}

	private void OnLeftMouseClicked(Grid_N grid)
	{
		if (m_CanDragMedicine && grid.Item != null)
		{
			SelectItem_N.Instance.SetItemGrid(grid);
		}
	}

	private void onRemoveOriginItem(Grid_N grid)
	{
		grid.SetItem(null);
	}

	private void Awake()
	{
	}

	private void Start()
	{
		m_Grid.SetItemPlace(ItemPlaceType.IPT_Hospital, 0);
		m_Grid.onDropItem = OnDropItem;
		m_Grid.onLeftMouseClicked = OnLeftMouseClicked;
		m_Grid.onRemoveOriginItem = onRemoveOriginItem;
	}

	private void Update()
	{
	}
}
