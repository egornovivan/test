using System;
using System.Collections.Generic;
using UnityEngine;

public class UIMissionTrackWnd : UIBaseWnd
{
	[SerializeField]
	private UITable viewTable;

	[SerializeField]
	private UIMissionGoalNode viewNodePrefab;

	public GameObject childNodePrefab;

	private List<UIMissionGoalNode> m_ViewNodes = new List<UIMissionGoalNode>(10);

	public Action<UIMissionGoalNode> onSetViewNodeContent;

	public Action<UIMissionGoalNode> onDestroyViewNode;

	public bool repositionNow
	{
		get
		{
			return viewTable.repositionNow;
		}
		set
		{
			viewTable.repositionNow = true;
		}
	}

	public List<UIMissionGoalNode> viewNodes => m_ViewNodes;

	public UIMissionGoalNode GetNode(int index)
	{
		return m_ViewNodes[index];
	}

	public void UpdateViewNode(int count)
	{
		UIUtility.UpdateListItems(m_ViewNodes, viewNodePrefab, viewTable.transform, count, OnSetViewNodeContent, OnDestroyViewNode);
		viewTable.repositionNow = true;
	}

	public void AddViewNode()
	{
		UIMissionGoalNode uIMissionGoalNode = UIUtility.CreateItem(viewNodePrefab, viewTable.transform);
		m_ViewNodes.Add(uIMissionGoalNode);
		OnSetViewNodeContent(m_ViewNodes.Count - 1, uIMissionGoalNode);
		viewTable.repositionNow = true;
	}

	public void RemoveViewNode(int index)
	{
		if (index >= 0 || index < m_ViewNodes.Count)
		{
			UIMissionGoalNode uIMissionGoalNode = m_ViewNodes[index];
			OnDestroyViewNode(uIMissionGoalNode);
			UnityEngine.Object.Destroy(uIMissionGoalNode.gameObject);
			uIMissionGoalNode.transform.parent = null;
			m_ViewNodes.RemoveAt(index);
		}
	}

	private void OnSetViewNodeContent(int index, UIMissionGoalNode node)
	{
		node.index = index;
		if (onSetViewNodeContent != null)
		{
			onSetViewNodeContent(node);
		}
	}

	private void OnDestroyViewNode(UIMissionGoalNode node)
	{
		if (onDestroyViewNode != null)
		{
			onDestroyViewNode(node);
		}
	}

	public override void Show()
	{
		base.Show();
	}

	protected override void OnHide()
	{
		base.OnHide();
	}

	protected override void OnClose()
	{
		base.OnClose();
	}
}
