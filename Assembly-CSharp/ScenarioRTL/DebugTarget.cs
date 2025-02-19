using System.Collections.Generic;
using ScenarioRTL.IO;

namespace ScenarioRTL;

public class DebugTarget
{
	public class PostedEvent
	{
		public List<StatementLog> logs = new List<StatementLog>();

		public StatementLog status;
	}

	public class StatementLog
	{
		public string stmt;

		public string log;
	}

	public Dictionary<int, MissionRaw> missionRaws;

	public Dictionary<int, Mission> missionInsts;

	public Dictionary<int, Mission> runningMissions;

	public List<ActionThread> actionThreads;

	public VarScope scenarioVars;

	public VarCollection<Mission> missionVars;
}
