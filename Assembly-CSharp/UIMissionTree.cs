using System.Collections.Generic;
using UnityEngine;

public class UIMissionTree : UIComponent
{
	public delegate void BaseMsgEvent(object sender);

	public GameObject mNodePrefab;

	[HideInInspector]
	public List<UIMissionNode> mNodes = new List<UIMissionNode>();

	[HideInInspector]
	public UIMissionNode mSelectedNode;

	public UITable mContentTable;

	public int UITreeHight = 30;

	[SerializeField]
	private UIScrollBar mScroBar;

	public event BaseMsgEvent e_ChangeSelectedNode;

	private void Start()
	{
	}

	public void Clear()
	{
		for (int num = mNodes.Count - 1; num >= 0; num--)
		{
			DeleteMissionNode(mNodes[num]);
		}
		mNodes.Clear();
	}

	public void DeleteMissionNode(UIMissionNode node)
	{
		if (node == null)
		{
			return;
		}
		for (int num = mNodes.Count - 1; num >= 0; num--)
		{
			if (num < mNodes.Count && mNodes[num].mParent == node)
			{
				mNodes.Remove(mNodes[num]);
			}
		}
		mNodes.Remove(node);
		Object.Destroy(node.gameObject);
		node.gameObject.transform.parent = null;
		node = null;
		RepositionContent();
	}

	public void RepositionContent()
	{
		mContentTable.repositionNow = true;
		mScroBar.scrollValue = 0f;
	}

	public List<UIMissionNode> GetChildNode(UIMissionNode parentNode)
	{
		List<UIMissionNode> list = new List<UIMissionNode>();
		for (int i = 0; i < mNodes.Count; i++)
		{
			if (parentNode == mNodes[i].mParent)
			{
				list.Add(mNodes[i]);
			}
		}
		return list;
	}

	public UIMissionNode AddMissionNode(UIMissionNode parentNode, string text, bool enableCkTag = true, bool enableBtnDel = true, bool canSelected = true)
	{
		GameObject gameObject = Object.Instantiate(mNodePrefab);
		UIMissionNode component = gameObject.GetComponent<UIMissionNode>();
		if (parentNode == null)
		{
			component.transform.parent = mContentTable.gameObject.transform;
			component.mTablePartent = mContentTable;
		}
		else
		{
			component.transform.parent = parentNode.mTable.gameObject.transform;
			component.mTablePartent = parentNode.mTable;
		}
		component.gameObject.transform.localScale = Vector3.one;
		component.mLbTitle.text = text;
		component.enableCkTag = enableCkTag;
		component.enableBtnDelete = enableBtnDel;
		component.e_OnClick += OnChangeSelectedNode;
		component.gameObject.SetActive(value: true);
		component.mCanSelected = canSelected;
		component.mParent = parentNode;
		component.transform.localPosition = new Vector3(0f, -(FindChildNodeCount(parentNode) * UITreeHight), 0f);
		if ((bool)parentNode)
		{
			parentNode.mChilds.Add(component);
		}
		mNodes.Add(component);
		return component;
	}

	private int FindChildNodeCount(UIMissionNode parentNode)
	{
		int num = 0;
		for (int i = 0; i < mNodes.Count; i++)
		{
			if (mNodes[i].mParent == parentNode)
			{
				num++;
			}
		}
		return num;
	}

	private void ResetSelectBg(UIMissionNode node)
	{
		if (!(node.mParent == null))
		{
			UIMissionNode mParent = node.mParent;
			int num = 0;
			while (mParent != null)
			{
				num++;
				mParent = mParent.mParent;
			}
			Vector3 localScale = node.mSpSelected.transform.localScale;
			node.mSpSelected.transform.localScale = new Vector3(localScale.x - 30f, localScale.y, localScale.z);
		}
	}

	private void OnChangeSelectedNode(object sender)
	{
		UIMissionNode uIMissionNode = sender as UIMissionNode;
		if (!(uIMissionNode == null) && !(uIMissionNode == mSelectedNode))
		{
			if (mSelectedNode != null)
			{
				mSelectedNode.Selected = false;
			}
			uIMissionNode.Selected = true;
			mSelectedNode = uIMissionNode;
			if (this.e_ChangeSelectedNode != null)
			{
				this.e_ChangeSelectedNode(mSelectedNode);
			}
		}
	}
}
