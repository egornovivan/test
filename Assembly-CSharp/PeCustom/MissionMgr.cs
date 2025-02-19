using System;
using System.Collections.Generic;
using System.IO;
using ScenarioRTL;
using UnityEngine;

namespace PeCustom;

public class MissionMgr
{
	public Action<int> onRunMission;

	public Action<int, EMissionResult> onCloseMission;

	public Action<int> onResumeMission;

	public Action<int, bool> onMissionTrackChanged;

	private Scenario m_Scenario;

	private List<int> m_MissionIDs;

	private Dictionary<int, MissionProperty> m_MissionProperties;

	private Dictionary<int, bool> m_MissionTrack;

	private Dictionary<int, SortedList<int, MissionGoal>> m_Goals;

	public Action<int, int> onSetMissionGoal;

	public Action<int, int> onUnsetMissionGoal;

	public event Action<int, int> OnGoalAchieve;

	public MissionMgr(Scenario scenario)
	{
		m_Scenario = scenario;
		m_MissionIDs = new List<int>(10);
		m_MissionProperties = new Dictionary<int, MissionProperty>(10);
		m_MissionTrack = new Dictionary<int, bool>(10);
		InitGoals();
	}

	public bool RunMission(int id)
	{
		if (m_Scenario.RunMission(id))
		{
			m_MissionIDs.Add(id);
			MissionProperty missionProperty = new MissionProperty();
			missionProperty.Parse(m_Scenario.GetMissionProperties(id), m_Scenario.GetMissionName(id));
			m_MissionProperties.Add(id, missionProperty);
			if (!m_MissionTrack.ContainsKey(id))
			{
				m_MissionTrack.Add(id, value: true);
			}
			if (onRunMission != null)
			{
				onRunMission(id);
			}
			return true;
		}
		Debug.LogError("Run mission [" + id + "] failed !");
		return false;
	}

	public bool CloseMission(int id, EMissionResult result)
	{
		if (m_Scenario.IsMissionRunning(id))
		{
			if (onCloseMission != null)
			{
				onCloseMission(id, result);
			}
			if (m_Goals != null)
			{
				FreeGoals(id);
				m_Goals.Remove(id);
			}
			if (m_Scenario.CloseMission(id))
			{
				m_MissionIDs.Remove(id);
				m_MissionProperties.Remove(id);
				m_MissionTrack.Remove(id);
				return true;
			}
		}
		else
		{
			Debug.LogError("Close mission [" + id + "] failed !");
		}
		return false;
	}

	public void ResumeMission()
	{
		m_Scenario.Resume();
		int[] runningMissionIds = m_Scenario.runningMissionIds;
		for (int i = 0; i < runningMissionIds.Length; i++)
		{
			m_MissionIDs.Add(runningMissionIds[i]);
			int num = m_MissionIDs[i];
			if (!m_MissionProperties.ContainsKey(num))
			{
				MissionProperty missionProperty = new MissionProperty();
				missionProperty.Parse(m_Scenario.GetMissionProperties(num), m_Scenario.GetMissionName(num));
				m_MissionProperties.Add(num, missionProperty);
			}
			if (!m_MissionTrack.ContainsKey(num))
			{
				m_MissionTrack.Add(num, value: true);
			}
			if (onResumeMission != null)
			{
				onResumeMission(m_MissionIDs[i]);
			}
		}
	}

	public MissionProperty GetMissionProperty(int id)
	{
		if (m_MissionProperties.ContainsKey(id))
		{
			return m_MissionProperties[id];
		}
		return null;
	}

	public bool MissionIsTracked(int mission_id)
	{
		if (m_MissionTrack.ContainsKey(mission_id))
		{
			return m_MissionTrack[mission_id];
		}
		return false;
	}

	public void SetMissionIsTracked(int mission_id, bool isTracked)
	{
		if (m_MissionTrack.ContainsKey(mission_id) && m_MissionTrack[mission_id] != isTracked)
		{
			m_MissionTrack[mission_id] = isTracked;
			if (onMissionTrackChanged != null)
			{
				onMissionTrackChanged(mission_id, isTracked);
			}
		}
	}

	public void Free()
	{
		FreeGoals();
	}

	private void InitGoals()
	{
		m_Goals = new Dictionary<int, SortedList<int, MissionGoal>>(10);
	}

	private void FreeGoals()
	{
		if (m_Goals == null)
		{
			return;
		}
		foreach (KeyValuePair<int, SortedList<int, MissionGoal>> goal in m_Goals)
		{
			FreeGoals(goal.Key);
		}
		m_Goals.Clear();
	}

	private void FreeGoals(int id)
	{
		if (m_Goals == null || !m_Goals.ContainsKey(id))
		{
			return;
		}
		SortedList<int, MissionGoal> sortedList = m_Goals[id];
		foreach (KeyValuePair<int, MissionGoal> item in sortedList)
		{
			item.Value.Free();
		}
		sortedList.Clear();
	}

	public SortedList<int, MissionGoal> GetGoals(int mission_id)
	{
		if (m_Goals.ContainsKey(mission_id))
		{
			return m_Goals[mission_id];
		}
		return null;
	}

	public MissionGoal GetGoal(int goal_id, int mission_id)
	{
		if (m_Goals.ContainsKey(mission_id) && m_Goals[mission_id].ContainsKey(goal_id))
		{
			return m_Goals[mission_id][goal_id];
		}
		return null;
	}

	public void UpdateGoals()
	{
		foreach (KeyValuePair<int, SortedList<int, MissionGoal>> goal in m_Goals)
		{
			SortedList<int, MissionGoal> sortedList = m_Goals[goal.Key];
			for (int i = 0; i < sortedList.Count; i++)
			{
				sortedList.Values[i].Update();
			}
		}
	}

	public void SetBoolGoal(int id, string text, int missionId, bool achieved)
	{
		if (m_Scenario.IsMissionActive(missionId))
		{
			if (!m_Goals.ContainsKey(missionId))
			{
				m_Goals[missionId] = new SortedList<int, MissionGoal>(4);
			}
			SortedList<int, MissionGoal> sortedList = m_Goals[missionId];
			if (!sortedList.ContainsKey(id))
			{
				sortedList[id] = new MissionGoal_Bool();
				sortedList[id].onAchieve = onGoalAchieve;
				sortedList[id].Init();
			}
			if (!(sortedList[id] is MissionGoal_Bool))
			{
				sortedList[id].Free();
				sortedList[id] = new MissionGoal_Bool();
				sortedList[id].onAchieve = onGoalAchieve;
				sortedList[id].Init();
			}
			MissionGoal_Bool missionGoal_Bool = sortedList[id] as MissionGoal_Bool;
			missionGoal_Bool.id = id;
			missionGoal_Bool.text = text;
			missionGoal_Bool.missionId = missionId;
			missionGoal_Bool.achieved = achieved;
			if (onSetMissionGoal != null)
			{
				onSetMissionGoal(id, missionId);
			}
		}
	}

	public void SetItemGoal(int id, string text, int missionId, OBJECT item, ECompare compare, int amount)
	{
		if (m_Scenario.IsMissionActive(missionId))
		{
			if (!m_Goals.ContainsKey(missionId))
			{
				m_Goals[missionId] = new SortedList<int, MissionGoal>(4);
			}
			SortedList<int, MissionGoal> sortedList = m_Goals[missionId];
			if (!sortedList.ContainsKey(id))
			{
				sortedList[id] = new MissionGoal_Item();
				sortedList[id].onAchieve = onGoalAchieve;
				sortedList[id].Init();
			}
			if (!(sortedList[id] is MissionGoal_Item))
			{
				sortedList[id].Free();
				sortedList[id] = new MissionGoal_Item();
				sortedList[id].onAchieve = onGoalAchieve;
				sortedList[id].Init();
			}
			MissionGoal_Item missionGoal_Item = sortedList[id] as MissionGoal_Item;
			missionGoal_Item.id = id;
			missionGoal_Item.text = text;
			missionGoal_Item.missionId = missionId;
			missionGoal_Item.item = item;
			missionGoal_Item.compare = compare;
			missionGoal_Item.target = amount;
			if (onSetMissionGoal != null)
			{
				onSetMissionGoal(id, missionId);
			}
		}
	}

	public void SetKillGoal(int id, string text, int missionId, OBJECT monster, ECompare compare, int amount)
	{
		if (m_Scenario.IsMissionActive(missionId))
		{
			if (!m_Goals.ContainsKey(missionId))
			{
				m_Goals[missionId] = new SortedList<int, MissionGoal>(4);
			}
			SortedList<int, MissionGoal> sortedList = m_Goals[missionId];
			if (!sortedList.ContainsKey(id))
			{
				sortedList[id] = new MissionGoal_Kill();
				sortedList[id].onAchieve = onGoalAchieve;
				sortedList[id].Init();
			}
			if (!(sortedList[id] is MissionGoal_Kill))
			{
				sortedList[id].Free();
				sortedList[id] = new MissionGoal_Kill();
				sortedList[id].onAchieve = onGoalAchieve;
				sortedList[id].Init();
			}
			MissionGoal_Kill missionGoal_Kill = sortedList[id] as MissionGoal_Kill;
			missionGoal_Kill.id = id;
			missionGoal_Kill.text = text;
			missionGoal_Kill.missionId = missionId;
			missionGoal_Kill.monster = monster;
			missionGoal_Kill.compare = compare;
			missionGoal_Kill.target = amount;
			if (onSetMissionGoal != null)
			{
				onSetMissionGoal(id, missionId);
			}
		}
	}

	public void UnsetGoal(int id, int missionId)
	{
		if (m_Goals.ContainsKey(missionId))
		{
			SortedList<int, MissionGoal> sortedList = m_Goals[missionId];
			if (sortedList.ContainsKey(id))
			{
				sortedList[id].Free();
				sortedList.Remove(id);
			}
			if (sortedList.Count == 0)
			{
				m_Goals.Remove(missionId);
			}
			if (onUnsetMissionGoal != null)
			{
				onUnsetMissionGoal(id, missionId);
			}
		}
	}

	public bool? GoalAchieved(int id, int missionId)
	{
		if (m_Goals.ContainsKey(missionId))
		{
			SortedList<int, MissionGoal> sortedList = m_Goals[missionId];
			if (sortedList.ContainsKey(id))
			{
				return sortedList[id].achieved;
			}
		}
		return null;
	}

	private void onGoalAchieve(int id, int missionId)
	{
		if (this.OnGoalAchieve != null)
		{
			this.OnGoalAchieve(id, missionId);
		}
	}

	public void Import(BinaryReader r)
	{
		r.ReadInt32();
		ImportGoals(r);
		ImportMisc(r);
	}

	public void Export(BinaryWriter w)
	{
		w.Write(0);
		ExportGoals(w);
		ExportMisc(w);
	}

	private void ImportGoals(BinaryReader r)
	{
		r.ReadInt32();
		FreeGoals();
		InitGoals();
		int num = 0;
		int num2;
		OBJECT item = default(OBJECT);
		OBJECT monster = default(OBJECT);
		do
		{
			num2 = r.ReadInt32();
			switch (num2)
			{
			case 1:
			{
				int id2 = r.ReadInt32();
				int missionId2 = r.ReadInt32();
				string text3 = r.ReadString();
				bool achieved = r.ReadBoolean();
				SetBoolGoal(id2, text3, missionId2, achieved);
				break;
			}
			case 2:
			{
				int id = r.ReadInt32();
				int missionId = r.ReadInt32();
				string text2 = r.ReadString();
				item.type = (OBJECT.OBJECTTYPE)r.ReadInt32();
				item.Group = r.ReadInt32();
				item.Id = r.ReadInt32();
				ECompare compare2 = (ECompare)r.ReadInt32();
				int amount2 = r.ReadInt32();
				SetItemGoal(id, text2, missionId, item, compare2, amount2);
				break;
			}
			case 3:
			{
				int num3 = r.ReadInt32();
				int num4 = r.ReadInt32();
				string text = r.ReadString();
				monster.type = (OBJECT.OBJECTTYPE)r.ReadInt32();
				monster.Group = r.ReadInt32();
				monster.Id = r.ReadInt32();
				ECompare compare = (ECompare)r.ReadInt32();
				int amount = r.ReadInt32();
				SetKillGoal(num3, text, num4, monster, compare, amount);
				int current = r.ReadInt32();
				if (m_Goals.ContainsKey(num4) && m_Goals[num4].ContainsKey(num3))
				{
					MissionGoal missionGoal = m_Goals[num4][num3];
					if (missionGoal != null && missionGoal is MissionGoal_Kill)
					{
						(missionGoal as MissionGoal_Kill).current = current;
					}
				}
				break;
			}
			}
		}
		while (num2 != -1 && ++num <= 1024);
	}

	private void ExportGoals(BinaryWriter w)
	{
		w.Write(0);
		foreach (KeyValuePair<int, SortedList<int, MissionGoal>> goal in m_Goals)
		{
			SortedList<int, MissionGoal> value = goal.Value;
			for (int i = 0; i < value.Count; i++)
			{
				MissionGoal missionGoal = value.Values[i];
				if (missionGoal is MissionGoal_Bool)
				{
					MissionGoal_Bool missionGoal_Bool = missionGoal as MissionGoal_Bool;
					w.Write(1);
					w.Write(missionGoal_Bool.id);
					w.Write(missionGoal_Bool.missionId);
					w.Write(missionGoal_Bool.text);
					w.Write(missionGoal_Bool.achieved);
				}
				else if (missionGoal is MissionGoal_Item)
				{
					MissionGoal_Item missionGoal_Item = missionGoal as MissionGoal_Item;
					w.Write(2);
					w.Write(missionGoal_Item.id);
					w.Write(missionGoal_Item.missionId);
					w.Write(missionGoal_Item.text);
					w.Write((int)missionGoal_Item.item.type);
					w.Write(missionGoal_Item.item.Group);
					w.Write(missionGoal_Item.item.Id);
					w.Write((int)missionGoal_Item.compare);
					w.Write(missionGoal_Item.target);
				}
				else if (missionGoal is MissionGoal_Kill)
				{
					MissionGoal_Kill missionGoal_Kill = missionGoal as MissionGoal_Kill;
					w.Write(3);
					w.Write(missionGoal_Kill.id);
					w.Write(missionGoal_Kill.missionId);
					w.Write(missionGoal_Kill.text);
					w.Write((int)missionGoal_Kill.monster.type);
					w.Write(missionGoal_Kill.monster.Group);
					w.Write(missionGoal_Kill.monster.Id);
					w.Write((int)missionGoal_Kill.compare);
					w.Write(missionGoal_Kill.target);
					w.Write(missionGoal_Kill.current);
				}
				else
				{
					w.Write(0);
				}
			}
		}
		w.Write(-1);
	}

	private void ImportMisc(BinaryReader r)
	{
		r.ReadInt32();
		int num = r.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			m_MissionTrack.Add(r.ReadInt32(), r.ReadBoolean());
		}
	}

	private void ExportMisc(BinaryWriter w)
	{
		w.Write(0);
		w.Write(m_MissionTrack.Count);
		foreach (KeyValuePair<int, bool> item in m_MissionTrack)
		{
			w.Write(item.Key);
			w.Write(item.Value);
		}
	}
}
