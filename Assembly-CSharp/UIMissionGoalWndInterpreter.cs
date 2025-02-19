using System;
using System.Collections.Generic;
using ItemAsset;
using Pathea;
using PeCustom;
using UnityEngine;

public class UIMissionGoalWndInterpreter : MonoBehaviour
{
	public UIMissionGoalWnd missionWnd;

	private List<int> m_MainStoryIds = new List<int>(5);

	private List<int> m_SideQuestIdes = new List<int>(5);

	private int _currentMissionId = -1;

	private bool _initialized;

	private int _deleteMissionId;

	private SortedList<int, MissionGoal> _currentGoals;

	private IList<MissionGoal> _goals;

	private List<int> _rewardItemIds;

	private List<int> _rewardItemCount;

	public void Init()
	{
		if (!_initialized)
		{
			UIMissionGoalWnd uIMissionGoalWnd = missionWnd;
			uIMissionGoalWnd.onSetNodeContent = (Action<int, UIMissionGoalNode>)Delegate.Combine(uIMissionGoalWnd.onSetNodeContent, new Action<int, UIMissionGoalNode>(OnSetNodeContent));
			UIMissionGoalWnd uIMissionGoalWnd2 = missionWnd;
			uIMissionGoalWnd2.onMissionNodeClick = (Action<int, UIMissionGoalNode>)Delegate.Combine(uIMissionGoalWnd2.onMissionNodeClick, new Action<int, UIMissionGoalNode>(OnMissionNodeClick));
			UIMissionGoalWnd uIMissionGoalWnd3 = missionWnd;
			uIMissionGoalWnd3.onSetGoalItemContent = (Action<UIMissionGoalItem>)Delegate.Combine(uIMissionGoalWnd3.onSetGoalItemContent, new Action<UIMissionGoalItem>(OnSetGoalItemContent));
			UIMissionGoalWnd uIMissionGoalWnd4 = missionWnd;
			uIMissionGoalWnd4.onSetRewardItemContent = (Action<Grid_N>)Delegate.Combine(uIMissionGoalWnd4.onSetRewardItemContent, new Action<Grid_N>(OnSetRewardItemContent));
			UIMissionGoalWnd uIMissionGoalWnd5 = missionWnd;
			uIMissionGoalWnd5.onMissionDeleteClick = (Action<UIMissionGoalNode>)Delegate.Combine(uIMissionGoalWnd5.onMissionDeleteClick, new Action<UIMissionGoalNode>(OnMissionDeleteClick));
			UIMissionGoalWnd uIMissionGoalWnd6 = missionWnd;
			uIMissionGoalWnd6.onTrackBoxSelected = (Action<bool, int, UIMissionGoalNode>)Delegate.Combine(uIMissionGoalWnd6.onTrackBoxSelected, new Action<bool, int, UIMissionGoalNode>(OnTrackBoxSelected));
			MissionMgr missionMgr = PeCustomScene.Self.scenario.missionMgr;
			missionMgr.onRunMission = (Action<int>)Delegate.Combine(missionMgr.onRunMission, new Action<int>(OnRunMission));
			MissionMgr missionMgr2 = PeCustomScene.Self.scenario.missionMgr;
			missionMgr2.onCloseMission = (Action<int, EMissionResult>)Delegate.Combine(missionMgr2.onCloseMission, new Action<int, EMissionResult>(OnCloseMission));
			MissionMgr missionMgr3 = PeCustomScene.Self.scenario.missionMgr;
			missionMgr3.onSetMissionGoal = (Action<int, int>)Delegate.Combine(missionMgr3.onSetMissionGoal, new Action<int, int>(OnSetMissionGoal));
			MissionMgr missionMgr4 = PeCustomScene.Self.scenario.missionMgr;
			missionMgr4.onMissionTrackChanged = (Action<int, bool>)Delegate.Combine(missionMgr4.onMissionTrackChanged, new Action<int, bool>(OnMissionTrackChanged));
			MissionMgr missionMgr5 = PeCustomScene.Self.scenario.missionMgr;
			missionMgr5.onResumeMission = (Action<int>)Delegate.Combine(missionMgr5.onResumeMission, new Action<int>(OnMissionResume));
			RefreshNodeSelected();
			_initialized = true;
		}
	}

	public void Close()
	{
		if (_initialized)
		{
			UIMissionGoalWnd uIMissionGoalWnd = missionWnd;
			uIMissionGoalWnd.onSetNodeContent = (Action<int, UIMissionGoalNode>)Delegate.Remove(uIMissionGoalWnd.onSetNodeContent, new Action<int, UIMissionGoalNode>(OnSetNodeContent));
			UIMissionGoalWnd uIMissionGoalWnd2 = missionWnd;
			uIMissionGoalWnd2.onMissionNodeClick = (Action<int, UIMissionGoalNode>)Delegate.Remove(uIMissionGoalWnd2.onMissionNodeClick, new Action<int, UIMissionGoalNode>(OnMissionNodeClick));
			UIMissionGoalWnd uIMissionGoalWnd3 = missionWnd;
			uIMissionGoalWnd3.onSetGoalItemContent = (Action<UIMissionGoalItem>)Delegate.Remove(uIMissionGoalWnd3.onSetGoalItemContent, new Action<UIMissionGoalItem>(OnSetGoalItemContent));
			UIMissionGoalWnd uIMissionGoalWnd4 = missionWnd;
			uIMissionGoalWnd4.onSetRewardItemContent = (Action<Grid_N>)Delegate.Remove(uIMissionGoalWnd4.onSetRewardItemContent, new Action<Grid_N>(OnSetRewardItemContent));
			UIMissionGoalWnd uIMissionGoalWnd5 = missionWnd;
			uIMissionGoalWnd5.onMissionDeleteClick = (Action<UIMissionGoalNode>)Delegate.Remove(uIMissionGoalWnd5.onMissionDeleteClick, new Action<UIMissionGoalNode>(OnMissionDeleteClick));
			UIMissionGoalWnd uIMissionGoalWnd6 = missionWnd;
			uIMissionGoalWnd6.onTrackBoxSelected = (Action<bool, int, UIMissionGoalNode>)Delegate.Remove(uIMissionGoalWnd6.onTrackBoxSelected, new Action<bool, int, UIMissionGoalNode>(OnTrackBoxSelected));
			MissionMgr missionMgr = PeCustomScene.Self.scenario.missionMgr;
			missionMgr.onRunMission = (Action<int>)Delegate.Remove(missionMgr.onRunMission, new Action<int>(OnRunMission));
			MissionMgr missionMgr2 = PeCustomScene.Self.scenario.missionMgr;
			missionMgr2.onCloseMission = (Action<int, EMissionResult>)Delegate.Remove(missionMgr2.onCloseMission, new Action<int, EMissionResult>(OnCloseMission));
			MissionMgr missionMgr3 = PeCustomScene.Self.scenario.missionMgr;
			missionMgr3.onSetMissionGoal = (Action<int, int>)Delegate.Remove(missionMgr3.onSetMissionGoal, new Action<int, int>(OnSetMissionGoal));
			MissionMgr missionMgr4 = PeCustomScene.Self.scenario.missionMgr;
			missionMgr4.onMissionTrackChanged = (Action<int, bool>)Delegate.Remove(missionMgr4.onMissionTrackChanged, new Action<int, bool>(OnMissionTrackChanged));
			MissionMgr missionMgr5 = PeCustomScene.Self.scenario.missionMgr;
			missionMgr5.onResumeMission = (Action<int>)Delegate.Remove(missionMgr5.onResumeMission, new Action<int>(OnMissionResume));
			_initialized = false;
		}
	}

	public void RefreshNodeSelected()
	{
		if (missionWnd.selectedNode == null)
		{
			if (missionWnd.IsShowAboutUI)
			{
				missionWnd.IsShowAboutUI = false;
			}
			if (missionWnd.IsShowGoalUI)
			{
				missionWnd.IsShowGoalUI = false;
			}
			if (missionWnd.IsShowRewardUI)
			{
				missionWnd.IsShowRewardUI = false;
			}
		}
		else
		{
			if (!missionWnd.IsShowAboutUI)
			{
				missionWnd.IsShowAboutUI = true;
			}
			if (!missionWnd.IsShowGoalUI)
			{
				missionWnd.IsShowGoalUI = true;
			}
			if (!missionWnd.IsShowRewardUI)
			{
				missionWnd.IsShowRewardUI = true;
			}
		}
	}

	private void OnRunMission(int id)
	{
		MissionProperty missionProperty = PeCustomScene.Self.scenario.missionMgr.GetMissionProperty(id);
		if (missionProperty != null)
		{
			if (missionProperty.type == MissionProperty.EType.MainStory)
			{
				m_MainStoryIds.Add(id);
				missionWnd.AddMainStoryNode();
			}
			else if (missionProperty.type == MissionProperty.EType.SideQuest)
			{
				m_SideQuestIdes.Add(id);
				missionWnd.AddSideQuestNode();
			}
		}
	}

	private void OnCloseMission(int id, EMissionResult result)
	{
		int num = m_MainStoryIds.FindIndex((int item0) => item0 == id);
		if (num != -1)
		{
			missionWnd.RemoveMainStoryNode(num);
			m_MainStoryIds.RemoveAt(num);
		}
		else
		{
			num = m_SideQuestIdes.FindIndex((int item0) => item0 == id);
			if (num != -1)
			{
				missionWnd.RemoveSideQuestNode(num);
				m_SideQuestIdes.RemoveAt(num);
			}
		}
		RefreshNodeSelected();
		if (missionWnd.selectedNode == null)
		{
			DetachGoalEvent();
			_currentMissionId = -1;
		}
	}

	private void OnMissionResume(int id)
	{
		MissionProperty missionProperty = PeCustomScene.Self.scenario.missionMgr.GetMissionProperty(id);
		if (missionProperty == null)
		{
			return;
		}
		if (missionProperty.type == MissionProperty.EType.MainStory)
		{
			if (!m_MainStoryIds.Contains(id))
			{
				m_MainStoryIds.Add(id);
				missionWnd.AddMainStoryNode();
			}
		}
		else if (missionProperty.type == MissionProperty.EType.SideQuest && !m_MainStoryIds.Contains(id))
		{
			m_SideQuestIdes.Add(id);
			missionWnd.AddSideQuestNode();
		}
	}

	private void OnMissionTrackChanged(int mission_id, bool is_tracked)
	{
		int num = m_MainStoryIds.FindIndex((int item0) => item0 == mission_id);
		if (num != -1)
		{
			UIMissionGoalNode uIMissionGoalNode = missionWnd.mainStoryNodes[num];
			uIMissionGoalNode.isTracked = is_tracked;
			return;
		}
		num = m_SideQuestIdes.FindIndex((int item0) => item0 == mission_id);
		if (num != -1)
		{
			UIMissionGoalNode uIMissionGoalNode2 = missionWnd.sidQuestNodes[num];
			uIMissionGoalNode2.isTracked = is_tracked;
		}
	}

	private void OnSetMissionGoal(int goal_id, int mission_id)
	{
		if (mission_id == _currentMissionId)
		{
			DetachGoalEvent();
			_currentGoals = PeCustomScene.Self.scenario.missionMgr.GetGoals(mission_id);
			if (_currentGoals != null)
			{
				_goals = _currentGoals.Values;
			}
			AttachGoalEvent();
			missionWnd.UpdateGoalItem(_currentGoals.Count);
		}
	}

	private void OnSetNodeContent(int type, UIMissionGoalNode node)
	{
		MissionProperty missionProperty = null;
		bool isTracked = false;
		switch (type)
		{
		case 0:
			missionProperty = PeCustomScene.Self.scenario.missionMgr.GetMissionProperty(m_MainStoryIds[node.index]);
			isTracked = PeCustomScene.Self.scenario.missionMgr.MissionIsTracked(m_MainStoryIds[node.index]);
			break;
		case 1:
			missionProperty = PeCustomScene.Self.scenario.missionMgr.GetMissionProperty(m_SideQuestIdes[node.index]);
			isTracked = PeCustomScene.Self.scenario.missionMgr.MissionIsTracked(m_SideQuestIdes[node.index]);
			break;
		}
		if (missionProperty != null)
		{
			node.SetContent(missionProperty.name, missionProperty.canAbort, _canTrack: true);
			node.IsSelected = false;
			node.isTracked = isTracked;
		}
	}

	private void OnMissionNodeClick(int type, UIMissionGoalNode node)
	{
		RefreshNodeSelected();
		DetachGoalEvent();
		MissionProperty missionProperty = null;
		switch (type)
		{
		case 0:
			missionProperty = PeCustomScene.Self.scenario.missionMgr.GetMissionProperty(m_MainStoryIds[node.index]);
			_currentGoals = PeCustomScene.Self.scenario.missionMgr.GetGoals(m_MainStoryIds[node.index]);
			if (_currentGoals != null)
			{
				_goals = _currentGoals.Values;
			}
			_currentMissionId = m_MainStoryIds[node.index];
			break;
		case 1:
			missionProperty = PeCustomScene.Self.scenario.missionMgr.GetMissionProperty(m_SideQuestIdes[node.index]);
			_currentGoals = PeCustomScene.Self.scenario.missionMgr.GetGoals(m_SideQuestIdes[node.index]);
			if (_currentGoals != null)
			{
				_goals = _currentGoals.Values;
			}
			_currentMissionId = m_SideQuestIdes[node.index];
			break;
		default:
			_currentMissionId = -1;
			_currentGoals = null;
			_goals = null;
			_rewardItemIds = null;
			_rewardItemCount = null;
			break;
		}
		if (missionProperty != null)
		{
			if (PeSingleton<CustomGameData.Mgr>.Instance != null && PeSingleton<CustomGameData.Mgr>.Instance.curGameData.WorldIndex == missionProperty.beginNpcWorldIndex)
			{
				SpawnPoint spawnPoint = PeCustomScene.Self.spawnData.GetSpawnPoint(missionProperty.beginNpcId);
				if (spawnPoint is NPCSpawnPoint)
				{
					NpcProtoDb.Item item = NpcProtoDb.Get(spawnPoint.Prototype);
					missionWnd.SetMissionAbout(spawnPoint.Name, item.iconBig, missionProperty.objective);
				}
				else if (spawnPoint is MonsterSpawnPoint)
				{
					MonsterProtoDb.Item item2 = MonsterProtoDb.Get(spawnPoint.Prototype);
					missionWnd.SetMissionAbout(spawnPoint.Name, item2.icon, missionProperty.objective);
				}
				else
				{
					missionWnd.SetMissionAbout("None", "npc_big_Unknown", missionProperty.objective);
				}
			}
			else
			{
				missionWnd.SetMissionAbout("None", "npc_big_Unknown", missionProperty.objective);
			}
			if (PeSingleton<CustomGameData.Mgr>.Instance != null && PeSingleton<CustomGameData.Mgr>.Instance.curGameData.WorldIndex == missionProperty.endNpcWorldIndex)
			{
				SpawnPoint spawnPoint2 = PeCustomScene.Self.spawnData.GetSpawnPoint(missionProperty.endNpcId);
				if (spawnPoint2 is NPCSpawnPoint)
				{
					NpcProtoDb.Item item3 = NpcProtoDb.Get(spawnPoint2.Prototype);
					missionWnd.SetRewardInfo(spawnPoint2.Name, item3.iconBig);
				}
				else if (spawnPoint2 is MonsterSpawnPoint)
				{
					MonsterProtoDb.Item item4 = MonsterProtoDb.Get(spawnPoint2.Prototype);
					missionWnd.SetRewardInfo(spawnPoint2.Name, item4.icon);
				}
				else
				{
					missionWnd.SetRewardInfo("None", "npc_big_Unknown");
					Debug.LogWarning("The Npc Id [" + missionProperty.endNpcId + "] spawn point is not exist");
				}
			}
			else
			{
				missionWnd.SetRewardInfo("None", "npc_big_Unknown");
			}
			if (missionProperty.rewardDesc != null)
			{
				missionWnd.SetRewardDesc(missionProperty.rewardDesc);
			}
			else
			{
				_rewardItemIds = missionProperty.rewardItemIds;
				_rewardItemCount = missionProperty.rewardItemCount;
				missionWnd.UpdateRewardItem(missionProperty.rewardItemIds.Count);
			}
		}
		if (_currentGoals != null)
		{
			AttachGoalEvent();
			missionWnd.UpdateGoalItem(_currentGoals.Count);
		}
	}

	private void OnSetGoalItemContent(UIMissionGoalItem item)
	{
		MissionGoal missionGoal = _goals[item.index];
		if (!missionGoal.achieved)
		{
			item.textColor = Color.white;
		}
		else
		{
			item.textColor = Color.green;
		}
		if (missionGoal is MissionGoal_Bool)
		{
			MissionGoal_Bool missionGoal_Bool = missionGoal as MissionGoal_Bool;
			item.SetBoolContent(missionGoal_Bool.text, missionGoal_Bool.achieved);
		}
		else if (missionGoal is MissionGoal_Item)
		{
			MissionGoal_Item missionGoal_Item = missionGoal as MissionGoal_Item;
			string text = missionGoal_Item.text + " " + missionGoal_Item.current + "/" + missionGoal_Item.target;
			item.value0 = missionGoal_Item.current;
			item.value1 = missionGoal_Item.target;
			if (missionGoal_Item.item.isSpecificPrototype)
			{
				string[] iconName = ItemProto.GetIconName(missionGoal_Item.item.Id);
				if (iconName != null)
				{
					item.SetItemContent(text, iconName[0]);
				}
				else
				{
					item.SetItemContent(text);
				}
			}
			else
			{
				item.SetItemContent(text);
			}
		}
		else
		{
			if (!(missionGoal is MissionGoal_Kill))
			{
				return;
			}
			MissionGoal_Kill missionGoal_Kill = missionGoal as MissionGoal_Kill;
			string text2 = missionGoal_Kill.text + " " + missionGoal_Kill.current + "/" + missionGoal_Kill.target;
			MissionGoal_Kill missionGoal_Kill2 = missionGoal as MissionGoal_Kill;
			item.value0 = missionGoal_Kill2.current;
			item.value1 = missionGoal_Kill2.target;
			if (item != null)
			{
				if (missionGoal_Kill2.monster.isSpecificEntity)
				{
					SpawnPoint spawnPoint = PeCustomScene.Self.spawnData.GetSpawnPoint(missionGoal_Kill2.id);
					if (spawnPoint is MonsterSpawnPoint)
					{
						MonsterProtoDb.Item item2 = MonsterProtoDb.Get(spawnPoint.ID);
						item.SetItemContent(text2, item2.icon);
					}
					else if (spawnPoint is NPCSpawnPoint)
					{
						NpcProtoDb.Item item3 = NpcProtoDb.Get(spawnPoint.ID);
						item.SetItemContent(text2, item3.icon);
					}
					else
					{
						item.SetItemContent(text2);
					}
				}
				else if (missionGoal_Kill2.monster.isSpecificPrototype)
				{
					MonsterProtoDb.Item item4 = MonsterProtoDb.Get(missionGoal_Kill2.monster.Id);
					if (item4 != null)
					{
						item.SetItemContent(text2, item4.icon);
					}
					else
					{
						item.SetItemContent(text2);
					}
				}
				else
				{
					item.SetItemContent(text2);
				}
			}
			else
			{
				item.SetItemContent(text2);
			}
		}
	}

	private void OnMissionGoalAchieved(int goal_id, int mission_id)
	{
		int index = _currentGoals.IndexOfKey(goal_id);
		UIMissionGoalItem goalItem = missionWnd.GetGoalItem(index);
		if (goalItem != null)
		{
			MissionGoal missionGoal = _currentGoals[goal_id];
			if (!missionGoal.achieved)
			{
				goalItem.textColor = Color.white;
			}
			else
			{
				goalItem.textColor = Color.green;
			}
		}
	}

	private void OnSetRewardItemContent(Grid_N grid)
	{
		int itemId = _rewardItemIds[grid.ItemIndex];
		int stackCount = _rewardItemCount[grid.ItemIndex];
		ItemSample itemGrid = new ItemSample(itemId, stackCount);
		grid.SetItem(itemGrid);
	}

	private void OnMissionDeleteClick(UIMissionGoalNode node)
	{
		_deleteMissionId = -1;
		if (missionWnd.MissionType == 0)
		{
			_deleteMissionId = m_MainStoryIds[node.index];
		}
		else if (missionWnd.MissionType == 1)
		{
			_deleteMissionId = m_SideQuestIdes[node.index];
		}
		if (_deleteMissionId == -1)
		{
			return;
		}
		MissionProperty missionProperty = PeCustomScene.Self.scenario.missionMgr.GetMissionProperty(_deleteMissionId);
		if (missionProperty != null)
		{
			if (missionProperty.canAbort)
			{
				MessageBox_N.ShowYNBox(PELocalization.GetString(8000066), DeleteMissionOk);
			}
			else
			{
				new PeTipMsg(PELocalization.GetString(8000850), PeTipMsg.EMsgLevel.Warning);
			}
		}
		else
		{
			Debug.LogError("Get the Deleted Mission property is error");
		}
	}

	private void DeleteMissionOk()
	{
		PeCustomScene.Self.scenario.missionMgr.CloseMission(_deleteMissionId, EMissionResult.Aborted);
	}

	private void OnTrackBoxSelected(bool active, int type, UIMissionGoalNode node)
	{
		int num = -1;
		switch (type)
		{
		case 0:
			num = m_MainStoryIds[node.index];
			break;
		case 1:
			num = m_SideQuestIdes[node.index];
			break;
		}
		if (num != -1)
		{
			PeCustomScene.Self.scenario.missionMgr.SetMissionIsTracked(num, active);
			Debug.Log("Do mission [" + num + "] track");
		}
	}

	private void UpdateGoal()
	{
	}

	private void Update()
	{
		if (!_initialized || !missionWnd.isShow || _currentGoals == null)
		{
			return;
		}
		for (int i = 0; i < _goals.Count; i++)
		{
			MissionGoal missionGoal = _goals[i];
			if (missionGoal is MissionGoal_Item)
			{
				MissionGoal_Item missionGoal_Item = missionGoal as MissionGoal_Item;
				UIMissionGoalItem goalItem = missionWnd.GetGoalItem(i);
				int current = missionGoal_Item.current;
				int target = missionGoal_Item.target;
				if (goalItem != null && (current != goalItem.value0 || target != goalItem.value1))
				{
					string itemText = missionGoal_Item.text + " " + missionGoal_Item.current + "/" + missionGoal_Item.target;
					goalItem.itemText = itemText;
					goalItem.value0 = current;
					goalItem.value1 = target;
				}
			}
			else if (missionGoal is MissionGoal_Kill)
			{
				MissionGoal_Kill missionGoal_Kill = missionGoal as MissionGoal_Kill;
				UIMissionGoalItem goalItem2 = missionWnd.GetGoalItem(i);
				int current2 = missionGoal_Kill.current;
				int target2 = missionGoal_Kill.target;
				if (goalItem2 != null && (current2 != goalItem2.value0 || target2 != goalItem2.value1))
				{
					string itemText2 = missionGoal_Kill.text + " " + missionGoal_Kill.current + "/" + missionGoal_Kill.target;
					goalItem2.itemText = itemText2;
					goalItem2.value0 = current2;
					goalItem2.value1 = target2;
				}
			}
		}
	}

	private void AttachGoalEvent()
	{
		if (_currentGoals != null)
		{
			for (int i = 0; i < _currentGoals.Count; i++)
			{
				MissionGoal missionGoal = _currentGoals.Values[i];
				missionGoal.onAchieve = (Action<int, int>)Delegate.Combine(missionGoal.onAchieve, new Action<int, int>(OnMissionGoalAchieved));
			}
		}
	}

	private void DetachGoalEvent()
	{
		if (_currentGoals != null)
		{
			for (int i = 0; i < _currentGoals.Count; i++)
			{
				MissionGoal missionGoal = _currentGoals.Values[i];
				missionGoal.onAchieve = (Action<int, int>)Delegate.Remove(missionGoal.onAchieve, new Action<int, int>(OnMissionGoalAchieved));
			}
		}
	}
}
