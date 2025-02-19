using ScenarioRTL.IO;

namespace ScenarioRTL;

public abstract class StatementObject
{
	private StatementRaw m_Raw;

	public string classname => m_Raw.classname;

	public ParamRaw parameters => m_Raw.parameters;

	public int order => m_Raw.order;

	public Trigger trigger { get; private set; }

	public Mission mission => (trigger != null) ? trigger.mission : null;

	public Scenario scenario => (trigger != null) ? trigger.mission.scenario : null;

	protected VarScope scenarioVars => (trigger != null) ? trigger.mission.scenario.Variables : null;

	protected VarScope missionVars => (trigger != null) ? trigger.mission.scenario.MissionVariables[trigger.mission] : null;

	protected abstract void OnCreate();

	internal void Init(Trigger _trigger, StatementRaw _raw)
	{
		trigger = _trigger;
		m_Raw = _raw;
		OnCreate();
	}

	public override string ToString()
	{
		return classname;
	}
}
