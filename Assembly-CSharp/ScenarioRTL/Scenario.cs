using System;
using System.Collections.Generic;
using System.IO;
using ScenarioRTL.IO;

namespace ScenarioRTL;

public class Scenario
{
	private const int VERSION = 1;

	private bool m_Pause;

	private Dictionary<int, MissionRaw> m_MissionRaws = new Dictionary<int, MissionRaw>();

	private int m_MaxMissionId;

	private Dictionary<int, Mission> m_MissionInsts = new Dictionary<int, Mission>();

	private Dictionary<int, Mission> m_RunningMissions = new Dictionary<int, Mission>();

	private List<ConditionThread> m_ConditionThreads = new List<ConditionThread>();

	private List<ActionThread> m_ActionThreads = new List<ActionThread>();

	private VarScope m_Variables = new VarScope();

	private VarCollection<Mission> m_MissionVariables = new VarCollection<Mission>();

	public int[] runningMissionIds
	{
		get
		{
			int[] array = new int[m_RunningMissions.Count];
			int num = 0;
			foreach (KeyValuePair<int, Mission> runningMission in m_RunningMissions)
			{
				array[num] = runningMission.Key;
				num++;
			}
			return array;
		}
	}

	public VarScope Variables => m_Variables;

	public VarCollection<Mission> MissionVariables => m_MissionVariables;

	public static DebugTarget debugTarget { get; private set; }

	private Scenario()
	{
		Asm.Init();
	}

	public static Scenario Create(string file_path)
	{
		if (!Directory.Exists(file_path))
		{
			return null;
		}
		Scenario scenario = new Scenario();
		scenario.LoadDir(file_path);
		return scenario;
	}

	public bool RunMission(int id)
	{
		if (!m_MissionRaws.ContainsKey(id))
		{
			return false;
		}
		if (m_RunningMissions.ContainsKey(id))
		{
			return false;
		}
		int num = ++m_MaxMissionId;
		Mission mission = new Mission(num, m_MissionRaws[id], this);
		m_MissionInsts.Add(num, mission);
		m_MissionVariables[mission] = m_Variables.CreateChild();
		mission.Init();
		mission.Run();
		m_RunningMissions[id] = mission;
		return true;
	}

	public bool CloseMission(int id)
	{
		if (!m_MissionRaws.ContainsKey(id))
		{
			return false;
		}
		if (!m_RunningMissions.ContainsKey(id))
		{
			return false;
		}
		Mission mission = m_RunningMissions[id];
		mission.Close();
		m_RunningMissions.Remove(id);
		return true;
	}

	public bool IsMissionRunning(int id)
	{
		if (!m_MissionRaws.ContainsKey(id))
		{
			return false;
		}
		if (!m_RunningMissions.ContainsKey(id))
		{
			return false;
		}
		return m_RunningMissions[id].enabled;
	}

	public bool IsMissionActive(int id)
	{
		if (!m_MissionRaws.ContainsKey(id))
		{
			return false;
		}
		return m_RunningMissions.ContainsKey(id);
	}

	public string GetMissionName(int id)
	{
		if (!m_MissionRaws.ContainsKey(id))
		{
			return string.Empty;
		}
		return m_MissionRaws[id].name;
	}

	public ParamRaw GetMissionProperties(int id)
	{
		if (!m_MissionRaws.ContainsKey(id))
		{
			return null;
		}
		return m_MissionRaws[id].properties;
	}

	public void LoadFile(string xmlpath)
	{
		MissionRaw missionRaw = MissionRaw.Create(xmlpath);
		m_MissionRaws[missionRaw.id] = missionRaw;
	}

	public void LoadDir(string dir)
	{
		string[] files = Directory.GetFiles(dir, "*.xml", SearchOption.AllDirectories);
		string[] array = files;
		foreach (string xmlpath in array)
		{
			LoadFile(xmlpath);
		}
	}

	public void Resume()
	{
		m_Pause = false;
		foreach (KeyValuePair<int, Mission> runningMission in m_RunningMissions)
		{
			runningMission.Value.Resume();
		}
	}

	public void Pause()
	{
		m_Pause = true;
		foreach (KeyValuePair<int, Mission> runningMission in m_RunningMissions)
		{
			runningMission.Value.Pause();
		}
	}

	public void Update()
	{
		if (m_Pause)
		{
			return;
		}
		int num = 0;
		while (num < m_ActionThreads.Count)
		{
			m_ActionThreads[num].ProcessAction();
			if (m_ActionThreads[num].isFinished)
			{
				m_ActionThreads.RemoveAt(num);
			}
			else
			{
				num++;
			}
		}
		int num2 = 0;
		while (num2 < m_ConditionThreads.Count)
		{
			ConditionThread conditionThread = m_ConditionThreads[num2];
			bool? flag = conditionThread.Check();
			if (flag.HasValue)
			{
				m_ConditionThreads.RemoveAt(num2);
			}
			else
			{
				num2++;
			}
			if (flag == true)
			{
				conditionThread.Pass();
			}
			else if (flag == false)
			{
				conditionThread.Fail();
			}
		}
	}

	public void Close()
	{
		foreach (KeyValuePair<int, Mission> missionInst in m_MissionInsts)
		{
			if (missionInst.Value != null)
			{
				missionInst.Value.Close();
			}
		}
		m_MissionInsts.Clear();
	}

	public bool Import(byte[] data)
	{
		if (data == null)
		{
			return false;
		}
		using (MemoryStream memoryStream = new MemoryStream(data))
		{
			BinaryReader r = new BinaryReader(memoryStream);
			Import(r);
			memoryStream.Close();
		}
		return true;
	}

	public byte[] Export()
	{
		byte[] array = null;
		using MemoryStream memoryStream = new MemoryStream();
		BinaryWriter w = new BinaryWriter(memoryStream);
		Export(w);
		array = memoryStream.ToArray();
		memoryStream.Close();
		return array;
	}

	public void Import(BinaryReader r)
	{
		int num = r.ReadInt32();
		int num2 = num;
		if (num2 != 1)
		{
			return;
		}
		m_Pause = true;
		int num3 = r.ReadInt32();
		m_MaxMissionId = 0;
		for (int i = 0; i < num3; i++)
		{
			int num4 = r.ReadInt32();
			int key = r.ReadInt32();
			bool flag = r.ReadBoolean();
			if (!m_MissionRaws.ContainsKey(key))
			{
				throw new Exception("Mission raw data of id [" + key + "] is missing");
			}
			Mission mission = new Mission(num4, m_MissionRaws[key], this);
			mission.Init();
			int num5 = r.ReadInt32();
			if (num5 != mission.triggers.Length)
			{
				throw new Exception("Trigger count is not correct");
			}
			for (int j = 0; j < num5; j++)
			{
				mission.triggers[j].Import(r);
			}
			VarScope varScope = Variables.CreateChild();
			varScope.Import(r);
			MissionVariables[mission] = varScope;
			m_MissionInsts.Add(num4, mission);
			if (flag)
			{
				m_RunningMissions.Add(key, mission);
			}
			if (num4 > m_MaxMissionId)
			{
				m_MaxMissionId = num4;
			}
		}
		Variables.Import(r);
		num3 = r.ReadInt32();
		for (int k = 0; k < num3; k++)
		{
			int key2 = r.ReadInt32();
			int key3 = r.ReadInt32();
			int num6 = r.ReadInt32();
			int num7 = r.ReadInt32();
			int cur_idx = r.ReadInt32();
			if (!m_MissionRaws.ContainsKey(key3))
			{
				throw new Exception("Mission raw data of id [" + key3 + "] is missing");
			}
			if (!m_MissionInsts.ContainsKey(key2))
			{
				throw new Exception("Mission instance of instId [" + key3 + "] is missing");
			}
			MissionRaw missionRaw = m_MissionRaws[key3];
			ActionThread actionThread = null;
			Mission mission2 = m_MissionInsts[key2];
			mission2.triggers[num6].FillActionCache(num7);
			actionThread = new ActionThread(mission2.triggers[num6], num6, num7, missionRaw.triggers[num6].actions[num7], mission2.triggers[num6].GetActionCache(num7), cur_idx);
			mission2.triggers[num6].RegisterActionThreadEvent(actionThread);
			AddActionThread(actionThread);
			actionThread.CreateCurrAction();
			if (actionThread.currAction != null)
			{
				actionThread.currAction.RestoreState(r);
			}
		}
	}

	public void Export(BinaryWriter w)
	{
		w.Write(1);
		w.Write(m_MissionInsts.Count);
		foreach (KeyValuePair<int, Mission> missionInst in m_MissionInsts)
		{
			w.Write(missionInst.Value.instId);
			w.Write(missionInst.Value.dataId);
			w.Write(missionInst.Value.enabled);
			w.Write(missionInst.Value.triggers.Length);
			Trigger[] triggers = missionInst.Value.triggers;
			foreach (Trigger trigger in triggers)
			{
				trigger.Export(w);
			}
			MissionVariables[missionInst.Value].Export(w);
		}
		Variables.Export(w);
		w.Write(m_ActionThreads.Count);
		foreach (ActionThread actionThread in m_ActionThreads)
		{
			w.Write(actionThread.trigger.mission.instId);
			w.Write(actionThread.trigger.mission.dataId);
			w.Write(actionThread.triggerIndex);
			w.Write(actionThread.group);
			w.Write(actionThread.currIndex);
			if (actionThread.currAction != null)
			{
				actionThread.currAction.StoreState(w);
			}
		}
	}

	internal void AddActionThread(ActionThread thread)
	{
		m_ActionThreads.Add(thread);
	}

	internal void AddConditionThread(ConditionThread thread)
	{
		m_ConditionThreads.Add(thread);
	}

	public void SetAsDebugTarget()
	{
		debugTarget = new DebugTarget();
		debugTarget.missionRaws = m_MissionRaws;
		debugTarget.missionInsts = m_MissionInsts;
		debugTarget.runningMissions = m_RunningMissions;
		debugTarget.actionThreads = m_ActionThreads;
		debugTarget.scenarioVars = Variables;
		debugTarget.missionVars = MissionVariables;
	}
}
