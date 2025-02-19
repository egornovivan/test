using System.Collections;
using System.Collections.Generic;
using Pathea;
using UnityEngine;

public class UIMissionTrackCtrl : UIBaseWnd
{
	private const string mainMissionColor = "[9b3df4]{0}[-]";

	private const string sideMissionColor = "[37cbf4]{0}[-]";

	private const string complateTitleFormat = "{0} [{1}]";

	private const string complateMissionColor = "[B4B4B4]{0} {1}[-]\n";

	private const string uncompletedMissionColor = "[FFFFFF]{0} {1}[-]\n";

	private const string countFormat = "{0}/{1}";

	public UIMissionTree mMissionTree;

	[SerializeField]
	private UIDraggablePanel m_DragPanel;

	[SerializeField]
	private Transform m_TutorialParent;

	[SerializeField]
	private GameObject m_TutorialPrefab;

	private Coroutine _coroutine;

	private void Awake()
	{
		UIMissionMgr.Instance.e_AddMission += OnAddMissionView;
		UIMissionMgr.Instance.e_DeleteMission += OnDelMissionView;
		UIMissionMgr.Instance.e_CheckTagMission += OnCheckTagMission;
		mMissionTree.mContentTable.sorted = true;
		ReGetAllMission();
	}

	private void OnEnable()
	{
		_coroutine = StartCoroutine(UpdateMissionTrack());
	}

	private void OnDisable()
	{
		if (_coroutine != null)
		{
			StopCoroutine(_coroutine);
		}
	}

	public override void Show()
	{
		base.Show();
		ResetContentPos();
	}

	private void ResetContentPos()
	{
		m_DragPanel.enabled = false;
		mMissionTree.RepositionContent();
		StartCoroutine(DelayActiveDragIterator());
	}

	private IEnumerator DelayActiveDragIterator()
	{
		yield return null;
		m_DragPanel.enabled = true;
	}

	private void ReGetAllMission()
	{
		foreach (KeyValuePair<int, UIMissionMgr.MissionView> item in UIMissionMgr.Instance.m_MissonMap)
		{
			OnAddMissionView(item.Value);
		}
	}

	private bool ContainsMissionView(UIMissionMgr.MissionView view)
	{
		bool result = false;
		foreach (UIMissionNode mNode in mMissionTree.mNodes)
		{
			if (mNode.mParent == null)
			{
				UIMissionMgr.MissionView missionView = mNode.mData as UIMissionMgr.MissionView;
				if (missionView.mMissionID == view.mMissionID)
				{
					return true;
				}
			}
		}
		return result;
	}

	private void OnAddMissionView(UIMissionMgr.MissionView view)
	{
		if (view.mMissionTag && !ContainsMissionView(view))
		{
			UIMissionNode uIMissionNode = mMissionTree.AddMissionNode(null, string.Empty, enableCkTag: false, enableBtnDel: false, canSelected: false);
			uIMissionNode.mData = view;
			uIMissionNode.mLbTitle.maxLineCount = 1;
			UIMissionNode uIMissionNode2 = mMissionTree.AddMissionNode(uIMissionNode, string.Empty, enableCkTag: false, enableBtnDel: false, canSelected: false);
			uIMissionNode2.mData = view.mTargetList;
			uIMissionNode2.mLbTitle.maxLineCount = 0;
			UpdateNodeText(uIMissionNode);
			UpdateNodeText(uIMissionNode2);
			Sort();
			uIMissionNode.ChangeExpand();
		}
	}

	private void Sort()
	{
		int num = 0;
		int count = mMissionTree.mNodes.Count;
		for (int i = 0; i < mMissionTree.mNodes.Count; i++)
		{
			UIMissionNode uIMissionNode = mMissionTree.mNodes[i];
			if (uIMissionNode.mTablePartent == mMissionTree.mContentTable && uIMissionNode.mData is UIMissionMgr.MissionView)
			{
				UIMissionMgr.MissionView missionView = uIMissionNode.mData as UIMissionMgr.MissionView;
				uIMissionNode.gameObject.name = ((missionView.mMissionType != MissionType.MissionType_Main) ? count++ : num++).ToString();
			}
		}
		mMissionTree.RepositionContent();
	}

	private void OnCheckTagMission(UIMissionMgr.MissionView view)
	{
		if (view.mMissionTag)
		{
			OnAddMissionView(view);
		}
		else
		{
			OnDelMissionView(view);
		}
	}

	private void OnDelMissionView(UIMissionMgr.MissionView view)
	{
		UIMissionNode uIMissionNode = FindMissionNode(view);
		if (uIMissionNode != null)
		{
			mMissionTree.DeleteMissionNode(uIMissionNode);
		}
	}

	private UIMissionNode FindMissionNode(UIMissionMgr.MissionView view)
	{
		foreach (UIMissionNode mNode in mMissionTree.mNodes)
		{
			if (mNode.mParent == null)
			{
				if (!(mNode.mData is UIMissionMgr.MissionView missionView))
				{
					Debug.LogError("missionView data eroor!");
				}
				else if (missionView.mMissionID == view.mMissionID)
				{
					return mNode;
				}
			}
		}
		return null;
	}

	private IEnumerator UpdateMissionTrack()
	{
		while (true)
		{
			foreach (UIMissionNode node in mMissionTree.mNodes)
			{
				UpdateNodeText(node);
			}
			yield return new WaitForSeconds(0.5f);
		}
	}

	private void UpdateNodeText(UIMissionNode node)
	{
		if (node.mData == null)
		{
			return;
		}
		if (node.mParent == null)
		{
			if (node.mData is UIMissionMgr.MissionView)
			{
				UIMissionMgr.MissionView missionView = node.mData as UIMissionMgr.MissionView;
				string format = ((missionView.mMissionType != MissionType.MissionType_Main) ? "[37cbf4]{0}[-]" : "[9b3df4]{0}[-]");
				string arg = ((!missionView.mComplete) ? missionView.mMissionTitle : $"{missionView.mMissionTitle} [{PELocalization.GetString(8000694)}]");
				node.mLbTitle.text = string.Format(format, arg);
			}
		}
		else
		{
			if (!(node.mData is List<UIMissionMgr.TargetShow>))
			{
				return;
			}
			List<UIMissionMgr.TargetShow> list = node.mData as List<UIMissionMgr.TargetShow>;
			string text = string.Empty;
			foreach (UIMissionMgr.TargetShow item in list)
			{
				string arg2 = string.Empty;
				if (item.mMaxCount > 0)
				{
					arg2 = $"{item.mCount}/{item.mMaxCount}";
				}
				text += string.Format((!item.mComplete) ? "[FFFFFF]{0} {1}[-]\n" : "[B4B4B4]{0} {1}[-]\n", item.mContent, arg2);
			}
			node.mLbTitle.text = text;
		}
	}

	public void ClearUI()
	{
		if (mMissionTree != null)
		{
			mMissionTree.Clear();
		}
	}

	public void EnableWndDrag(bool enable)
	{
		UIDragObject[] componentsInChildren = base.transform.GetComponentsInChildren<UIDragObject>();
		if (componentsInChildren != null && componentsInChildren.Length > 0)
		{
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = enable;
			}
		}
	}

	public void ShowWndTutorial()
	{
		if (PeGameMgr.IsTutorial)
		{
			GameObject gameObject = Object.Instantiate(m_TutorialPrefab);
			gameObject.transform.parent = m_TutorialParent;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localScale = Vector3.one;
			gameObject.transform.localRotation = Quaternion.identity;
		}
	}
}
