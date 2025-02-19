using System;
using System.Collections.Generic;
using UnityEngine;

public class UIMissionGoalWnd : UIBaseWnd
{
	[SerializeField]
	private GameObject mainStroyRoot;

	[SerializeField]
	private UITable mainStoryTb;

	[SerializeField]
	private GameObject sideQuestRoot;

	[SerializeField]
	private UITable sideQuestTb;

	[SerializeField]
	private UIMissionGoalNode missionNodePrefab;

	[SerializeField]
	private GameObject aboutRoot;

	[SerializeField]
	private UILabel aboutNpcNameLb;

	[SerializeField]
	private UILabel aboutDescLb;

	[SerializeField]
	private UISprite aboutNpcIcon;

	[SerializeField]
	private GameObject goalRoot;

	[SerializeField]
	private UITable goalsTb;

	[SerializeField]
	private UIMissionGoalItem goalItemPrefab;

	[SerializeField]
	private GameObject rewardRoot;

	[SerializeField]
	private GameObject rewardNpcInfoRoot;

	[SerializeField]
	private UILabel rewardNpcNameLb;

	[SerializeField]
	private UISprite rewardNpcIcon;

	[SerializeField]
	private UIGrid rewardItemGrid;

	[SerializeField]
	private UILabel rewardDesc;

	[SerializeField]
	private Grid_N gridPrefab;

	private List<UIMissionGoalNode> m_MainStoryNodes = new List<UIMissionGoalNode>(10);

	private List<UIMissionGoalNode> m_SideQuestNodes = new List<UIMissionGoalNode>(10);

	private List<UIMissionGoalItem> m_MissionGoalItems = new List<UIMissionGoalItem>(10);

	private List<Grid_N> m_RewardGrids = new List<Grid_N>(10);

	public Action<int, UIMissionGoalNode> onSetNodeContent;

	public Action<int, UIMissionGoalNode> onMissionNodeClick;

	public Action<UIMissionGoalItem> onSetGoalItemContent;

	public Action<Grid_N> onSetRewardItemContent;

	public Action<UIMissionGoalNode> onMissionDeleteClick;

	public Action<bool, int, UIMissionGoalNode> onTrackBoxSelected;

	public UIMissionGoalNode selectedNode { get; private set; }

	public bool IsShowAboutUI
	{
		get
		{
			return aboutRoot.activeSelf;
		}
		set
		{
			aboutRoot.SetActive(value);
		}
	}

	public bool IsShowGoalUI
	{
		get
		{
			return goalRoot.activeSelf;
		}
		set
		{
			goalRoot.SetActive(value);
		}
	}

	public bool IsShowRewardUI
	{
		get
		{
			return rewardRoot.activeSelf;
		}
		set
		{
			rewardRoot.SetActive(value);
		}
	}

	public List<UIMissionGoalNode> mainStoryNodes => m_MainStoryNodes;

	public List<UIMissionGoalNode> sidQuestNodes => m_SideQuestNodes;

	public int MissionType
	{
		get
		{
			if (mainStroyRoot.activeSelf)
			{
				return 0;
			}
			if (sideQuestRoot.activeSelf)
			{
				return 1;
			}
			return -1;
		}
	}

	public void UpdateMainStoryNodes(int count)
	{
		UIUtility.UpdateListItems(m_MainStoryNodes, missionNodePrefab, mainStoryTb.transform, count, OnSetMainStoryNodeContent, OnDestroyMissionNode);
		mainStoryTb.repositionNow = true;
	}

	public void UpdateSideQuestNodes(int count)
	{
		UIUtility.UpdateListItems(m_SideQuestNodes, missionNodePrefab, sideQuestTb.transform, count, OnSetMainStoryNodeContent, OnDestroyMissionNode);
		sideQuestTb.repositionNow = true;
	}

	public void AddMainStoryNode()
	{
		UIMissionGoalNode uIMissionGoalNode = UIUtility.CreateItem(missionNodePrefab, mainStoryTb.transform);
		m_MainStoryNodes.Add(uIMissionGoalNode);
		OnSetMainStoryNodeContent(m_MainStoryNodes.Count - 1, uIMissionGoalNode);
		mainStoryTb.repositionNow = true;
	}

	public void AddSideQuestNode()
	{
		UIMissionGoalNode uIMissionGoalNode = UIUtility.CreateItem(missionNodePrefab, sideQuestTb.transform);
		m_SideQuestNodes.Add(uIMissionGoalNode);
		OnSetSideQuestNodeContent(m_SideQuestNodes.Count - 1, uIMissionGoalNode);
		sideQuestTb.repositionNow = true;
	}

	public void RemoveMainStoryNode(int index)
	{
		if (index >= 0 || index < m_MainStoryNodes.Count)
		{
			if (selectedNode == m_MainStoryNodes[index])
			{
				selectedNode = null;
			}
			OnDestroyMissionNode(m_MainStoryNodes[index]);
			UnityEngine.Object.Destroy(m_MainStoryNodes[index].gameObject);
			m_MainStoryNodes[index].transform.parent = null;
			m_MainStoryNodes.RemoveAt(index);
		}
	}

	public void RemoveSideQuestNode(int index)
	{
		if (index >= 0 || index < m_SideQuestNodes.Count)
		{
			if (selectedNode == m_SideQuestNodes[index])
			{
				selectedNode = null;
			}
			OnDestroyMissionNode(m_SideQuestNodes[index]);
			UnityEngine.Object.Destroy(m_SideQuestNodes[index].gameObject);
			m_SideQuestNodes[index].transform.parent = null;
			m_SideQuestNodes.RemoveAt(index);
		}
	}

	private void OnSetMainStoryNodeContent(int index, UIMissionGoalNode node)
	{
		node.index = index;
		node.onTitleClick += OnMissionTitileClick;
		node.onDeleteBtnClick += OnMissionDeleteBtnClick;
		node.onTrackBoxActive += OnMissionTrackBoxChecked;
		if (onSetNodeContent != null)
		{
			onSetNodeContent(0, node);
		}
	}

	private void OnSetSideQuestNodeContent(int index, UIMissionGoalNode node)
	{
		node.index = index;
		node.onTitleClick += OnMissionTitileClick;
		node.onDeleteBtnClick += OnMissionDeleteBtnClick;
		node.onTrackBoxActive += OnMissionTrackBoxChecked;
		if (onSetNodeContent != null)
		{
			onSetNodeContent(1, node);
		}
	}

	private void OnDestroyMissionNode(UIMissionGoalNode node)
	{
		node.onTitleClick -= OnMissionTitileClick;
		node.onDeleteBtnClick -= OnMissionDeleteBtnClick;
		node.onTrackBoxActive -= OnMissionTrackBoxChecked;
	}

	public void SetMissionAbout(string _name, string _icon, string _desc)
	{
		aboutNpcNameLb.text = _name;
		aboutNpcIcon.spriteName = _icon;
		aboutDescLb.text = _desc;
	}

	public UIMissionGoalItem GetGoalItem(int index)
	{
		if (index < 0 && index >= m_MissionGoalItems.Count)
		{
			return null;
		}
		return m_MissionGoalItems[index];
	}

	public void UpdateGoalItem(int count)
	{
		UIUtility.UpdateListItems(m_MissionGoalItems, goalItemPrefab, goalsTb.transform, count, OnSetGoalItemContent, null);
		goalsTb.repositionNow = true;
	}

	private void OnSetGoalItemContent(int index, UIMissionGoalItem item)
	{
		m_MissionGoalItems[index].index = index;
		if (onSetGoalItemContent != null)
		{
			onSetGoalItemContent(m_MissionGoalItems[index]);
		}
	}

	public void SetRewardInfo(string _npcName, string _npcIcon)
	{
		if (!rewardNpcInfoRoot.activeSelf)
		{
			rewardNpcInfoRoot.SetActive(value: true);
		}
		rewardNpcNameLb.text = _npcName;
		rewardNpcIcon.spriteName = _npcIcon;
	}

	public void HideRewardInfo()
	{
		if (rewardNpcInfoRoot.activeSelf)
		{
			rewardNpcInfoRoot.SetActive(value: false);
		}
	}

	public void SetRewardDesc(string _desc)
	{
		if (!rewardDesc.gameObject.activeSelf)
		{
			rewardDesc.gameObject.SetActive(value: true);
		}
		rewardDesc.text = _desc;
		if (rewardItemGrid.gameObject.activeSelf)
		{
			rewardItemGrid.gameObject.SetActive(value: false);
		}
	}

	public void UpdateRewardItem(int count)
	{
		if (!rewardItemGrid.gameObject.activeSelf)
		{
			rewardItemGrid.gameObject.SetActive(value: true);
		}
		UIUtility.UpdateListItems(m_RewardGrids, gridPrefab, rewardItemGrid.transform, count, OnSetRewardItemContent, null);
		rewardItemGrid.repositionNow = true;
		if (rewardDesc.gameObject.activeSelf)
		{
			rewardDesc.gameObject.SetActive(value: false);
		}
	}

	private void OnSetRewardItemContent(int index, Grid_N grid)
	{
		grid.ItemIndex = index;
		if (onSetRewardItemContent != null)
		{
			onSetRewardItemContent(grid);
		}
	}

	private void OnMissionTitileClick(UIMissionGoalNode node)
	{
		int arg = -1;
		Transform transform = node.transform;
		if (transform.parent == mainStoryTb.transform)
		{
			arg = 0;
			node.IsSelected = true;
			selectedNode = node;
			for (int i = 0; i < m_MainStoryNodes.Count; i++)
			{
				if (m_MainStoryNodes[i] != node)
				{
					m_MainStoryNodes[i].IsSelected = false;
				}
			}
		}
		else
		{
			Transform transform3 = (transform.parent = sideQuestRoot.transform);
			if ((bool)transform3)
			{
				arg = 1;
				node.IsSelected = true;
				selectedNode = node;
				for (int j = 0; j < m_SideQuestNodes.Count; j++)
				{
					if (m_SideQuestNodes[j] != node)
					{
						m_SideQuestNodes[j].IsSelected = false;
					}
				}
			}
		}
		if (onMissionNodeClick != null)
		{
			onMissionNodeClick(arg, node);
		}
	}

	private void OnMissionDeleteBtnClick(UIMissionGoalNode node)
	{
		if (onMissionDeleteClick != null)
		{
			onMissionDeleteClick(node);
		}
	}

	private void OnMissionTrackBoxChecked(bool active, UIMissionGoalNode node)
	{
		int num = 0;
		num = ((m_MainStoryNodes.Count <= node.index || !(m_MainStoryNodes[node.index] == node)) ? ((m_SideQuestNodes.Count > node.index && m_SideQuestNodes[node.index] == node) ? 1 : (-1)) : 0);
		if (num != -1 && onTrackBoxSelected != null)
		{
			onTrackBoxSelected(active, num, node);
		}
	}

	private void OnMainStoryMenuClick()
	{
		if (!mainStroyRoot.activeSelf)
		{
			mainStroyRoot.SetActive(value: true);
		}
		if (sideQuestRoot.activeSelf)
		{
			sideQuestRoot.SetActive(value: false);
		}
	}

	private void OnSideQuestMenuClick()
	{
		if (!sideQuestRoot.activeSelf)
		{
			sideQuestRoot.SetActive(value: true);
		}
		if (mainStroyRoot.activeSelf)
		{
			mainStroyRoot.SetActive(value: false);
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
