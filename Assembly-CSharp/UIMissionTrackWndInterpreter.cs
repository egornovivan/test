using System;
using System.Collections.Generic;
using PeCustom;
using UnityEngine;

public class UIMissionTrackWndInterpreter : MonoBehaviour
{
	public UIMissionTrackWnd missionTrackWnd;

	private List<int> m_TrackedIds = new List<int>(10);

	private bool _initialized;

	private IList<MissionGoal> _goals;

	public void Init()
	{
		if (!_initialized)
		{
			UIMissionTrackWnd uIMissionTrackWnd = missionTrackWnd;
			uIMissionTrackWnd.onSetViewNodeContent = (Action<UIMissionGoalNode>)Delegate.Combine(uIMissionTrackWnd.onSetViewNodeContent, new Action<UIMissionGoalNode>(OnViewNodeSetContent));
			UIMissionTrackWnd uIMissionTrackWnd2 = missionTrackWnd;
			uIMissionTrackWnd2.onDestroyViewNode = (Action<UIMissionGoalNode>)Delegate.Combine(uIMissionTrackWnd2.onDestroyViewNode, new Action<UIMissionGoalNode>(OnDestroyViewNode));
			MissionMgr missionMgr = PeCustomScene.Self.scenario.missionMgr;
			missionMgr.onRunMission = (Action<int>)Delegate.Combine(missionMgr.onRunMission, new Action<int>(OnMissionRun));
			MissionMgr missionMgr2 = PeCustomScene.Self.scenario.missionMgr;
			missionMgr2.onCloseMission = (Action<int, EMissionResult>)Delegate.Combine(missionMgr2.onCloseMission, new Action<int, EMissionResult>(OnCloseMission));
			MissionMgr missionMgr3 = PeCustomScene.Self.scenario.missionMgr;
			missionMgr3.onMissionTrackChanged = (Action<int, bool>)Delegate.Combine(missionMgr3.onMissionTrackChanged, new Action<int, bool>(OnMissionTrackChanged));
			MissionMgr missionMgr4 = PeCustomScene.Self.scenario.missionMgr;
			missionMgr4.onSetMissionGoal = (Action<int, int>)Delegate.Combine(missionMgr4.onSetMissionGoal, new Action<int, int>(OnSetMissionGoal));
			MissionMgr missionMgr5 = PeCustomScene.Self.scenario.missionMgr;
			missionMgr5.onUnsetMissionGoal = (Action<int, int>)Delegate.Combine(missionMgr5.onUnsetMissionGoal, new Action<int, int>(OnUnsetMissionGoal));
			PeCustomScene.Self.scenario.missionMgr.OnGoalAchieve += OnGoalAchieve;
			MissionMgr missionMgr6 = PeCustomScene.Self.scenario.missionMgr;
			missionMgr6.onResumeMission = (Action<int>)Delegate.Combine(missionMgr6.onResumeMission, new Action<int>(OnMissionResume));
			_initialized = true;
		}
	}

	public void Close()
	{
		if (_initialized)
		{
			UIMissionTrackWnd uIMissionTrackWnd = missionTrackWnd;
			uIMissionTrackWnd.onSetViewNodeContent = (Action<UIMissionGoalNode>)Delegate.Remove(uIMissionTrackWnd.onSetViewNodeContent, new Action<UIMissionGoalNode>(OnViewNodeSetContent));
			UIMissionTrackWnd uIMissionTrackWnd2 = missionTrackWnd;
			uIMissionTrackWnd2.onDestroyViewNode = (Action<UIMissionGoalNode>)Delegate.Remove(uIMissionTrackWnd2.onDestroyViewNode, new Action<UIMissionGoalNode>(OnDestroyViewNode));
			MissionMgr missionMgr = PeCustomScene.Self.scenario.missionMgr;
			missionMgr.onRunMission = (Action<int>)Delegate.Remove(missionMgr.onRunMission, new Action<int>(OnMissionRun));
			MissionMgr missionMgr2 = PeCustomScene.Self.scenario.missionMgr;
			missionMgr2.onCloseMission = (Action<int, EMissionResult>)Delegate.Remove(missionMgr2.onCloseMission, new Action<int, EMissionResult>(OnCloseMission));
			MissionMgr missionMgr3 = PeCustomScene.Self.scenario.missionMgr;
			missionMgr3.onMissionTrackChanged = (Action<int, bool>)Delegate.Remove(missionMgr3.onMissionTrackChanged, new Action<int, bool>(OnMissionTrackChanged));
			MissionMgr missionMgr4 = PeCustomScene.Self.scenario.missionMgr;
			missionMgr4.onSetMissionGoal = (Action<int, int>)Delegate.Remove(missionMgr4.onSetMissionGoal, new Action<int, int>(OnSetMissionGoal));
			MissionMgr missionMgr5 = PeCustomScene.Self.scenario.missionMgr;
			missionMgr5.onUnsetMissionGoal = (Action<int, int>)Delegate.Remove(missionMgr5.onUnsetMissionGoal, new Action<int, int>(OnUnsetMissionGoal));
			PeCustomScene.Self.scenario.missionMgr.OnGoalAchieve -= OnGoalAchieve;
			MissionMgr missionMgr6 = PeCustomScene.Self.scenario.missionMgr;
			missionMgr6.onResumeMission = (Action<int>)Delegate.Remove(missionMgr6.onResumeMission, new Action<int>(OnMissionResume));
			_initialized = false;
		}
	}

	private void OnViewNodeSetContent(UIMissionGoalNode node)
	{
		MissionProperty missionProperty = PeCustomScene.Self.scenario.missionMgr.GetMissionProperty(m_TrackedIds[node.index]);
		node.SetContent("[C8C800]" + missionProperty.name + "[-]", _canAbort: false, _canTrack: false);
		node.onSetChildNodeContent = (Action<int, GameObject>)Delegate.Combine(node.onSetChildNodeContent, new Action<int, GameObject>(OnSetViewNodeChildContent));
		SortedList<int, MissionGoal> goals = PeCustomScene.Self.scenario.missionMgr.GetGoals(m_TrackedIds[node.index]);
		if (goals != null)
		{
			_goals = goals.Values;
			node.UpdateChildNode(_goals.Count, missionTrackWnd.childNodePrefab);
			node.PlayTween(foward: true);
		}
		else
		{
			node.UpdateChildNode(0, missionTrackWnd.childNodePrefab);
		}
	}

	private void OnDestroyViewNode(UIMissionGoalNode node)
	{
		node.onSetChildNodeContent = (Action<int, GameObject>)Delegate.Remove(node.onSetChildNodeContent, new Action<int, GameObject>(OnSetViewNodeChildContent));
	}

	private void OnSetViewNodeChildContent(int index, GameObject child)
	{
		UIMissionGoalNode component = child.GetComponent<UIMissionGoalNode>();
		MissionGoal missionGoal = _goals[index];
		string title = string.Empty;
		if (missionGoal is MissionGoal_Bool)
		{
			MissionGoal_Bool missionGoal_Bool = missionGoal as MissionGoal_Bool;
			title = missionGoal_Bool.text;
			component.value2 = missionGoal.id;
		}
		else if (missionGoal is MissionGoal_Item)
		{
			MissionGoal_Item missionGoal_Item = missionGoal as MissionGoal_Item;
			title = missionGoal_Item.text + " " + missionGoal_Item.current + "/" + missionGoal_Item.target;
			component.value0 = missionGoal_Item.current;
			component.value1 = missionGoal_Item.target;
			component.value2 = missionGoal.id;
		}
		else if (missionGoal is MissionGoal_Kill)
		{
			MissionGoal_Kill missionGoal_Kill = missionGoal as MissionGoal_Kill;
			title = missionGoal_Kill.text + " " + missionGoal_Kill.current + "/" + missionGoal_Kill.target;
			component.value0 = missionGoal_Kill.current;
			component.value1 = missionGoal_Kill.target;
			component.value2 = missionGoal.id;
		}
		component.SetContent(title, _canAbort: false, _canTrack: false);
	}

	private void OnMissionRun(int mission_id)
	{
		MissionProperty missionProperty = PeCustomScene.Self.scenario.missionMgr.GetMissionProperty(mission_id);
		bool flag = PeCustomScene.Self.scenario.missionMgr.MissionIsTracked(mission_id);
		if (missionProperty != null && flag && missionProperty.type != 0)
		{
			m_TrackedIds.Add(mission_id);
			missionTrackWnd.AddViewNode();
		}
	}

	private void OnCloseMission(int mission_id, EMissionResult r)
	{
		int num = m_TrackedIds.FindIndex((int item0) => item0 == mission_id);
		if (num != -1)
		{
			missionTrackWnd.RemoveViewNode(num);
			m_TrackedIds.RemoveAt(num);
		}
	}

	private void OnMissionResume(int mission_id)
	{
		MissionProperty missionProperty = PeCustomScene.Self.scenario.missionMgr.GetMissionProperty(mission_id);
		bool flag = PeCustomScene.Self.scenario.missionMgr.MissionIsTracked(mission_id);
		if (missionProperty != null && flag && missionProperty.type != 0 && !m_TrackedIds.Contains(mission_id))
		{
			m_TrackedIds.Add(mission_id);
			missionTrackWnd.AddViewNode();
		}
	}

	private void OnMissionTrackChanged(int mission_id, bool is_tracked)
	{
		int num = m_TrackedIds.FindIndex((int item0) => item0 == mission_id);
		if (is_tracked)
		{
			if (num == -1)
			{
				m_TrackedIds.Add(mission_id);
				missionTrackWnd.AddViewNode();
			}
			else
			{
				Debug.LogWarning("The mission [" + mission_id + "] is already tracked.");
			}
		}
		else if (num != -1)
		{
			missionTrackWnd.RemoveViewNode(num);
			m_TrackedIds.RemoveAt(num);
		}
	}

	private void OnGoalAchieve(int goal_id, int mission_id)
	{
		int num = m_TrackedIds.FindIndex((int item0) => item0 == mission_id);
		if (num == -1)
		{
			return;
		}
		SortedList<int, MissionGoal> goals = PeCustomScene.Self.scenario.missionMgr.GetGoals(m_TrackedIds[num]);
		if (goals != null)
		{
			int index = goals.IndexOfKey(goal_id);
			UIMissionGoalNode component = missionTrackWnd.GetNode(num).childNode[index].GetComponent<UIMissionGoalNode>();
			if (component != null)
			{
				component.titleColor = new Color(0.65f, 0.65f, 0.65f);
			}
		}
	}

	private void OnSetMissionGoal(int goal_id, int mission_id)
	{
		int num = m_TrackedIds.FindIndex((int item0) => item0 == mission_id);
		if (num != -1)
		{
			UIMissionGoalNode node = missionTrackWnd.GetNode(num);
			SortedList<int, MissionGoal> goals = PeCustomScene.Self.scenario.missionMgr.GetGoals(m_TrackedIds[node.index]);
			if (goals != null)
			{
				_goals = goals.Values;
				node.UpdateChildNode(goals.Count, missionTrackWnd.childNodePrefab);
				node.PlayTween(foward: true);
			}
			else
			{
				node.UpdateChildNode(0, missionTrackWnd.childNodePrefab);
			}
			missionTrackWnd.repositionNow = true;
		}
	}

	private void OnUnsetMissionGoal(int goal_id, int mission_id)
	{
		int num = m_TrackedIds.FindIndex((int item0) => item0 == mission_id);
		if (num == -1)
		{
			return;
		}
		SortedList<int, MissionGoal> goals = PeCustomScene.Self.scenario.missionMgr.GetGoals(m_TrackedIds[num]);
		if (goals != null)
		{
			int num2 = goals.IndexOfKey(goal_id);
			if (num2 != -1)
			{
				UIMissionGoalNode node = missionTrackWnd.GetNode(num);
				node.RemoveChildeNode(num2);
			}
		}
	}

	private void Update()
	{
		if (!_initialized || !missionTrackWnd.isShow)
		{
			return;
		}
		for (int i = 0; i < missionTrackWnd.viewNodes.Count; i++)
		{
			UIMissionGoalNode uIMissionGoalNode = missionTrackWnd.viewNodes[i];
			int mission_id = m_TrackedIds[i];
			for (int j = 0; j < uIMissionGoalNode.childNode.Count; j++)
			{
				UIMissionGoalNode component = uIMissionGoalNode.childNode[j].GetComponent<UIMissionGoalNode>();
				MissionGoal goal = PeCustomScene.Self.scenario.missionMgr.GetGoal(component.value2, mission_id);
				if (goal == null)
				{
					continue;
				}
				if (goal is MissionGoal_Item)
				{
					MissionGoal_Item missionGoal_Item = goal as MissionGoal_Item;
					int current = missionGoal_Item.current;
					int target = missionGoal_Item.target;
					if (current != component.value0 || target != component.value1)
					{
						if (missionGoal_Item.achieved)
						{
							component.titleColor = new Color(0.65f, 0.65f, 0.65f);
						}
						else
						{
							component.titleColor = Color.white;
						}
						string title = missionGoal_Item.text + " " + missionGoal_Item.current + "/" + missionGoal_Item.target;
						component.SetContent(title, _canAbort: false, _canTrack: false);
						component.value0 = current;
						component.value1 = target;
					}
				}
				else
				{
					if (!(goal is MissionGoal_Kill))
					{
						continue;
					}
					MissionGoal_Kill missionGoal_Kill = goal as MissionGoal_Kill;
					int current2 = missionGoal_Kill.current;
					int target2 = missionGoal_Kill.target;
					if (current2 != component.value0 || target2 != component.value1)
					{
						if (missionGoal_Kill.achieved)
						{
							component.titleColor = new Color(0.75f, 0.75f, 0.75f);
						}
						else
						{
							component.titleColor = Color.white;
						}
						string title2 = missionGoal_Kill.text + " " + missionGoal_Kill.current + "/" + missionGoal_Kill.target;
						component.SetContent(title2, _canAbort: false, _canTrack: false);
						component.value0 = current2;
						component.value1 = target2;
					}
				}
			}
		}
	}
}
