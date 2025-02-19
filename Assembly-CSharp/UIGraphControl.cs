using System.Collections.Generic;
using ItemAsset;
using Pathea;
using UnityEngine;

public class UIGraphControl : MonoBehaviour
{
	public GameObject mContent;

	public float mWndWidth = 450f;

	public float mWndHeight = 200f;

	public float mGraphPos_x = 30f;

	public float mGraphPos_y = -30f;

	public float mGrphLineWidth = 36f;

	public GameObject mPrefabGraphLine_H;

	public GameObject mPrefabGraphLine_V;

	public GameObject mPrefabGraphItem;

	public UIScrollBar mScrollBar_V;

	public int mSelectedIndex = -1;

	public List<UIGraphNode> mGraphItemList = new List<UIGraphNode>();

	private List<GameObject> GraphLineList = new List<GameObject>();

	public UIGraphNode rootNode;

	private float mGraphItemHeight;

	private float nodeIndex0Pos;

	private int tempCount;

	public void DrawGraph()
	{
		UpdateGraph();
		mGraphPos_x = mWndWidth / 2f - rootNode.mTreeGrid.m_Content.transform.localPosition.x + 16f;
		mContent.transform.localPosition = new Vector3(mGraphPos_x, 10f, 0f);
		mContent.SetActive(value: true);
		mSelectedIndex = 0;
		rootNode.mCtrl.SetSelected(isSelected: true);
		Invoke("SetScrolbarVale", 0.1f);
	}

	public void UpdateGraphCount()
	{
		for (int i = 0; i < mGraphItemList.Count; i++)
		{
			mGraphItemList[i].mCtrl.SetCount(mGraphItemList[i].needCount, mGraphItemList[i].bagCount, mGraphItemList[i].getCount);
		}
	}

	private void SetScrolbarVale()
	{
		mScrollBar_V.scrollValue = 0f;
	}

	private void UpdateGraph()
	{
		if (rootNode != null && rootNode.mTreeGrid != null)
		{
			rootNode.mTreeGrid.Reposition();
			SetGraphCount(rootNode.ms.m_productItemCount);
		}
	}

	public void SetGraphCount(int rootNodecount)
	{
		Replicator replicator = GetReplicator();
		if (replicator == null)
		{
			return;
		}
		int num = rootNodecount / rootNode.ms.m_productItemCount;
		rootNode.getCount = rootNodecount;
		int num2 = 0;
		for (int i = 0; i < mGraphItemList.Count; i++)
		{
			if (mGraphItemList[i].mPartent == rootNode)
			{
				mGraphItemList[i].needCount = rootNode.ms.materials[num2].itemCount * num;
				num2++;
			}
			mGraphItemList[i].bagCount = replicator.GetItemCount(mGraphItemList[i].GetItemID());
		}
		UpdateGraphCount();
		if ((bool)GameUI.Instance && GameUI.Instance.mItemsTrackWnd.ContainsScript(rootNode.ms.id))
		{
			GameUI.Instance.mItemsTrackWnd.UpdateOrAddScript(rootNode.ms, num);
		}
	}

	public static Replicator GetReplicator()
	{
		PeEntity mainPlayer = PeSingleton<PeCreature>.Instance.mainPlayer;
		if (null == mainPlayer)
		{
			return null;
		}
		ReplicatorCmpt cmpt = mainPlayer.GetCmpt<ReplicatorCmpt>();
		if (null == cmpt)
		{
			return null;
		}
		return cmpt.replicator;
	}

	public int GetMinCount()
	{
		if (rootNode == null || rootNode.ms == null)
		{
			return 1;
		}
		Replicator replicator = GetReplicator();
		if (replicator == null)
		{
			return rootNode.ms.m_productItemCount;
		}
		int num = replicator.MinProductCount(rootNode.ms.id);
		return ((num == 0) ? 1 : num) * rootNode.ms.m_productItemCount;
	}

	public int GetMaxCount()
	{
		if (rootNode == null || rootNode.ms == null)
		{
			return 1;
		}
		Replicator replicator = GetReplicator();
		if (replicator == null)
		{
			return rootNode.ms.m_productItemCount;
		}
		int a = replicator.MaxProductCount(rootNode.ms.id) * rootNode.ms.m_productItemCount;
		int stackMax = ItemProto.GetStackMax(rootNode.GetItemID());
		a = ((stackMax != 1) ? Mathf.Min(a, 168 * stackMax) : Mathf.Min(a, 168));
		return (a == 0) ? 1 : a;
	}

	public bool isCanCreate()
	{
		if (mGraphItemList.Count == 0)
		{
			return false;
		}
		Replicator replicator = GetReplicator();
		if (replicator == null)
		{
			return false;
		}
		Replicator.Formula formula = PeSingleton<Replicator.Formula.Mgr>.Instance.Find(rootNode.ms.id);
		if (formula == null)
		{
			return false;
		}
		ItemProto itemData = ItemProto.GetItemData(formula.productItemId);
		if (itemData != null)
		{
			if (!GameUI.Instance.mSkillWndCtrl._SkillMgr.CheckUnlockProductItemLevel(itemData.level))
			{
				return false;
			}
			if (!GameUI.Instance.mSkillWndCtrl._SkillMgr.CheckUnlockProductItemType(itemData.itemClassId))
			{
				return false;
			}
			if (replicator.MaxProductCount(rootNode.ms.id) < rootNode.getCount / rootNode.ms.m_productItemCount)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	private GameObject CreateGraphItem(UIGraphNode parent, int lever_v)
	{
		if (mContent == null || mPrefabGraphItem == null)
		{
			return null;
		}
		GameObject gameObject = Object.Instantiate(mPrefabGraphItem);
		if (parent == null)
		{
			gameObject.transform.parent = mContent.transform;
			gameObject.transform.localPosition = new Vector3(0f, 0f, 0f);
		}
		else
		{
			gameObject.transform.parent = parent.mCtrl.child.transform;
			gameObject.transform.localPosition = new Vector3(0f, mGraphItemHeight, 0f);
		}
		gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
		if (parent != null)
		{
			UITreeGrid component = parent.mObject.GetComponent<UITreeGrid>();
			UITreeGrid component2 = gameObject.GetComponent<UITreeGrid>();
			if (component != null && component2 != null)
			{
				component.m_Children.Add(component2);
			}
		}
		return gameObject;
	}

	public void ClearGraph()
	{
		for (int i = 0; i < mGraphItemList.Count; i++)
		{
			Object.Destroy(mGraphItemList[i].mObject);
		}
		mGraphItemList.Clear();
		for (int j = 0; j < GraphLineList.Count; j++)
		{
			Object.Destroy(GraphLineList[j]);
		}
		GraphLineList.Clear();
		mContent.SetActive(value: false);
	}

	public UIGraphNode AddGraphItem(int lever_v, UIGraphNode partent, Replicator.Formula ms, Texture contentTexture)
	{
		UIGraphNode uIGraphNode = new UIGraphNode(lever_v, partent);
		GameObject gameObject = CreateGraphItem(partent, lever_v);
		int count = mGraphItemList.Count;
		uIGraphNode.mObject = gameObject;
		uIGraphNode.mIndex = count;
		uIGraphNode.mCtrl = gameObject.GetComponent<UIGraphItemCtrl>();
		uIGraphNode.mTreeGrid = gameObject.GetComponent<UITreeGrid>();
		uIGraphNode.ms = ms;
		if (partent == null)
		{
			rootNode = uIGraphNode;
		}
		if (uIGraphNode.mCtrl != null)
		{
			uIGraphNode.mCtrl.SetCotent(contentTexture);
			uIGraphNode.mCtrl.SetIndex(count);
		}
		mGraphItemList.Add(uIGraphNode);
		return uIGraphNode;
	}

	public UIGraphNode AddGraphItem(int lever_v, UIGraphNode partent, Replicator.Formula ms, string[] strSprites, string strAtlas)
	{
		UIGraphNode uIGraphNode = new UIGraphNode(lever_v, partent);
		GameObject gameObject = CreateGraphItem(partent, lever_v);
		int count = mGraphItemList.Count;
		uIGraphNode.mObject = gameObject;
		uIGraphNode.mIndex = count;
		uIGraphNode.mCtrl = gameObject.GetComponent<UIGraphItemCtrl>();
		uIGraphNode.mTreeGrid = gameObject.GetComponent<UITreeGrid>();
		uIGraphNode.mTipCtrl = uIGraphNode.mCtrl.mTipCtrl;
		uIGraphNode.ms = ms;
		if (partent == null)
		{
			rootNode = uIGraphNode;
		}
		if (uIGraphNode.mCtrl != null)
		{
			uIGraphNode.mCtrl.SetCotent(strSprites, strAtlas);
			uIGraphNode.mCtrl.SetIndex(count);
		}
		mGraphItemList.Add(uIGraphNode);
		return uIGraphNode;
	}

	private void Awake()
	{
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	private void FixedUpdate()
	{
		tempCount++;
		if (tempCount >= 50)
		{
			UpdateGraphNodeCount();
			tempCount = 0;
		}
	}

	private void UpdateGraphNodeCount()
	{
		Replicator replicator = GetReplicator();
		if (replicator != null)
		{
			for (int i = 0; i < mGraphItemList.Count; i++)
			{
				int itemCount = replicator.GetItemCount(mGraphItemList[i].GetItemID());
				mGraphItemList[i].bagCount = itemCount;
				mGraphItemList[i].mCtrl.SetCount(mGraphItemList[i].needCount, mGraphItemList[i].bagCount, mGraphItemList[i].getCount);
			}
		}
	}
}
