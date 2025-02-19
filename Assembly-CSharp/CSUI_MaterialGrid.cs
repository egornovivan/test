using ItemAsset;
using UnityEngine;

public class CSUI_MaterialGrid : MonoBehaviour
{
	[SerializeField]
	private CSUI_Grid m_GridPrefab;

	private CSUI_Grid m_Grid;

	[SerializeField]
	public UILabel m_NeedCntLb;

	private int m_NeedCnt;

	public int MaxCnt;

	public bool bUseColors = true;

	public int ItemID
	{
		get
		{
			if (m_Grid.m_Grid.Item == null)
			{
				return 0;
			}
			return m_Grid.m_Grid.Item.protoId;
		}
		set
		{
			if (value <= 0)
			{
				m_Grid.m_Grid.SetItem(null);
			}
			else
			{
				m_Grid.m_Grid.SetItem(new ItemSample(value, 0));
			}
		}
	}

	public CSUI_Grid Grid => m_Grid;

	public int ItemNum
	{
		get
		{
			if (m_Grid.m_Grid.Item == null)
			{
				return 0;
			}
			return m_Grid.m_Grid.Item.stackCount;
		}
		set
		{
			if (m_Grid.m_Grid.Item != null)
			{
				m_Grid.m_Grid.Item.stackCount = value;
			}
		}
	}

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

	private void InitGrid()
	{
		m_Grid = Object.Instantiate(m_GridPrefab);
		m_Grid.transform.parent = base.transform;
		m_Grid.transform.localPosition = Vector3.zero;
		m_Grid.transform.localRotation = Quaternion.identity;
		m_Grid.transform.localScale = Vector3.one;
		m_Grid.m_Grid.SetItem(null);
		m_Grid.bUseZeroLab = true;
		m_Grid.m_Active = false;
	}

	private void Awake()
	{
		InitGrid();
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (bUseColors)
		{
			if (m_NeedCnt > ItemNum)
			{
				m_Grid.m_Grid.mNumCount.color = new Color(1f, 0.1f, 0f);
			}
			else
			{
				m_Grid.m_Grid.mNumCount.color = new Color(0.1f, 1f, 0f);
			}
			m_Grid.bUseZeroLab = bUseColors;
		}
		else
		{
			m_Grid.m_Grid.mNumCount.color = Color.white;
			m_Grid.bUseZeroLab = false;
		}
	}
}
